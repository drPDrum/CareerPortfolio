using ProjectS.Protocol;
using System.Collections.Generic;
using DataFileEnum;
using UnityEngine;

namespace ProjectS
{
    public class ActorInfo
    {
        public Dictionary<long, CUserActor> AllUserActors { get; private set; } = new Dictionary<long, CUserActor>();

        public Dictionary<EGameModeType, CUserSquadGroup[]> AllPresets { get; protected set; } = null;

        public int MaxActorCount { get; private set; } = 0;

        #region Actors

        public ActorInfo()
        {
            InitAllPreset();
        }

        #region Update Actor Info

        public void UpdateInfo(getPlayerData.result cInfo)
        {
            UpdateInfo(cInfo.actors);

            MaxActorCount = cInfo.max_actor_cnt;
        }

        public void UpdateInfo(actor[] arrInfos)
        {
            if (arrInfos is null)
                return;

            for(int i = 0 ; i < arrInfos.Length ; ++i)
            {
                var cInfo = arrInfos[i];
                if (cInfo is null)
                    continue;

                if (AllUserActors.ContainsKey(cInfo.actor_key))
                    AllUserActors[cInfo.actor_key].UpdateInfo(cInfo);
                else
                    AllUserActors.Add(cInfo.actor_key, new CUserActor(UserInfo.Game.UUID, cInfo));
            }
        }

        public void UpdateInfo(actor_level_exp[] arrInfos)
        {
            if (arrInfos is null)
                return;

            for(int i = 0 ; i < arrInfos.Length ; ++i)
            {
                var cInfo = arrInfos[i];

                if (cInfo is null)
                    return;

                var cActor = AllUserActors.GetOrNull(cInfo.actor_key);
                if (cActor is null)
                    return;

                cActor.UpdateInfo(cInfo);
            }
        }

        public void UpdateInfo(actor_disease[] arrInfos)
        {
            if (arrInfos is null)
                return;

            for(int i = 0 ; i < arrInfos.Length ; ++i)
            {
                var cInfo = arrInfos[i];
                if (cInfo is null)
                    return;

                var cActor = AllUserActors.GetOrNull(cInfo.actor_key);
                if (cActor is null)
                    return;

                cActor.UpdateInfo(cInfo);
            }
            
        }

        #endregion Update Actor Info

        public CUserActor GetUserActor(long lKey)
        {
            return AllUserActors.GetOrNull(lKey);
        }

        public CUserActor GetUserOrEmptyActor(long lKey)
        {
            if (AllUserActors.ContainsKey(lKey))
                return AllUserActors[lKey];

            var cActor = new CUserActor(UserInfo.Game.UUID, lKey);
            AllUserActors.Add(lKey, cActor);
            return cActor;
        }

        public void ClearActorInfo()
        {
            AllUserActors.Clear();
        }

        #endregion Actors

        #region Squad Presets

        private void InitAllPreset()
        {
            AllPresets = new Dictionary<EGameModeType, CUserSquadGroup[]>();

            foreach (EGameModeType eType in System.Enum.GetValues(typeof(EGameModeType)))
            {
                int nPresetCount = GetPresetCount(eType);
                if (nPresetCount <= 0)
                    continue;

                CUserSquadGroup[] arrPresets = new CUserSquadGroup[nPresetCount];
                for (int i = 0 ; i < arrPresets.Length ; ++i)
                    arrPresets[i] = new CUserSquadGroup(i, eType);

                AllPresets.Add(eType, arrPresets);
            }
        }

        public void UpdateInfo(squad_preset_info[] arrPresetInfo)
        {
            if (arrPresetInfo == null)
                return;

            foreach (var cInfo in arrPresetInfo)
                UpdateInfo(cInfo);
        }

        public void UpdateInfo(squad_preset_info cInfo)
        {
            if (cInfo == null)
                return;

            var eType = (EGameModeType)cInfo.game_mode_type;
            
            if (!AllPresets.ContainsKey(eType))
            {
#if USE_LOG
                Debug.LogError("Preset Initialize ERROR : " + eType);
#endif
                return;
            }

            var arrSquadGroup = AllPresets[eType];
            if (cInfo.preset_index < 0 || cInfo.preset_index >= arrSquadGroup.Length)
            {
#if USE_LOG
                Debug.LogError("Preset Index ERROR : " + cInfo.preset_index);
#endif
                return;
            }

            arrSquadGroup[cInfo.preset_index].UpdateInfo(cInfo.squad_group_info);
        }

        public CUserSquadGroup[] GetSquadGroupPresets(EGameModeType eGameModeType)
        {
            return AllPresets.GetOrNull(eGameModeType);
        }

        public CUserSquadGroup GetSquadGroup(EGameModeType eGameModeType, int nPresetIndex)
        {
            if (nPresetIndex < 0)
                return null;

            var arr = AllPresets.GetOrNull(eGameModeType);
            if (arr.CheckIndex(nPresetIndex))
                return arr[nPresetIndex];

            return null;
        }

        public squad_preset_info[] ToServerInfo(EGameModeType eType)
        {
            if (!AllPresets.ContainsKey(eType))
                return null;
            
            int nPresetCount = GetPresetCount(eType);
            if (nPresetCount <= 0)
                return null;

            squad_preset_info[] arrInfos = new squad_preset_info[nPresetCount];
            for(int i = 0 ; i < arrInfos.Length ; ++i)
            {
                arrInfos[i] = ToServerInfo(eType, i);
            }

            return arrInfos;
        }

        public squad_preset_info ToServerInfo(EGameModeType eType, int nPresetIndex)
        {
            if (!AllPresets.ContainsKey(eType))
                return null;

            
            var arrGroup = AllPresets[eType];
            if (!arrGroup.CheckIndex(nPresetIndex))
                return null;

            return new squad_preset_info()
            {
                preset_index = nPresetIndex,
                game_mode_type = (int)eType,
                squad_group_info = arrGroup[nPresetIndex].ToServerInfo()
            };
        }

        // TODO : Drum : ConstData 혹은 다른 데이터에서 맥스 설정
        private static int GetPresetCount(EGameModeType eType)
        {
            if (eType == EGameModeType.None)
                return 0;

            switch (eType)
            {
                case EGameModeType.NormalBattle: return 5;
                case EGameModeType.Defense: return 1;
                case EGameModeType.Offense: return 1;
                case EGameModeType.Disaster: return 5;
                default: return 0;
            }
        }
        #endregion Squad Presets
    }
}
