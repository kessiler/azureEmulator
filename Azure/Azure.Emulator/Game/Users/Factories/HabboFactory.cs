using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Azure.Game.Browser.Interfaces;
using Azure.Game.Groups.Interfaces;

namespace Azure.Game.Users.Factories
{
    /// <summary>
    ///     Class HabboFactory.
    /// </summary>
    internal static class HabboFactory
    {
        /// <summary>
        ///     Generates the habbo.
        /// </summary>
        /// <param name="dRow">The d row.</param>
        /// <param name="mRow">The m row.</param>
        /// <param name="group">The group.</param>
        /// <returns>Habbo.</returns>
        internal static Habbo GenerateHabbo(DataRow dRow, DataRow mRow, HashSet<GroupMember> group)
        {
            uint id = (uint)dRow["id"];
            string userName = (string)dRow["username"];
            string realName = (string)dRow["real_name"];
            uint ras = (uint)dRow["rank"];
            string motto = (string)dRow["motto"];
            string look = (string)dRow["look"];
            string gender = (string)dRow["gender"];
            int lastOnline = int.Parse(dRow["last_online"].ToString());
            int credits = (int)dRow["credits"];
            int activityPoints = (int)dRow["activity_points"];
            double lastActivityPointsUpdate = (double)dRow["activity_points_lastupdate"];
            bool muted = Azure.EnumToBool(dRow["is_muted"].ToString());
            uint homeRoom = (uint)dRow["home_room"];

            int respect = (int)mRow["respect"];
            int dailyRespectPoints = (int)mRow["daily_respect_points"];
            int dailyPetRespectPoints = (int)mRow["daily_pet_respect_points"];
            uint currentQuestId = (uint)mRow["quest_id"];
            int currentQuestProgress = (int)mRow["quest_progress"];
            int achievementPoints = (int)mRow["achievement_score"];
            uint favId = (uint)mRow["favourite_group"];
            int dailyCompetitionVotes = (int)mRow["daily_competition_votes"];

            bool hasFriendRequestsDisabled = Azure.EnumToBool(dRow["block_newfriends"].ToString());
            bool appearOffline = Azure.EnumToBool(dRow["hide_online"].ToString());
            bool hideInRoom = Azure.EnumToBool(dRow["hide_inroom"].ToString());

            bool vip = Azure.EnumToBool(dRow["vip"].ToString());
            double createDate = Convert.ToDouble(dRow["account_created"]);
            bool online = Azure.EnumToBool(dRow["online"].ToString());
            string citizenship = dRow["talent_status"].ToString();
            int diamonds = (int)dRow["diamonds"];
            int lastChange = (int)dRow["last_name_change"];
            int regTimestamp = (int)dRow["account_created"];
            bool tradeLocked = Azure.EnumToBool(dRow["trade_lock"].ToString());
            int tradeLockExpire = (int)dRow["trade_lock_expire"];
            bool nuxPassed = Azure.EnumToBool(dRow["nux_passed"].ToString());

            int buildersExpire = (int)dRow["builders_expire"];
            int buildersItemsMax = (int)dRow["builders_items_max"];
            int buildersItemsUsed = (int)dRow["builders_items_used"];
            int releaseVersion = (int)dRow["release_version"];

            bool onDuty = (bool)dRow["on_duty"];
            uint dutyLevel = (uint)dRow["duty_level"];

            Dictionary<int, UserSearchLog> navilogs = new Dictionary<int, UserSearchLog>();

            string navilogstring = (string)dRow["navigator_logs"];

            if (navilogstring.Length > 0)
                foreach (UserSearchLog naviLogs in navilogstring.Split(';').Where(value => navilogstring.Contains(',')).Select(value => new UserSearchLog(int.Parse(value.Split(',')[0]), value.Split(',')[1], value.Split(',')[2])).Where(naviLogs => !navilogs.ContainsKey(naviLogs.Id)))
                    navilogs.Add(naviLogs.Id, naviLogs);

            return new Habbo(id, userName, realName, ras, motto, look, gender, credits, activityPoints,
                lastActivityPointsUpdate, muted, homeRoom, respect, dailyRespectPoints, dailyPetRespectPoints,
                hasFriendRequestsDisabled, currentQuestId, currentQuestProgress, achievementPoints, regTimestamp,
                lastOnline, appearOffline, hideInRoom, vip, createDate, online, citizenship, diamonds, group, favId,
                lastChange, tradeLocked, tradeLockExpire, nuxPassed, buildersExpire, buildersItemsMax,
                buildersItemsUsed, releaseVersion, onDuty, navilogs, dailyCompetitionVotes, dutyLevel);
        }
    }
}