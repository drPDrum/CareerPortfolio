using UnityEngine;
using System;
using System.Collections.Generic;
using LitJson;


public class SocialGoogle : SocialBase 
{
	private enum EAchievementType  { Standard = 0, Incremental };
	private enum EAchievementState { Unlocked = 0, Revealed, Hidden };
	private enum EIntentCode { Achievement = 1004, Leaderboard = 2004 };

	private struct CAchievementNode
	{
		public string				strID;
		public string				strName;
		public EAchievementState	eState;
		public EAchievementType		eType;
		public int					nCurStep;
		public int					nCurFomattedStep;
		public int					nTotalFormattedStep;
		public int					nAddStep;
	};

	private Dictionary<string, CAchievementNode>	m_dicAchievements = new Dictionary<string, CAchievementNode>();

	private static AndroidJavaObject	m_joCurrent = null;
	private static AndroidJavaObject	m_joAPI = null;

	public override void Clear ()
	{
		base.Clear();
		m_dicAchievements.Clear();
		m_joCurrent = null;
		m_joAPI = null;
		m_bInitialized = false;
	}

	public override void ConnectAPI ()
	{
		if(m_bInitialized)
			return;

		Clear();
		if(m_joCurrent == null)
		{
			m_joCurrent = EAPIManager.Instance.Activity;
			if(m_joCurrent == null)
				return;
		}

		using(AndroidJavaClass jcPlugin = new AndroidJavaClass("com.npicsoft.drum.GPSManager"))
		{
			if(jcPlugin != null)
			{
				m_joAPI = jcPlugin.CallStatic<AndroidJavaObject>("Instance");
				m_joAPI.Call("SetInitial", m_joCurrent, m_joCurrent);
				m_joCurrent.Call("runOnUiThread", new AndroidJavaRunnable(() => { m_joAPI.Call("ConnectAPI"); }));
			}
		}

		if(m_joAPI != null && m_joCurrent != null)
		{
			m_bInitialized = true;
		}
	}

	public override void DisconnectAPI()
	{
		m_joCurrent.Call("runOnUiThread", new AndroidJavaRunnable(() => { m_joAPI.Call("DisconnectAPI"); }));
	}

	public override void OpenAchievement ()
	{
		if(!m_bInitialized)
			return;

		m_joCurrent.Call("runOnUiThread", new AndroidJavaRunnable(() => { m_joAPI.Call("OpenAchievementIntent"); }));
	}

	public override void OpenLeaderboard ()
	{
		if(!m_bInitialized)
			return;

		m_joCurrent.Call("runOnUiThread", new AndroidJavaRunnable(() => { m_joAPI.Call("OpenLeaderboardIntent"); }));
	}

	public override void SetAchievementEnd(string strID)
	{
		if(!m_bInitialized)
			return;		

		if(m_dicAchievements.ContainsKey(strID))
		{
			m_joCurrent.Call("runOnUiThread", new AndroidJavaRunnable(() => { m_joAPI.Call("UnLockAchievement", strID); }));
		}
		else
		{
			SocialManager.Instance.CBUnlockAchievementError(strID);
		}
	}

	public override void SetLeaderBoard(string strID, long lValue)
	{
		if(!m_bInitialized)
			return;

		m_joCurrent.Call("runOnUiThread", new AndroidJavaRunnable(() => { m_joAPI.Call("IncrementLeaderBoard", strID, lValue); }));
	}

	public void CBConnectAPI(string strArg)
	{
		m_joCurrent.Call("runOnUiThread", new AndroidJavaRunnable(() => { m_joAPI.Call("LoadAchievements"); }));
	}

	public void CBLoadAchievements(string strArg)
	{
		if(string.IsNullOrEmpty(strArg))
		{
			Debug.LogError("Achievements Parsing Error!");
			return;
		}
		else
		{
			JsonData jsonAchievements = JsonMapper.ToObject(strArg);
			if(jsonAchievements == null)
				return;

			for(int i = 0 ; i < jsonAchievements["Achievements"].Count ; ++i)
			{
				CAchievementNode sNode = new CAchievementNode();

				sNode.strID = jsonAchievements["Achievements"][i]["ID"].ToString();
				sNode.strName = jsonAchievements["Achievements"][i]["Name"].ToString();
				sNode.eState = (EAchievementState)int.Parse(jsonAchievements["Achievements"][i]["State"].ToString());
				sNode.eType = (EAchievementType)int.Parse(jsonAchievements["Achievements"][i]["Type"].ToString());

				if(sNode.eType == EAchievementType.Incremental)
				{
					sNode.nCurStep = int.Parse(jsonAchievements["Achievements"][i]["CurStep"].ToString());
					sNode.nCurFomattedStep = int.Parse(jsonAchievements["Achievements"][i]["CurFormattedStep"].ToString());
					sNode.nTotalFormattedStep = int.Parse(jsonAchievements["Achievements"][i]["TotalFormattedStep"].ToString());
				}
				else
				{
					sNode.nCurStep = -1;
					sNode.nCurFomattedStep = -1;
					sNode.nTotalFormattedStep = -1;
				}
				m_dicAchievements.Add(sNode.strID, sNode);
			}
		}
	}

	public void CBUnlockAchievement(string strID)
	{
		if(m_dicAchievements.ContainsKey(strID))
		{
			CAchievementNode cNode = m_dicAchievements[strID];
			cNode.eState = EAchievementState.Unlocked;
		}
	}

	public void CBIncrementAchievement(string strID)
	{
		if(m_dicAchievements.ContainsKey(strID))
		{
			CAchievementNode cNode = m_dicAchievements[strID];

			cNode.nCurStep += cNode.nAddStep;
			cNode.nAddStep = 0;
			if(cNode.nCurStep > cNode.nTotalFormattedStep)
			{
				cNode.eState = EAchievementState.Unlocked;
			}
		}
	}
}
