using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Azure.Game.Browser.Interfaces;
using Azure.Game.Groups.Interfaces;
using Azure.IO;

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
            uint ras = (uint)dRow["rank"];
            uint homeRoom = (uint)dRow["home_room"];

            string userName = (string)dRow["username"];
            string realName = (string)dRow["real_name"];
            string motto = (string)dRow["motto"];
            string look = (string)dRow["look"];
            string gender = (string)dRow["gender"];
            string citizenship = dRow["talent_status"].ToString();

            int lastOnline = (int)dRow["last_online"];
            int credits = (int)dRow["credits"];
            int activityPoints = (int)dRow["activity_points"];

            double lastActivityPointsUpdate = (double)dRow["activity_points_lastupdate"];
            double createDate = (double)dRow["account_created"];

            int respect = int.Parse(mRow["respect"].ToString());
            int dailyRespectPoints = int.Parse(mRow["daily_respect_points"].ToString());
            int dailyPetRespectPoints = int.Parse(mRow["daily_pet_respect_points"].ToString());
            int currentQuestId = int.Parse(mRow["quest_id"].ToString());
            int currentQuestProgress = int.Parse(mRow["quest_progress"].ToString());
            int achievementPoints = int.Parse(mRow["achievement_score"].ToString());
            int favId = int.Parse(mRow["favourite_group"].ToString());
            int dailyCompetitionVotes = int.Parse(mRow["daily_competition_votes"].ToString());

            bool hasFriendRequestsDisabled = Azure.EnumToBool(dRow["block_newfriends"].ToString());
            bool appearOffline = Azure.EnumToBool(dRow["hide_online"].ToString());
            bool hideInRoom = Azure.EnumToBool(dRow["hide_inroom"].ToString());
            bool muted = Azure.EnumToBool(dRow["is_muted"].ToString());
            bool vip = Azure.EnumToBool(dRow["vip"].ToString());
            bool online = Azure.EnumToBool(dRow["online"].ToString());
            bool tradeLocked = Azure.EnumToBool(dRow["trade_lock"].ToString());
            bool nuxPassed = Azure.EnumToBool(dRow["nux_passed"].ToString());
            bool onDuty = Azure.EnumToBool(dRow["on_duty"].ToString());

            int diamonds = (int)dRow["diamonds"];
            int lastChange = (int)dRow["last_name_change"];
            int regTimestamp = int.Parse(dRow["account_created"].ToString());
            int tradeLockExpire = (int)dRow["trade_lock_expire"];
            int buildersExpire = (int)dRow["builders_expire"];
            int buildersItemsMax = (int)dRow["builders_items_max"];
            int buildersItemsUsed = (int)dRow["builders_items_used"];
            int releaseVersion = (int)dRow["release_version"];
            int dutyLevel = (int)dRow["duty_level"];

            Dictionary<int, UserSearchLog> navilogs = new Dictionary<int, UserSearchLog>();

            string navilogstring = dRow["navigator_logs"].ToString();

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