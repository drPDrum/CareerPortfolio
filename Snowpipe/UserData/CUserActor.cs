using System.Linq;
using System.Collections.Generic;
using ProjectS.Protocol;
using DataFileEnum;


namespace ProjectS
{
    public class CUserActor
    {
        /// <summary>
        /// Owner UUID
        /// </summary>
        public string Owner { get; set; } = string.Empty;
        public bool IsSetData { get; private set; } = false;
        public CActorData Data { get; private set; } = null;
        #region Server Info
        public actor Info { get; private set; } = null;
        public actor_level_exp LevelInfo { get; private set; } = null;
        public actor_disease DiseaseInfo { get; private set; } = null;
        #endregion Server Info
        public long Key { get; private set; } = 0;
		public int ID { get; private set; } = 0;
        public int Level { get; private set; } = 0;
        public int Exp { get; private set; } = 0;
        public int Awakening { get; private set; } = 0;
        public int SurveyID { get; private set; } = 0;
        public bool IsInSurvey { get { return SurveyID > 0; } }
        public ActorStats BasicStats { get; private set; } = null;

        public List<CActorStatData> ActorLevelStatList { get; private set; } = null;

        public List<CActorActionSetData> BasicActionDatas { get; private set; } = null;
        public int BasicActionLevel { get; private set; } = 0;
        public CActorActionSetData CurBasicAction { get; private set; } = null;

        public List<CActorActionSetData> ActiveActionDatas { get; private set; } = null;
        public int ActiveActionLevel { get; private set; } = 0;
        public CActorActionSetData CurActiveAction { get; private set; } = null;

        public List<CActorPassiveSkillData> PassiveSkillDatas { get; private set; } = null;
        public int PassiveSkillLevel { get; private set; } = 0;
        public CActorPassiveSkillData CurPassiveSkill { get; private set; } = null;

        public CDiseaseData DiseaseData { get; private set; } = null;
        public bool HasDisease { get { return DiseaseData != null; } }

        public long WeaponKey { get; private set; } = 0;
        public long ArmorKey { get; private set; } = 0;
        public long AccessoryKey { get; private set; } = 0;

        public CUserItemEquip Weapon { get; private set; } = null;
        public CUserItemEquip Armor { get; private set; } = null;
        public CUserItemEquip Accessory { get; private set; } = null;

        //TODO : Drum :: 방어 배치 정보
        //public int move_index;
        //public int defense_index;

        //TODO : Drum : Remove this. (Squad 정보를 통해 가져오는 방법으로 진행)
        public int DefenseAreaId { get; private set; } = 0;
        public int DefenseIndex { get; private set; } = 0;        

        //public int attack_damage;
        //public int area_id;

        public CUserActor(string strOwnerUUID, long lKey)
        {
            Owner = strOwnerUUID;
            Key = lKey;
            IsSetData = false;
        }

        public CUserActor(string strOwnerUUID, actor cInfo)
        {
            Owner = strOwnerUUID;
            UpdateInfo(cInfo);
        }

        public CUserActor(actor cInfo)
        {
            Owner = CConstData.MOB_COMMON_UUID;
            UpdateInfo(cInfo);
        }

        public void UpdateInfo(actor cInfo)
        {
            Info = cInfo;

            Key = cInfo.actor_key;
            ID = cInfo.actor_id;
            Exp = cInfo.exp;            
            SurveyID = cInfo.survey_id;

            //Data Set
            if (Data == null || Data.ID != ID)
            {
                Data = Managers.GameData.GetData<CActorData>(ID);
            }

            if(Data == null)
            {
                UnityEngine.Debug.LogError("UserActor Data ERROR : " + ID);
                return;
            }

            WeaponKey = cInfo.weapon_key;
            ArmorKey = cInfo.armor_key;
            AccessoryKey = cInfo.accessory_key;

            RefreshEquipItem();

            RefreshBasicStat(cInfo.level, cInfo.normal_attack_level, cInfo.active_skill_level, cInfo.passive_skill_level, cInfo.awakening);

            RefreshDisease(cInfo.disease_id_1);


            IsSetData = true;
        }

        public void UpdateInfo(actor_level_exp cInfo)
        {
            if (cInfo is null || Key != cInfo.actor_key)
                return;

            LevelInfo = cInfo;

            Exp = cInfo.exp;
            RefreshBasicStat(cInfo.level);
        }

        public void UpdateInfo(actor_disease cInfo)
        {
            if (cInfo is null || Key != cInfo.actor_key)
                return;

            DiseaseInfo = cInfo;

            RefreshDisease(cInfo.disease_id_1);
        }

        public void RefreshEquipItem(Dictionary<long, CUserItemEquip> dicEquipInventory = null)
        {
            if (dicEquipInventory == null)
                dicEquipInventory = UserInfo.Item.EquipItems;

            if(Weapon == null || Weapon.Key != WeaponKey)
            {
                if(Weapon != null)
                    BasicStats.SubStats(Weapon.Stats);

                Weapon = dicEquipInventory.GetOrNull(WeaponKey);
                if (Weapon != null)
                    BasicStats.AddStats(Weapon.Stats);
            }

            if (Armor == null || Armor.Key != ArmorKey)
            {
                if (Armor != null)
                    BasicStats.SubStats(Armor.Stats);

                Armor = dicEquipInventory.GetOrNull(ArmorKey);
                if (Armor != null)
                    BasicStats.AddStats(Armor.Stats);
            }

            if (Accessory == null || Accessory.Key != AccessoryKey)
            {
                if (Accessory != null)
                    BasicStats.SubStats(Accessory.Stats);

                Accessory = dicEquipInventory.GetOrNull(AccessoryKey);
                if (Accessory != null)
                    BasicStats.AddStats(Accessory.Stats);
            }
        }

        /// <summary>
        /// 값이 -1인 경우, 기존 것을 그대로 가겠다는 의미.
        /// </summary>
        public void RefreshBasicStat(int nActorLevel = -1, int nBasicActionLevel = -1, int nActiveActionLevel = -1, 
            int nPassiveSkillLevel = -1, int nAwakening = -1)
        {
            if (BasicStats == null)
                BasicStats = new ActorStats();
            else
                BasicStats.Clear();

            var cMgr = Managers.GameData;

            //Set Level and Level Stat
            if (nActorLevel < 0)
                nActorLevel = Level;

            if (nActorLevel > 0)
            {
                if (ActorLevelStatList == null)
                    ActorLevelStatList = cMgr.GetGroupDataList<CActorStatData>(Data.StatDataGroupId);

                if (ActorLevelStatList != null)
                {
                    if (ActorLevelStatList.CheckIndex(nActorLevel - 1))
                    {
                        BasicStats.AddStats(ActorLevelStatList[nActorLevel - 1]);
                    }
                }
#if USE_LOG
                else
                {
                    UnityEngine.Debug.LogErrorFormat("Stat Data EMPTY with Group Data ID {0} in Actor Data ID {1}", Data.StatDataGroupId, Data.ID);
                }
#endif //USE_LOG
            }

            Level = nActorLevel;

            //Set Basic Action and Basic Action Stat
            if (nBasicActionLevel < 0)
                nBasicActionLevel = BasicActionLevel;
            
            if (nBasicActionLevel > 0)
            {
                if (BasicActionDatas == null)
                    BasicActionDatas = cMgr.GetGroupDataList<CActorActionSetData>(Data.ActionSetDataGroupIdNormalAtk);

                if (BasicActionDatas != null && nBasicActionLevel != BasicActionLevel)
                {
                    if (BasicActionDatas.CheckIndex(nBasicActionLevel - 1))
                        CurBasicAction = BasicActionDatas[nBasicActionLevel - 1];
                }

                if(CurBasicAction != null)
                {
                    //TODO : Drum : Stat
                }
            }

            BasicActionLevel = nBasicActionLevel;

            //Set Active Skill Action and Active Skill Stat
            if (nActiveActionLevel < 0)
                nActiveActionLevel = ActiveActionLevel;

            if (nActiveActionLevel > 0)
            {
                if (ActiveActionDatas == null)
                    ActiveActionDatas = cMgr.GetGroupDataList<CActorActionSetData>(Data.ActionSetDataGroupIdActiveSkill);

                if (ActiveActionDatas != null && nActiveActionLevel != ActiveActionLevel)
                {
                    if (ActiveActionDatas.CheckIndex(nActiveActionLevel - 1))
                        CurActiveAction = BasicActionDatas[nActiveActionLevel - 1];
                }

                if(CurActiveAction != null)
                {
                    //TODO : Drum : Stat
                }
            }

            ActiveActionLevel = nActiveActionLevel;

            //Set Passive Skill and Passive Skil Stat
            if (nPassiveSkillLevel < 0)
                nPassiveSkillLevel = PassiveSkillLevel;

            if(nPassiveSkillLevel > 0)
            {
                if (PassiveSkillDatas == null)
                    PassiveSkillDatas = cMgr.GetGroupDataList<CActorPassiveSkillData>(Data.PassiveSkillGroupId);

                if (PassiveSkillDatas != null && nPassiveSkillLevel != PassiveSkillLevel)
                {
                    if (PassiveSkillDatas.CheckIndex(nPassiveSkillLevel - 1))
                        CurPassiveSkill = PassiveSkillDatas[nPassiveSkillLevel - 1];
                }

                if(CurPassiveSkill != null)
                {
                    //TODO : Drum : Stat
                }
            }

            PassiveSkillLevel = nPassiveSkillLevel;

            //Set Awakening Value and Stat
            if (nAwakening < 0)
                nAwakening = Awakening;

            if (Awakening > 0)
            {
                //TODO : Awakening Data

                //TODO : Drum : Stat
            }

            Awakening = nAwakening;
        }

        public void RefreshDisease(int nDiseaseID)
        {
            if (DiseaseData != null)
            {
                //TODO : Drum : SubStat
            }

            if (nDiseaseID > 0)
            {
                if (DiseaseData == null)
                    DiseaseData = Managers.GameData.GetData<CDiseaseData>(nDiseaseID);
                
                //TODO : Drum : AddStat
            }
        }

        public void ReUse(long lActorKey)
        {
            if(Info == null)
            {
#if USE_LOG
                UnityEngine.Debug.LogError("Invalid function call. Info should be set.");
#endif
                return;
            }

            Info.actor_key = lActorKey;
            LevelInfo = null;
            DiseaseInfo = null;

            Reset();
        }

        /// <summary>
        /// 기본 Info를 기준으로 초기화
        /// </summary>
        public void Reset()
        {
            UpdateInfo(Info);
            UpdateInfo(LevelInfo);
            UpdateInfo(DiseaseInfo);
        }

        // 임시 (@kw)
        public int GetCombatPower()
        {
            return BasicStats.GetStatValue(StatTypes.Atk);
        }

#if USE_LOG
        public override string ToString()
        {
            string strRet = "[CUserActor] :: DataID " + ID;
            strRet += "\n Key : " + Key;

            return strRet;
        }
#endif
    }
}
