#region

using System;
using System.Collections.Generic;
using System.Linq;
using Azure.Database.Manager.Database.Session_Details.Interfaces;
using Azure.HabboHotel.Achievements.Composer;
using Azure.HabboHotel.GameClients;
using Azure.HabboHotel.Users.Subscriptions;
using Azure.Messages;
using Azure.Messages.Parsers;

#endregion

namespace Azure.HabboHotel.Achievements
{
    /// <summary>
    /// Class AchievementManager.
    /// </summary>
    public class AchievementManager
    {
        /// <summary>
        /// The achievements
        /// </summary>
        internal Dictionary<string, Achievement> Achievements;

        /// <summary>
        /// The achievement data cached
        /// </summary>
        internal ServerMessage AchievementDataCached;

        /// <summary>
        /// Initializes a new instance of the <see cref="AchievementManager"/> class.
        /// </summary>
        /// <param name="dbClient">The database client.</param>
        /// <param name="LoadedAchs">The loaded achs.</param>
        internal AchievementManager(IQueryAdapter dbClient, out uint LoadedAchs)
        {
            Achievements = new Dictionary<string, Achievement>();
            LoadAchievements(dbClient);
            LoadedAchs = (uint)Achievements.Count;
        }

        /// <summary>
        /// Loads the achievements.
        /// </summary>
        /// <param name="dbClient">The database client.</param>
        internal void LoadAchievements(IQueryAdapter dbClient)
        {
            Achievements.Clear();
            AchievementLevelFactory.GetAchievementLevels(out Achievements, dbClient);
            AchievementDataCached = new ServerMessage(LibraryParser.OutgoingRequest("SendAchievementsRequirementsMessageComposer"));
            AchievementDataCached.AppendInteger(Achievements.Count);
            foreach (Achievement Ach in Achievements.Values)
            {
                AchievementDataCached.AppendString(Ach.GroupName.Replace("ACH_", ""));
                AchievementDataCached.AppendInteger(Ach.Levels.Count);
                for (int i = 1; i < Ach.Levels.Count + 1; i++)
                {
                    AchievementDataCached.AppendInteger(i);
                    AchievementDataCached.AppendInteger(Ach.Levels[i].Requirement);
                }
            }
            AchievementDataCached.AppendInteger(0);
        }

        /// <summary>
        /// Gets the list.
        /// </summary>
        /// <param name="Session">The session.</param>
        /// <param name="Message">The message.</param>
        internal void GetList(GameClient Session, ClientMessage Message)
        {
            Session.SendMessage(AchievementListComposer.Compose(Session, Achievements.Values.ToList()));
        }

        /// <summary>
        /// Tries the progress login achievements.
        /// </summary>
        /// <param name="Session">The session.</param>
        internal void TryProgressLoginAchievements(GameClient Session)
        {
            if (Session.GetHabbo() == null)
                return;
            UserAchievement loginACH = Session.GetHabbo().GetAchievementData("ACH_Login");
            if (loginACH == null)
            {
                ProgressUserAchievement(Session, "ACH_Login", 1, true);
                return;
            }
            int daysBtwLastLogin = Azure.GetUnixTimeStamp() - Session.GetHabbo().PreviousOnline;
            if (daysBtwLastLogin >= 51840 && daysBtwLastLogin <= 112320)
                ProgressUserAchievement(Session, "ACH_Login", 1, true);
        }

        /// <summary>
        /// Tries the progress registration achievements.
        /// </summary>
        /// <param name="Session">The session.</param>
        internal void TryProgressRegistrationAchievements(GameClient Session)
        {
            if (Session.GetHabbo() == null)
                return;
            UserAchievement regACH = Session.GetHabbo().GetAchievementData("ACH_RegistrationDuration");
            if (regACH == null)
            {
                ProgressUserAchievement(Session, "ACH_RegistrationDuration", 1, true);
                return;
            }
            if (regACH.Level == 5)
                return;
            double sinceMember = Azure.GetUnixTimeStamp() - (int)Session.GetHabbo().CreateDate;
            int daysSinceMember = Convert.ToInt32(Math.Round(sinceMember / 86400));
            if (daysSinceMember == regACH.Progress)
                return;
            int dais = daysSinceMember - regACH.Progress;
            if (dais < 1)
                return;
            ProgressUserAchievement(Session, "ACH_RegistrationDuration", dais, false);
        }

        /// <summary>
        /// Tries the progress habbo club achievements.
        /// </summary>
        /// <param name="Session">The session.</param>
        internal void TryProgressHabboClubAchievements(GameClient Session)
        {
            if (Session.GetHabbo() == null || !Session.GetHabbo().GetSubscriptionManager().HasSubscription)
                return;
            UserAchievement ClubACH = Session.GetHabbo().GetAchievementData("ACH_VipHC");
            if (ClubACH == null)
            {
                ProgressUserAchievement(Session, "ACH_VipHC", 1, true);
                ProgressUserAchievement(Session, "ACH_BasicClub", 1, true);
                return;
            }
            if (ClubACH.Level == 5)
                return;
            Subscription Subscription = Session.GetHabbo().GetSubscriptionManager().GetSubscription();
            int SinceActivation = Azure.GetUnixTimeStamp() - Subscription.ActivateTime;
            if (SinceActivation < 31556926)
                return;
            if (SinceActivation >= 31556926)
            {
                ProgressUserAchievement(Session, "ACH_VipHC", 1, false);
                ProgressUserAchievement(Session, "ACH_BasicClub", 1, false);
            }
            if (SinceActivation >= 63113851)
            {
                ProgressUserAchievement(Session, "ACH_VipHC", 1, false);
                ProgressUserAchievement(Session, "ACH_BasicClub", 1, false);
            }
            if (SinceActivation >= 94670777)
            {
                ProgressUserAchievement(Session, "ACH_VipHC", 1, false);
                ProgressUserAchievement(Session, "ACH_BasicClub", 1, false);
            }
            if (SinceActivation >= 126227704)
            {
                ProgressUserAchievement(Session, "ACH_VipHC", 1, false);
                ProgressUserAchievement(Session, "ACH_BasicClub", 1, false);
            }
            if (SinceActivation >= 157784630)
            {
                ProgressUserAchievement(Session, "ACH_VipHC", 1, false);
                ProgressUserAchievement(Session, "ACH_BasicClub", 1, false);
            }
        }

        /// <summary>
        /// Progresses the user achievement.
        /// </summary>
        /// <param name="Session">The session.</param>
        /// <param name="AchievementGroup">The achievement group.</param>
        /// <param name="ProgressAmount">The progress amount.</param>
        /// <param name="FromZero">if set to <c>true</c> [from zero].</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool ProgressUserAchievement(GameClient Session, string AchievementGroup, int ProgressAmount, bool FromZero = false)
        {
            if (!Achievements.ContainsKey(AchievementGroup) || Session == null)
                return false;
            Achievement achievement = null;
            achievement = Achievements[AchievementGroup];
            UserAchievement userAchievement = Session.GetHabbo().GetAchievementData(AchievementGroup);
            if (userAchievement == null)
            {
                userAchievement = new UserAchievement(AchievementGroup, 0, 0);
                Session.GetHabbo().Achievements.Add(AchievementGroup, userAchievement);
            }
            int count = achievement.Levels.Count;
            if (userAchievement != null && userAchievement.Level == count)
                return false;

            {
                int num = (userAchievement != null) ? (userAchievement.Level + 1) : 1;
                if (num > count)
                    num = count;
                AchievementLevel targetLevelData = achievement.Levels[num];
                int num2 = 0;
                if (FromZero)
                    num2 = ProgressAmount;
                else
                    num2 = ((userAchievement != null) ? (userAchievement.Progress + ProgressAmount) : ProgressAmount);
                int num3 = (userAchievement != null) ? userAchievement.Level : 0;
                int num4 = num3 + 1;
                if (num4 > count)
                    num4 = count;
                if (num2 >= targetLevelData.Requirement)
                {
                    num3++;
                    num4++;
                    int argE60 = num2 - targetLevelData.Requirement;
                    num2 = 0;
                    if (num == 1)
                        Session.GetHabbo().GetBadgeComponent().GiveBadge(string.Format("{0}{1}", AchievementGroup, num), true, Session, false);
                    else
                    {
                        Session.GetHabbo().GetBadgeComponent().RemoveBadge(Convert.ToString(string.Format("{0}{1}", AchievementGroup, num - 1)), Session);
                        Session.GetHabbo().GetBadgeComponent().GiveBadge(string.Format("{0}{1}", AchievementGroup, num), true, Session, false);
                    }
                    if (num4 > count)
                        num4 = count;
                    Session.GetHabbo().ActivityPoints += targetLevelData.RewardPixels;
                    Session.GetHabbo().NotifyNewPixels(targetLevelData.RewardPixels);
                    Session.GetHabbo().UpdateActivityPointsBalance();
                    Session.SendMessage(AchievementUnlockedComposer.Compose(achievement, num, targetLevelData.RewardPoints, targetLevelData.RewardPixels));
                    using (IQueryAdapter queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
                    {
                        queryReactor.SetQuery(string.Concat("REPLACE INTO users_achievements VALUES (", Session.GetHabbo().Id, ", @group, ", num3, ", ", num2, ")"));
                        queryReactor.AddParameter("group", AchievementGroup);
                        queryReactor.RunQuery();
                    }
                    userAchievement.Level = num3;
                    userAchievement.Progress = num2;
                    Session.GetHabbo().AchievementPoints += targetLevelData.RewardPoints;
                    Session.GetHabbo().NotifyNewPixels(targetLevelData.RewardPixels);
                    Session.GetHabbo().ActivityPoints += targetLevelData.RewardPixels;
                    Session.GetHabbo().UpdateActivityPointsBalance();
                    Session.SendMessage(AchievementScoreUpdateComposer.Compose(Session.GetHabbo().AchievementPoints));
                    AchievementLevel targetLevelData2 = achievement.Levels[num4];
                    Session.SendMessage(AchievementProgressComposer.Compose(achievement, num4, targetLevelData2, count, Session.GetHabbo().GetAchievementData(AchievementGroup)));
                    Talent talent = null;
                    if (Azure.GetGame().GetTalentManager().TryGetTalent(AchievementGroup, out talent))
                        Azure.GetGame().GetTalentManager().CompleteUserTalent(Session, talent);
                    return true;
                }
                userAchievement.Level = num3;
                userAchievement.Progress = num2;
                using (IQueryAdapter queryreactor2 = Azure.GetDatabaseManager().GetQueryReactor())
                {
                    queryreactor2.SetQuery(string.Concat("REPLACE INTO users_achievements VALUES (", Session.GetHabbo().Id, ", @group, ", num3, ", ", num2, ")"));
                    queryreactor2.AddParameter("group", AchievementGroup);
                    queryreactor2.RunQuery();
                }

                if (Session == null || Session.GetHabbo() == null) return false;
                Session.SendMessage(AchievementProgressComposer.Compose(achievement, num, targetLevelData, count, Session.GetHabbo().GetAchievementData(AchievementGroup)));
                Session.GetMessageHandler().GetResponse().Init(LibraryParser.OutgoingRequest("UpdateUserDataMessageComposer"));
                Session.GetMessageHandler().GetResponse().AppendInteger(-1);
                Session.GetMessageHandler().GetResponse().AppendString(Session.GetHabbo().Look);
                Session.GetMessageHandler().GetResponse().AppendString(Session.GetHabbo().Gender.ToLower());
                Session.GetMessageHandler().GetResponse().AppendString(Session.GetHabbo().Motto);
                Session.GetMessageHandler().GetResponse().AppendInteger(Session.GetHabbo().AchievementPoints);
                Session.GetMessageHandler().SendResponse();
                return false;
            }
        }

        /// <summary>
        /// Gets the achievement.
        /// </summary>
        /// <param name="achievementGroup">The achievement group.</param>
        /// <returns>Achievement.</returns>
        internal Achievement GetAchievement(string achievementGroup)
        {
            return Achievements.ContainsKey(achievementGroup) ? Achievements[achievementGroup] : null;
        }
    }
}