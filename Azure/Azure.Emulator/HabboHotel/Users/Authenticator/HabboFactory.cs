using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Azure.HabboHotel.Groups.Interfaces;
using Azure.HabboHotel.Navigators.Interfaces;

namespace Azure.HabboHotel.Users.Authenticator
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
            var id = uint.Parse(dRow["id"].ToString());
            var userName = (string) dRow["username"];
            var realName = (string) dRow["real_name"];
            var ras = uint.Parse(dRow["rank"].ToString());
            var motto = (string) dRow["motto"];
            var look = (string) dRow["look"];
            var gender = (string) dRow["gender"];
            var lastOnline = int.Parse(dRow["last_online"].ToString());
            var credits = (int) dRow["credits"];
            var activityPoints = (int) dRow["activity_points"];
            var lastActivityPointsUpdate = Convert.ToDouble(dRow["activity_points_lastupdate"]);
            var muted = Azure.EnumToBool(dRow["is_muted"].ToString());
            var homeRoom = Convert.ToUInt32(dRow["home_room"]);

            var respect = (int) mRow["respect"];
            var dailyRespectPoints = (int) mRow["daily_respect_points"];
            var dailyPetRespectPoints = (int) mRow["daily_pet_respect_points"];
            var currentQuestId = Convert.ToUInt32(mRow["quest_id"]);
            var currentQuestProgress = (int) mRow["quest_progress"];
            var achievementPoints = (int) mRow["achievement_score"];
            var favId = uint.Parse(mRow["favourite_group"].ToString());
            var dailyCompetitionVotes = (int) mRow["daily_competition_votes"];
            var hasFriendRequestsDisabled = Azure.EnumToBool(dRow["block_newfriends"].ToString());
            var appearOffline = Azure.EnumToBool(dRow["hide_online"].ToString());
            var hideInRoom = Azure.EnumToBool(dRow["hide_inroom"].ToString());
            var vip = Azure.EnumToBool(dRow["vip"].ToString());
            var createDate = Convert.ToDouble(dRow["account_created"]);
            var online = Azure.EnumToBool(dRow["online"].ToString());
            var citizenship = dRow["talent_status"].ToString();
            var diamonds = int.Parse(dRow["diamonds"].ToString());
            var lastChange = (int) dRow["last_name_change"];
            var regTimestamp = int.Parse(dRow["account_created"].ToString());
            var tradeLocked = Azure.EnumToBool(dRow["trade_lock"].ToString());
            var tradeLockExpire = int.Parse(dRow["trade_lock_expire"].ToString());
            var nuxPassed = Azure.EnumToBool(dRow["nux_passed"].ToString());
            var buildersExpire = (int) dRow["builders_expire"];
            var buildersItemsMax = (int) dRow["builders_items_max"];
            var buildersItemsUsed = (int) dRow["builders_items_used"];
            var releaseVersion = (int) dRow["release_version"];
            var onDuty = Convert.ToBoolean(dRow["OnDuty"]);
            var dutyLevel = uint.Parse(dRow["DutyLevel"].ToString());

            var navilogs = new Dictionary<int, NaviLogs>();
            var navilogstring = (string) dRow["navilogs"];

            if (navilogstring.Length > 0)
            {
                foreach (
                    var naviLogs in
                        navilogstring.Split(';')
                            .Where(value => navilogstring.Contains(","))
                            .Select(
                                value =>
                                    new NaviLogs(int.Parse(value.Split(',')[0]), value.Split(',')[1],
                                        value.Split(',')[2]))
                            .Where(naviLogs => !navilogs.ContainsKey(naviLogs.Id)))
                    navilogs.Add(naviLogs.Id, naviLogs);
            }

            return new Habbo(id, userName, realName, ras, motto, look, gender, credits, activityPoints,
                lastActivityPointsUpdate, muted, homeRoom, respect, dailyRespectPoints, dailyPetRespectPoints,
                hasFriendRequestsDisabled, currentQuestId, currentQuestProgress, achievementPoints, regTimestamp,
                lastOnline, appearOffline, hideInRoom, vip, createDate, online, citizenship, diamonds, group, favId,
                lastChange, tradeLocked, tradeLockExpire, nuxPassed, buildersExpire, buildersItemsMax,
                buildersItemsUsed, releaseVersion, onDuty, navilogs, dailyCompetitionVotes, dutyLevel);
        }
    }
}