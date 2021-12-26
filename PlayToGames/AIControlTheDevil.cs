using UnityEngine;
using System;
using System.Collections;

namespace gameRevengers
{
	public class AIControlTheDevil : MonsterControl
	{
		[SerializeField] private float m_fLongTraceDist		= 60f;
		[SerializeField] private float m_fWeaponAttackDist	= 2f;
		[SerializeField] private float m_fFollowSpeed		= 20f;
		[SerializeField] private float m_fTurnMinTime		= 7f;
		[SerializeField] private float m_fTurnMaxTime		= 15f;
		[SerializeField] private float m_fTurnDist			= 15f;

		private EFSMStateID	m_ePrevState = EFSMStateID.None;
		private	float		m_fTurnTime = 0.0f;
		private Vector3		m_vTargetPos = Vector3.zero;

		private int 		m_nAvoidPercent	= 0;
		private int			m_nLoopFireCnt = 4;
		private int			m_nLoopCnt = 0;

		private bool		m_bPhaseAirCalled = false;
		private bool		m_bLanding = false;
		private	bool		m_bAirMode = false;
		private bool		m_bModeChanging = false;
		private int			m_nAirAttackSeq = 0;
		private bool		m_bAirAttacking = false;
		public 	bool 		AirMode	{ get { return m_bAirMode; } }

		private	GameObject	m_goEffPos;
		private GameObject	m_goEffWave;

		private CBossEffect	m_cBossEffect;

		private GameObject	m_goDummyMovePoint;
		private GameObject	m_goEffMoveDust;

		private	Transform	m_TransTurnPivot;
		private	Transform	m_TransMovePoint;
		private	Transform[]	m_TransRoundTarget;
		private	Transform[]	m_TransLandPoint;
		private int			m_nCurSelected = 0;

		private Vector3		m_vFollowEndPos;
		private float		m_fFollowTimer = 1.2f;
		private bool		m_bFollow = false;

		private int 		m_nAniTagRun	= Animator.StringToHash("Move");
		private bool		m_bMoveDust		= false;
		private bool		m_bIsGroggy		= false;
		private bool		m_bCallModeChange = false;

		private bool		m_bNaviOn = false;

		protected override void InitMonster()
		{
			base.InitMonster();
			
			int nAveLevel		= m_cCharacterManager.AVE_LEVEL;
			SMonsterInfo sInfo	= m_cGameManager.GetMonsterInfo(m_nMasterID, nAveLevel);
			
			m_nDefaultHP		= m_nCurHP	= sInfo.nHP;
			m_nDefaultDF		= m_nCurDF	= sInfo.nDef;
			m_nDefaultATK		= m_nCurATK	= sInfo.nAtk;
			
			string[] strModels	= new string[1];
			strModels[0]		= sInfo.strWeaponR;
			
			m_eSpawnType		= ESpawnType.WalkAir;
			m_BaseMonster		= ObjectManager.CreateBossInstance(gameObject, EBossType.Beelzebul, null, m_nDefaultATK);

			m_goEffPos			= CommonUtil.findChildObject(gameObject, ComType.POINT_BREATH);

			m_TransTurnPivot 	= GameObject.Find("BossTurnPivot").transform;
			m_TransMovePoint	= GameObject.Find("BossMovePoint").transform;
			Transform transRound = GameObject.Find("BossAtkPoint").transform;
			if(transRound)
			{
				m_TransRoundTarget = new Transform[transRound.childCount];
				for(int i = 0 ; i < m_TransRoundTarget.Length ; ++i)
				{
					m_TransRoundTarget[i] = transRound.GetChild(i);
				}
			}
			Transform transLand	= GameObject.Find("LandingPoint").transform;
			if(transLand)
			{
				m_TransLandPoint = new Transform[transLand.childCount];
				for(int i = 0 ; i < m_TransLandPoint.Length ; ++i)
				{
					m_TransLandPoint[i] = transLand.GetChild(i);
				}
			}

			Action<string> action = m_cUIManager.GetEvent<string>(KeyValues.EVT_BOSS_NAME);
			if(action != null)
			{
				action.Invoke(m_cUIManager.GetLocalizedText(sInfo.strName));
			}
			else
			{				
				Debug.Log("Action Boss Name is NULL");
			}

			m_cBossEffect		= m_Trans.GetComponent<CBossEffect>();
			m_cBossEffect.SetDamage(m_nDefaultATK);

			m_goDummyMovePoint	= CommonUtil.findChildObject(gameObject, "Bip001 Footsteps");

			m_NavAgent = m_Trans.GetComponent<NavMeshAgent>();
			if (null != m_NavAgent)
				m_NavAgent.enabled = false;

			// caching audio clip
			m_nBgmIndex = 101;
			CSoundMgr.Instance.GetAudioClip(m_nBgmIndex);
		}
		
		protected override void ConstructFSM()
		{
			CAIStatePatrol cPatrol = new CAIStatePatrol(m_Trans, m_fLongTraceDist);
			cPatrol.AddTransition(ETransition.Detected,			EFSMStateID.LongTrace);
			cPatrol.AddTransition(ETransition.Wait,				EFSMStateID.Stop);
			cPatrol.AddTransition(ETransition.NoHP,				EFSMStateID.Dead);
			cPatrol.AddTransition(ETransition.ModeChange,		EFSMStateID.ModeChange);

			CTheDevilChase cTrace = new CTheDevilChase(m_Trans, m_fWeaponAttackDist, m_fLongTraceDist);
			cTrace.AddTransition(ETransition.Lost,				EFSMStateID.Patrol);
			cTrace.AddTransition(ETransition.Fight,				EFSMStateID.Attack);
			cTrace.AddTransition(ETransition.Wait,				EFSMStateID.Stop);
			cTrace.AddTransition(ETransition.NoHP,				EFSMStateID.Dead);
			cTrace.AddTransition(ETransition.ModeChange,		EFSMStateID.ModeChange);

			CTheDevilAttack cAttack = new CTheDevilAttack(m_Trans, m_fWeaponAttackDist);
			cAttack.AddTransition(ETransition.Detected,			EFSMStateID.LongTrace);
			cAttack.AddTransition(ETransition.Wait,				EFSMStateID.Stop);
			cAttack.AddTransition(ETransition.NoHP,				EFSMStateID.Dead);
			cAttack.AddTransition(ETransition.ModeChange,		EFSMStateID.ModeChange);
			cAttack.AddTransition(ETransition.MoveTarget,		EFSMStateID.MoveTarget);

			CTheDevilModeChange cMode = new CTheDevilModeChange(m_Trans);
			cMode.AddTransition(ETransition.TurnPivot,			EFSMStateID.TurnPivot);
			cMode.AddTransition(ETransition.Detected,			EFSMStateID.LongTrace);
			cMode.AddTransition(ETransition.Wait,				EFSMStateID.Stop);
			cMode.AddTransition(ETransition.NoHP,				EFSMStateID.Dead);

			CTheDevilTurnPivot cTurn = new CTheDevilTurnPivot(m_Trans);
			cTurn.AddTransition(ETransition.MoveTarget, 		EFSMStateID.MoveTarget);
			cTurn.AddTransition(ETransition.Wait,				EFSMStateID.Stop);
			cTurn.AddTransition(ETransition.NoHP,				EFSMStateID.Dead);

			CTheDevilMovePoint cMoveTarget = new CTheDevilMovePoint(m_Trans);
			cMoveTarget.AddTransition(ETransition.Fight,		EFSMStateID.Attack);
			cMoveTarget.AddTransition(ETransition.Wait,			EFSMStateID.Stop);
			cMoveTarget.AddTransition(ETransition.NoHP,			EFSMStateID.Dead);
			cMoveTarget.AddTransition(ETransition.ModeChange,	EFSMStateID.ModeChange);

			CTheDevilWait cWait = new CTheDevilWait(m_Trans, m_fLongTraceDist, m_fWeaponAttackDist);
			cWait.AddTransition(ETransition.Lost,				EFSMStateID.Patrol);
			cWait.AddTransition(ETransition.Detected,			EFSMStateID.LongTrace);
			cWait.AddTransition(ETransition.Fight,				EFSMStateID.Attack);
			cWait.AddTransition(ETransition.ModeChange,			EFSMStateID.ModeChange);
			
			CAIStateDead cDead = new CAIStateDead();
			cDead.AddTransition(ETransition.NoHP, 				EFSMStateID.Dead);
			
			AddFSMState(cPatrol);
			AddFSMState(cTrace);
			AddFSMState(cAttack);
			AddFSMState(cMode);
			AddFSMState(cTurn);
			AddFSMState(cMoveTarget);
			AddFSMState(cWait);
			AddFSMState(cDead);
		}
		
		protected override void FSMUpdate()
		{
			m_fElapsedTime += Time.deltaTime;
			
			if (null != m_BaseMonster)
			{
				m_AniStateInfo		= m_Animator.GetCurrentAnimatorStateInfo(0);
				int nAnimNameHash	= m_AniStateInfo.fullPathHash;
				int nAnimTagHash	= m_AniStateInfo.tagHash;
				
				if (null != m_BaseMonster)
					m_BaseMonster.UpdatePerFrame(nAnimNameHash, nAnimTagHash);

				if (nAnimTagHash == m_nAniTagRun)
				{
					if (!m_bMoveDust)
					{
						m_bMoveDust = true;
						MoveEffect(m_bMoveDust);
					}
				}
				else
				{
					if (m_bMoveDust)
					{
						m_bMoveDust = false;
						MoveEffect(m_bMoveDust);
					}
				}
			}

			if (!photonView.isMine)
			{
				m_Trans.position = Vector3.Lerp(m_Trans.position, m_vCurPos, Time.deltaTime * 5f);
				m_Trans.rotation = Quaternion.Slerp(m_Trans.rotation, m_qCurRot, Time.deltaTime * 5f);
			}

			if (m_eState == EFSMStateID.TurnPivot)
			{
				if(m_fElapsedTime > m_fTurnTime)
				{
					CurFSMState.ActEnd();
					m_fElapsedTime = 0.0f;
				}
			}

			if(m_eState == EFSMStateID.MoveTarget
			   || (m_bLanding && m_eState == EFSMStateID.ModeChange))
			{
				float fTargetDist = Vector3.Distance(m_Trans.position, m_vTargetPos);
				if(fTargetDist < 0.5f)
				{
					CurFSMState.ActEnd();
				}
			}

			if (m_bFollow)
			{
				float fDist = Vector3.Distance(m_vFollowEndPos, m_Trans.position);
				if (fDist <= 0.5f)
				{
					m_bFollow = false;
					m_Animator.SetBool("ChkFollow", false);
					m_BaseMonster.SetTargetEnd();
					m_fFollowTimer = 1.2f;
				}
				
				if (m_bFollow)
				{
					m_fFollowTimer -= Time.deltaTime;
					if (0f >= m_fFollowTimer)
					{
						m_Animator.SetBool("ChkFollow", false);
						m_bFollow = false;
						m_BaseMonster.SetTargetEnd();
						m_fFollowTimer = 1.2f;
					}
				}
			}
		}
		
		protected override void FSMFixedUpdate()
		{
			if (m_bFollow)
				m_Rigid.velocity = m_Trans.forward * m_fFollowSpeed;
		}
		
		protected override void LevelScaling(int nLevel)
		{
			SMonsterInfo sInfo = m_cGameManager.GetMonsterInfo(m_nMasterID, nLevel);
			m_nDefaultHP	= m_nCurHP	= sInfo.nHP;
			m_nDefaultDF	= m_nCurDF	= sInfo.nDef;
			m_nDefaultATK	= m_nCurATK	= sInfo.nAtk;

			if (null != m_BaseMonster)
				m_BaseMonster.SetDamage(m_nDefaultATK);

			if (null != m_cBossEffect)
				m_cBossEffect.SetDamage(m_nDefaultATK);
		}

		void MoveEffect(bool bMove)
		{
			if (bMove)
			{
				m_goEffMoveDust = m_cResourceMgr.CreateEffectObject("devil_run", m_goDummyMovePoint.transform.position, m_goDummyMovePoint.transform.rotation);
				if(m_goEffMoveDust)
					m_goEffMoveDust.transform.parent = m_goDummyMovePoint.transform;
			}
			else
			{
				if (m_goEffMoveDust)
					Destroy(m_goEffMoveDust);
			}
		}
		
		#region Photon RPC method
		public override void SetTransition(ETransition eTransition)
		{
			if (m_bPause) return;
			photonView.RPC("RpcTransition", PhotonTargets.All, (int)eTransition);
		}
		
		[PunRPC]
		protected override void RpcTransition(int nTransition, PhotonMessageInfo cMsgInfo)
		{
			PerformTransition((ETransition)nTransition);
		}
		
		public override void Dash (float fStayTime)
		{
			if(m_bAirMode)
			{
				int nIdx = (m_nCurSelected + 3) % 6;
				Debug.Log("SetJump CurIdx : " + m_nCurSelected + " TarIdx : " + nIdx);
				m_vFollowEndPos = m_TransRoundTarget[nIdx].position;

				m_BaseMonster.SetTarget(m_vFollowEndPos, 0.1f);
			}
			else
			{				
				Transform transPlayer = m_cCharacterManager.GetNearestTargetPlayer(m_Trans.position);
				if(transPlayer == null)
				{
					Vector3 vPos = m_TransTurnPivot.position;
					vPos.y = 15.0f;
					m_vFollowEndPos = vPos;
				}
				else
				{
					m_vFollowEndPos = transPlayer.position;
				}

				m_BaseMonster.SetTarget(m_vFollowEndPos, 3f);
			}
			m_bFollow = true;
		}

		public override void Avoid(int nDirection)
		{
			m_BaseMonster.Avoid(nDirection);
			photonView.RPC("RpcAvoid", PhotonTargets.Others, nDirection);
		}
		
		[PunRPC]
		protected override void RpcAvoid(int nDirection, PhotonMessageInfo info)
		{
			m_BaseMonster.Avoid(nDirection);
		}
		
		public override void ChangePhase()
		{
			if (PhotonNetwork.isMasterClient)
				photonView.RPC("RpcChangePhase", PhotonTargets.All);
		}
		
		[PunRPC]
		protected override void RpcChangePhase(PhotonMessageInfo info)
		{
			m_BaseMonster.ChangePhase(m_nPhaseState);

			m_BaseMonster.Pause(true);
			if(m_nPhaseState == 0)
			{
				m_BaseMonster.GetBodyCollider().enabled = false;
			}
		}
		
		public override void ChangePhaseEnd()
		{
			m_bIsGroggy = false;
			m_bPhaseAirCalled = false;
			m_bChangePhase = false;
			m_BaseMonster.ChangePhaseEnd();
			m_BaseMonster.GetBodyCollider().enabled = true;
			m_BaseMonster.Pause(false);

			if (m_nPhaseState == 0)
			{
				m_cGameControl.BossStart();

				SetToggleNavigation(true);

				m_Rigid.useGravity = true;
			}
			else if(m_nPhaseState > 0 && m_bAirMode)
			{
				if(CurFSMStateID == EFSMStateID.Attack)
					SetMoveNext();
			}
			else if(m_nPhaseState > 0 && !m_bAirMode)
			{
				if(CurFSMStateID == EFSMStateID.ModeChange)
					SetLanding();
			}
			
			m_nPhaseState++;
		}

		void SetToggleNavigation(bool bEnable)
		{
			if (m_bNaviOn != bEnable)
			{
				if (!bEnable && m_NavAgent.enabled)
					m_NavAgent.ResetPath();

				m_NavAgent.enabled	= bEnable;
				m_Rigid.isKinematic = bEnable;
				m_bNaviOn = bEnable;
			}
		}

		public override void AttackStart(EAttackType eType, int nAtkNum)
		{
			if (m_bPause) return;
			if (!m_BaseMonster.IsIdle()) return;
			if (!IsChkAttackTime()) return;

			photonView.RPC("RpcAttackStart", PhotonTargets.All, (int)eType, nAtkNum);
			m_fElapsedTime = 0f;
		}

		[PunRPC]
		protected override void RpcAttackStart(int nType, int nAtkNum, PhotonMessageInfo info)
		{
			m_BaseMonster.AttackStart((EAttackType)nType, nAtkNum);
		}

		public override void Attacked(GameObject goAtker, int nDamage, int nDirHit, int nHitAttribute, int nSkillID, int nSkillLevel, int nHittedType)
		{
			if (m_bPause || m_bChangePhase)
				return;

//			PlayAudio(14);
			
			if (null != goAtker)
			{
				PlayerControl cPC = goAtker.GetComponent<PlayerControl>();
				if(cPC)
				{
					int nCombo = cPC.GetCombo();
					nDamage = GetComboDamage(nCombo, nDamage);

					int nAtkerID = cPC.ViewID;
					photonView.RPC("RpcAttacked", PhotonTargets.All, nDamage, nDirHit, nHitAttribute, nAtkerID, nSkillID, nSkillLevel, nHittedType);
				}
				else
				{
					photonView.RPC("RpcAttacked", PhotonTargets.All, nDamage, nDirHit, nHitAttribute, 0, nSkillID, nSkillLevel, nHittedType);
				}

				// Hitted SFX
				int nIndex = UnityEngine.Random.Range(4017, 4022 + 1);
				PlayAudio(nIndex);
			}
		}
		
		[PunRPC]
		protected override void RpcAttacked(int nDamage, int nDirHit, int nHitAttr, int nUserID, int nSkillID, int nSkillLevel, int nHittedType, PhotonMessageInfo cPMInfo)
		{
			bool[] arrHitAct = new bool[ComType.HIT_ATTR_END];
			if (0 != nHitAttr)
				CommonUtil.ConvertNumToArray(nHitAttr, ref arrHitAct);

			float fCalcDamage;
			float fMinDamage = (float)nDamage * 0.1f;
			fCalcDamage = (float)(nDamage - m_nCurDF);
			if (fCalcDamage < fMinDamage)
				fCalcDamage = fMinDamage;
			
			if (arrHitAct[ComType.HIT_ATTR_CRITICAL])
			{
				fCalcDamage = fCalcDamage * 1.3f;
				
				if (m_cHUDDamage)
				{
					m_cHUDDamage.SetText("c\n\n" + ((int)fCalcDamage).ToString(), 1.33f);
				}
			}
			else
			{
				string strText = ((int)fCalcDamage).ToString();
				if (m_cHUDDamage)
					m_cHUDDamage.SetText(strText);
			}

			string strHitEffName	= m_cResourceMgr.GetHitEffect(nHittedType);
			int nIndex = UnityEngine.Random.Range(0, m_TransHitted.Length);
			GameObject objEffect 	= m_cResourceMgr.CreateEffectObject(strHitEffName, m_TransHitted[nIndex].position, m_Trans.rotation);
			if(objEffect)
			{
				
				objEffect.transform.parent = m_TransHitted[nIndex];
				if (0 == nHittedType)	Destroy(objEffect, 0.5f);
				else 					Destroy(objEffect, 2.0f);
			}
			
			if (nSkillID > 0)
			{
				CSkill cSkill = m_cGameManager.GetCachingSkill(nSkillID, nSkillLevel);
				if(cSkill != null)
				{
					SSkillInfo sInfo = cSkill.listSkill[0];
					int nValue = sInfo.nEffectValue;
					ESkillEffect eEffectType = sInfo.eEffect;
					int nTime = sInfo.nDurationTime / 1000;
					SetDebuff((int)EDebuffCategoty.UserAttack, (int)eEffectType, nValue, nTime, nUserID, "flame_hit");
				}
			}
			else
			{
				if (1 == nHittedType)
				{
					SetDebuff((int)EDebuffCategoty.UserAttack, (int)ESkillEffect.DOT, 10, 5, nUserID, "flame_hit");
				}
				else if (2 == nHittedType)
				{
					SetDebuff((int)EDebuffCategoty.UserAttack, (int)ESkillEffect.DOT, 10, 5, nUserID, "debuff_acid");
				}
			}
			
			CheckDamage((int)fCalcDamage, nUserID);

			if(nUserID != 0)
			{
				PlayerControl cPC = m_cCharacterManager.GetPlayer(nUserID);
				cPC.AddCombo();
			}

			if (!m_bIsDead)
			{
				if (ComType.DIR_BACK != nDirHit && !m_bAirMode && !m_bChangePhase)
				{
					int nRandom = UnityEngine.Random.Range(0, 100);
					if (m_nAvoidPercent > nRandom)
					{
						Turn(0);
					}
				}
			}
		}

		#endregion

		protected override void CheckDamage(int nDamage, int nAtkerID)
		{
			m_nCurHP -= nDamage;
			if (0 > m_nCurHP)
				m_nCurHP = 0;
			
			float fRatio = (float)m_nCurHP / (float)m_nDefaultHP;
			Action<float> action = m_cUIManager.GetEvent<float>(KeyValues.EVT_BOSS_GAUGE_VALUE);
			if(action != null)
			{
				action.Invoke(fRatio);
			}
			else
			{
				//Debug.LogError("Event Action is NULL");
			}
						
			if (CheckDie())
			{
				StopAllCoroutines();
				
				if (null != m_cGameManager.DungeonControl)
				{
					m_cGameManager.DungeonControl.m_EventHandler.SetDie(m_nSpawnPointID, m_nSpawnNumber, m_nDropItemID, m_nDropGold, nAtkerID);
					SetItemView(m_nDropItemID);
					SetGoldView(m_nDropGold);
					StartCoroutine(Die());
				}
				return;
			}

			if(m_bAirMode)
				return;

			if(m_nPhaseState == 1)
			{
				if(fRatio < 0.8f)
					m_nPhaseState = 2;
				else if(!m_bIsGroggy && !m_bChangePhase && fRatio < 0.9f)
				{
					m_bIsGroggy = true;
					m_bChangePhase = false;
					ChangePhase();
				}
			}

			if(m_nPhaseState == 2)
			{
				if(fRatio < 0.7f)
					m_nPhaseState = 3;
				else if(!m_bIsGroggy && !m_bChangePhase && fRatio < 0.8f)
				{
					m_bIsGroggy = true;
					m_bChangePhase = false;
					ChangePhase();
				}
			}

			if(m_nPhaseState == 3)
			{
				if(fRatio < 0.6f)
					m_nPhaseState = 4;
				else if(!m_bChangePhase && fRatio < 0.7f)
				{
					Debug.Log("Angry Mode");
					m_bIsGroggy = false;
					m_bChangePhase = true;
					m_nAvoidPercent = 10;
					ChangePhase();
				}
			}

			if(m_nPhaseState == 4)
			{
				if(fRatio < 0.5f)
					m_nPhaseState = 5;
				else if(!m_bIsGroggy && !m_bChangePhase && fRatio < 0.6f)
				{
					m_bIsGroggy = true;
					m_bChangePhase = false;
					m_nAvoidPercent = 10;
					ChangePhase();
				}
			}

			if(m_nPhaseState == 5)
			{
				if(fRatio < 0.4f)
					m_nPhaseState = 6;
				else if(!m_bIsGroggy && !m_bChangePhase && fRatio < 0.5f)
				{
					m_bIsGroggy = true;
					m_bChangePhase = false;
					m_nAvoidPercent = 20;
					ChangePhase();
				}
			}

			if(m_nPhaseState == 6)
			{
				if(fRatio < 0.3f)
					m_nPhaseState = 7;
				else if(!m_bCallModeChange && !m_bChangePhase && fRatio < 0.4f)
				{
					Debug.Log("Angry Mode");
					m_bIsGroggy = false;
					m_bChangePhase = true;
					m_nAvoidPercent = 20;
					ChangePhase();
				}
				else if(!m_bCallModeChange && !m_bPhaseAirCalled && fRatio < 0.45f && !m_bIsGroggy && !m_bChangePhase && !m_bAirMode)
				{
					Debug.Log("Call Air - 1");
					m_bCallModeChange = true;
					m_bPhaseAirCalled = true;

					m_nAirAttackSeq = 1;
					m_nLoopCnt = 3;

					SetTransition(ETransition.ModeChange);
				}
			}

			if(m_nPhaseState == 7)
			{
				if(fRatio < 0.2f)
					m_nPhaseState = 8;
				else if(!m_bCallModeChange && !m_bIsGroggy && !m_bChangePhase && fRatio < 0.3f)
				{
					m_bIsGroggy = true;
					m_bChangePhase = false;
					m_nAvoidPercent = 30;
					ChangePhase();
				}
				else if(!m_bCallModeChange && !m_bPhaseAirCalled && fRatio < 0.35f && !m_bIsGroggy && !m_bChangePhase && !m_bAirMode)
				{
					Debug.Log("Call Air - 2");
					m_bCallModeChange = true;
					m_bPhaseAirCalled = true;

					m_nAirAttackSeq = 2;
					m_nLoopCnt = 2;

					SetTransition(ETransition.ModeChange);
				}
			}

			if(m_nPhaseState == 8)
			{
				if(fRatio < 0.1f)
					m_nPhaseState = 9;
				else if(!m_bCallModeChange && !m_bIsGroggy && !m_bChangePhase && fRatio < 0.2f)
				{
					m_bIsGroggy = true;
					m_bChangePhase = false;
					m_nAvoidPercent = 30;
					ChangePhase();
				}
				else if(!m_bCallModeChange && !m_bPhaseAirCalled && fRatio < 0.25f && !m_bIsGroggy && !m_bChangePhase && !m_bAirMode)
				{
					Debug.Log("Call Air - 3");
					m_bCallModeChange = true;
					m_bPhaseAirCalled = true;

					m_nAirAttackSeq = 0;
					m_nLoopCnt = 0;

					SetTransition(ETransition.ModeChange);
				}
			}

			if(m_nPhaseState == 9)
			{
				if(!m_bCallModeChange && !m_bChangePhase && fRatio < 0.1f)
				{
					Debug.Log("Angry Mode");
					m_bIsGroggy = false;
					m_bChangePhase = true;
					m_nAvoidPercent = 40;
					ChangePhase();
				}
				else if(!m_bCallModeChange && !m_bPhaseAirCalled && fRatio < 0.15f && !m_bIsGroggy && !m_bChangePhase && !m_bAirMode)
				{
					Debug.Log("Call Air - 4");
					m_bCallModeChange = true;
					m_bPhaseAirCalled = true;

					m_nAirAttackSeq = 1;
					m_nLoopCnt = 4;

					SetTransition(ETransition.ModeChange);
				}
			}
		}

		public void SetAirMode(bool bEnable)
		{
			Debug.Log("SetAirMode : Cur : " + m_bAirMode + " Next : " + bEnable);
			if(m_bAirMode == bEnable)
				return;

			m_bModeChanging = true;
			m_eState = CurFSMStateID;

			((CBossBeelzebul)m_BaseMonster).SetAirMode(bEnable);

			m_bAirMode = bEnable;
			if(bEnable)
			{
				m_Animator.SetTrigger("ChkTriModeChange");

			}
			else
			{
				m_Animator.SetTrigger("ChkTriLanding");

				Vector3 vPos;
				if(m_TransLandPoint == null)
				{
					vPos = m_TransTurnPivot.position;
					vPos.y = 15.0f;
				}
				else
				{
					float fMinDist = 9999.0f;
					int nMinIdx = -1;

					for(int i = 0 ; i < m_TransLandPoint.Length ; ++i)
					{
						if(m_TransLandPoint[i] == null)
							continue;

						float fDist = Vector3.Distance(m_Trans.position, m_TransLandPoint[i].position);
						if(fDist < fMinDist)
						{
							fMinDist = fDist;
							nMinIdx = i;
						}
					}
					if(nMinIdx == -1)
					{
						vPos = m_TransTurnPivot.position;
						vPos.y = 15.0f;
					}
					else
					{
						vPos = m_TransLandPoint[nMinIdx].position;
					}
				}
				m_vFollowEndPos = vPos;

				m_BaseMonster.SetTarget(vPos, 3f);

				m_vTargetPos = vPos;

				m_BaseMonster.GetBodyCollider().enabled = false;
				m_bLanding = true;
			}
		}

		public void SetRising()
		{
			Debug.Log("SetRising");
			CurFSMState.ActEnd();
			m_Rigid.useGravity = false;
			SetToggleNavigation(false);
			m_Animator.SetBool("ChkAir", true);
			m_bLanding = false;
		}

		public void SetModeChangeEnd(bool bAirMode)
		{
			m_bCallModeChange = false;
			Debug.Log("SetModeChangeEnd : " + bAirMode);
			m_bModeChanging = false;
			if(bAirMode)
				SetTransition(ETransition.TurnPivot);
			else
				SetTransition(ETransition.Detected);
		}

		public void SetLanding()
		{
			Debug.Log("SetLanding");
			CurFSMState.ActEnd();

			m_BaseMonster.SetTargetEnd();

			m_BaseMonster.GetBodyCollider().enabled = true;
			m_Rigid.useGravity = true;
			SetToggleNavigation(true);
			m_Animator.SetBool("ChkAir", false);

			m_bAirMode = false;
			m_bModeChanging = false;
		}
		
		protected override bool CheckDie()
		{
			if (m_bIsDead)
				return false;
			
			if (0 < m_nCurHP)
				return false;
			
			m_nCurHP = 0;
			m_bIsDead = true;
			
			m_cCharacterManager.DelMonster(m_Trans);
			m_Rigid.useGravity = true;

			SetToggleNavigation(false);

			m_cGameControl.DeleteTarget(gameObject);

			m_BaseMonster.Die();
			
			return true;
		}
		
		IEnumerator Die()
		{
			for (int i = 0; i < m_bChkDebuff.Length; ++i)
				m_bChkDebuff[i] = false;
			
			if (!PhotonNetwork.isMasterClient)
				yield break;
			
			yield return new WaitForSeconds(5f);
			PhotonNetwork.Destroy(gameObject);
		}

		public override void EffectOn(string strKey, bool bFireArrow)
		{
			LongRangeShot(m_nCurATK, strKey, m_goEffPos.transform, EHitEffectAttibute.Fire);
		}

		
		public void SetTurnPivot()
		{
			m_eState = CurFSMStateID;
			
			Vector3 vPos = m_Trans.position;
			vPos.y = m_TransTurnPivot.position.y;

			m_BaseMonster.SetTarget(vPos, 3f);
			m_BaseMonster.PivotRotate(m_TransTurnPivot.position, m_fTurnDist);

			m_fTurnTime = UnityEngine.Random.Range(m_fTurnMinTime, m_fTurnMaxTime);
			m_fElapsedTime = 0.0f;
		}
		
		public void SetTurnPivotEnd()
		{
			m_BaseMonster.PivotRotateEnd();
			SetTransition(ETransition.MoveTarget);
		}

		public void SetMoveTarget()
		{
			if(m_nAirAttackSeq == 0)
			{
				CurFSMState.ActEnd();
				return;
			}

			if(m_bAirAttacking && m_nLoopCnt == 0)
			{
				CurFSMState.ActEnd();
				return;
			}
			else if(m_nLoopCnt == 0)
			{
				Debug.LogError("Loop Count Error.");
				return;
			}

			m_eState = CurFSMStateID;
			CBossBeelzebul cMonster = m_BaseMonster as CBossBeelzebul;

			if(m_nAirAttackSeq == 1)	//Fireball attack
			{
				if(m_bAirAttacking)
				{
					if(cMonster != null)
					{
						int nRand = UnityEngine.Random.Range(0, 2);
						Vector3 vPos;
						if(nRand == 0)
						{
							vPos = m_Trans.right.normalized;
							m_Animator.SetTrigger("ChkTriParryR");
						}
						else
						{
							vPos = -m_Trans.right.normalized;
							m_Animator.SetTrigger("ChkTriParryL");
						}
						vPos *= 5.0f;
						m_vTargetPos = m_Trans.position + vPos;
						cMonster.SetMoveSideStep(m_vTargetPos);
					}
				}
				else
				{
					m_BaseMonster.SetTarget(m_TransMovePoint.position, 1f);
					m_vTargetPos = m_TransMovePoint.position;
				}
			}
			else if(m_nAirAttackSeq == 2)	//Round Attack
			{
				m_Animator.SetBool("ChkGlide", true);
				int nSelected = UnityEngine.Random.Range(0, m_TransRoundTarget.Length);
				if(m_bAirAttacking)
				{					 
					if(nSelected == ((m_nCurSelected + 3) % 6))
					{
						nSelected = m_nCurSelected;
					}
				}
				m_nCurSelected = nSelected;

				cMonster.SetMoveRoundTarget(m_TransTurnPivot.position, m_TransRoundTarget[m_nCurSelected].position);
				m_vTargetPos = m_TransRoundTarget[m_nCurSelected].position;
				Debug.Log("TransTarget Pos : " + m_vTargetPos);
			}
		}

		public void SetMoveTargetEnd()
		{
			Debug.Log("SetMoveTargetEnd()");
			if(m_nAirAttackSeq == 0)
			{
				SetTransition(ETransition.ModeChange);
				return;
			}
			else if(m_nAirAttackSeq == 1)
				m_BaseMonster.SetTargetEnd();
			else if(m_nAirAttackSeq == 2)
			{
				m_Animator.SetBool("ChkGlide", false);
				m_BaseMonster.PivotRotateEnd();
			}

			if(m_bAirAttacking && m_nLoopCnt == 0)
			{
				m_bAirAttacking = false;
				SetTransition(ETransition.ModeChange);
			}
			else if(m_nLoopCnt == 0)
			{
				Debug.LogError("Loop Count Error.");
			}
			else
				SetTransition(ETransition.Fight);
		}

		
		public void AirAttack()
		{
			if(m_bPause) return;

			if(m_nAirAttackSeq == 0)
				return;

			Debug.Log("AirAttackStart");
			m_bAirAttacking = true;
			m_eState = CurFSMStateID;

			if(m_nAirAttackSeq == 1)
			{
				m_BaseMonster.ResetTrigger();

				m_Animator.SetBool("ChkAir", true);
				m_Animator.SetInteger("ChkAttackNum", m_nAirAttackSeq);
				m_Animator.SetTrigger("ChkTriAttack");
//				m_Animator.SetTrigger("ChkTriAttack01");
				m_Animator.SetInteger("FireCnt", m_nLoopFireCnt);
			}
			else if(m_nAirAttackSeq == 2)
			{
				m_BaseMonster.ResetTrigger();

				m_Animator.SetBool("ChkAir", true);
				m_Animator.SetInteger("ChkAttackNum", m_nAirAttackSeq);
				m_Animator.SetTrigger("ChkTriAttack");

				//m_Animator.SetTrigger("ChkTriAttack02");
				m_Animator.SetBool("ChkFollow", true);
			}
		}

		public void SetMoveNext()
		{
			if(m_nLoopCnt > 0)
				--m_nLoopCnt;

			Debug.Log("SetMoveNext");
			CurFSMState.ActEnd();
			SetTransition(ETransition.MoveTarget);
		}

		public void SetAttackRotationTarget()
		{
			int nIndex = (m_nCurSelected + 3) % 6;
			Vector3 vTarget = m_TransRoundTarget[nIndex].position;
			m_BaseMonster.SetTarget(vTarget);
			m_BaseMonster.SetLookAtDirect();
		}

		public override void SetPause(bool bPause)
		{
			if(m_bPause == bPause)
				return;

			m_bPause = bPause;

			m_BaseMonster.Pause(bPause);
			if(bPause)
			{
				m_ePrevState = CurFSMStateID;
				if(m_bAirMode)
				{
					if(m_ePrevState == EFSMStateID.TurnPivot
					   || (m_nAirAttackSeq == 2 && m_ePrevState == EFSMStateID.MoveTarget))
					{
						m_Animator.SetBool("ChkGlide", false);
					}
				}
			}
			else
			{
				m_eState = m_ePrevState;
				if(m_bAirMode)
				{
					if(m_ePrevState == EFSMStateID.TurnPivot
					   || (m_nAirAttackSeq == 2 && m_ePrevState == EFSMStateID.MoveTarget))
					{
						m_Animator.SetBool("ChkGlide", true);
					}
				}
			}

			if(bPause && !m_bAirMode)
				m_BaseMonster.MoveStop();

			if(!bPause && m_nPhaseState > 0 && m_bAirMode && m_bAirAttacking && CurFSMStateID == EFSMStateID.Attack)			
				SetMoveNext();

			if(m_nPhaseState > 0 && !m_bAirMode)
			{
				if(CurFSMStateID == EFSMStateID.ModeChange)
				{
					if(m_bModeChanging)
						SetLanding();
					else
						SetModeChangeEnd(false);
				}
			}


			if (m_bPause)
			{
				if (PhotonNetwork.isMasterClient)
				{
					m_eState = EFSMStateID.None;
				}
			}
		}

		public void WaitState()
		{

		}
	}
}
