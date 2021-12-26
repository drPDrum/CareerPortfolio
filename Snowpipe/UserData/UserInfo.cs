using UnityEngine;
using ProjectS.Protocol;

namespace ProjectS
{
    public static class UserInfo
    {
        public static AccountInfo Account { get; private set; } = new AccountInfo();

        public static GameInfo Game { get; private set; } = new GameInfo();

        public static PlayInfo Play { get; private set; } = new PlayInfo();

        public static ActorInfo Actor { get; private set; } = new ActorInfo();

        public static ItemInfo Item { get; private set; } = new ItemInfo();

        public static ShelterInfo Shelter { get; private set; } = new ShelterInfo();

        public static DeviceInfo Device { get; private set; } = new DeviceInfo();

        public static void Clear()
        {
            Account = new AccountInfo();
            Game = new GameInfo();
            Play = new PlayInfo();
            Actor = new ActorInfo();
            Item = new ItemInfo();
            Device = new DeviceInfo();
            Shelter = new ShelterInfo();
        }

        public static void UpdateInfoCommonResp(CResponseDataBase cResponse)
        {
            if (cResponse == null)
                return;

            //TODO
            //this.Synchronize(cResponse.quests);
        }

        public static void UpdateInfo(getPlayerData.result cInfo)
        {
            //ShelterInfo Setting
            Shelter.UpdateInfo(cInfo.shelter);

            //AccountInfo Setting            
            Account.UpdateInfo(cInfo.user_level_exp);
            Account.UpdateInfo(cInfo.ticket_info);
            Account.UpdateInfo(cInfo.goods_infos);

            Account.UpdateProfileInfo(cInfo.nick_name, cInfo.profile_actor_id);
            Account.UpdateRankInfo((eRankType)cInfo.rank_type, cInfo.trophy);

            //TODO : 서버에 맥스 관련 처리 적용 시 관련 아이템 처리.
            if (cInfo.wall_remain_time > 0)
                Account.SetWallRemainTime(cInfo.wall_remain_time);
            if(cInfo.work_remain_time > 0)
                Account.SetMaxWorkingCount(cInfo.work_remain_time, cInfo.max_work_count);

            //ActorInfo Setting
            Actor.UpdateInfo(cInfo);

            //PlayInfo Setting
            Play.UpdateStoryInfo(cInfo.last_opened_incident_id);

            //ItemInfo Setting
            Item.UpdateInfo(cInfo.inventory);
        }

        public static void UpdateInfo(reward_info cInfo)
        {
            Account.UpdateInfo(cInfo.goods_infos);
            Item.UpdateInfo(cInfo.items);            
            Actor.UpdateInfo(cInfo.actors);
        }

        public static bool UpdateInfo(story_mode_end_info cInfo)
        {
            if (cInfo == null)
                return false;

            UpdateInfo(cInfo.story_first_reward_info);
            UpdateInfo(cInfo.story_reward_info);

            Actor.UpdateInfo(cInfo.actor_level_exps);

            if (cInfo.is_first_clear == 1)
                Play.UpdateStoryInfo(cInfo.open_incident_id);

            return Account.UpdateInfo(cInfo.user_level_exp);
        }


    }
}