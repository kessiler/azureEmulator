#region

using System;
using System.Collections.Generic;
using System.Linq;
using Azure.Database.Manager.Database.Session_Details.Interfaces;
using Azure.HabboHotel.Achievements.Composers;
using Azure.HabboHotel.Achievements.Factorys;
using Azure.HabboHotel.Achievements.Interfaces;
using Azure.HabboHotel.GameClients.Interfaces;
using Azure.Messages;
using Azure.Messages.Parsers;

#endregion

namespace Azure.HabboHotel.Achievements
{
    /// <summary>
    ///     Class AchievementManager.
    /// </summary>
    public class AchievementManager
    {
        /// <summary>
        ///     The achievement data cached
        /// </summary>
        internal ServerMessage AchievementDataCached;

        /// <summary>
        ///     The achievements
        /// </summary>
        internal Dictionary<string, Achievement> Achievements;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AchievementManager" /> class.
        /// </summary>
        /// <param name="dbClient">The database client.</param>
        /// <param name="loadedAchs">The loaded achs.</param>
        internal AchievementManager(IQueryAdapter dbClient, out uint loadedAchs)
        {
            Achievements = new Dictionary<string, Achievement>();
            LoadAchievements(dbClient);
            loadedAchs = (uint)Achievements.Count;
        }

        /// <summary>
        ///     Loads the achievements.
        /// </summary>
        /// <param name="dbClient">The database client.</param>
        internal void LoadAchievements(IQueryAdapter dbClient)
        {
            Achievements.Clear();

            AchievementLevelFactory.GetAchievementLevels(out Achievements, dbClient);

            AchievementDataCached =
                new ServerMessage(LibraryParser.OutgoingRequest("SendAchievementsRequirementsMessageComposer"));
            AchievementDataCached.AppendInteger(Achievements.Count);

            foreach (var ach in Achievements.Values)
            {
                AchievementDataCached.AppendString(ach.GroupName.Replace("ACH_", string.Empty));
                AchievementDataCached.AppendInteger(ach.Levels.Count);

                for (var i = 1; i < ach.Levels.Count + 1; i++)
                {
                    AchievementDataCached.AppendInteger(i);
                    AchievementDataCached.AppendInteger(ach.Levels[i].Requirement);
                }
            }

            AchievementDataCached.AppendInteger(0);
        }

        /// <summary>
        ///     Gets the list.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="message">The message.</param>
        internal void GetList(GameClient session, ClientMessage message)
        {
            session.SendMessage(AchievementListComposer.Compose(session, Achievements.Values.ToList()));
        }

        /// <summary>
        ///     Tries the progress login achievements.
        /// </summary>
        /// <param name="session">The session.</param>
        internal void TryProgressLoginAchievements(GameClient session)
        {
            if (session.GetHabbo() == null)
                return;

            var loginAch = session.GetHabbo().GetAchievementData("ACH_Login");

            if (loginAch == null)
            {
                ProgressUserAchievement(session, "ACH_Login", 1, true);
                return;
            }

            var daysBtwLastLogin = Azure.GetUnixTimeStamp() - session.GetHabbo().PreviousOnline;

            if (daysBtwLastLogin >= 51840 && daysBtwLastLogin <= 112320)
                ProgressUserAchievement(session, "ACH_Login", 1, true);
        }

        /// <summary>
        ///     Tries the progress registration achievements.
        /// </summary>
        /// <param name="session">The session.</param>
        internal void TryProgressRegistrationAchievements(GameClient session)
        {
            if (session.GetHabbo() == null)
                return;

            var regAch = session.GetHabbo().GetAchievementData("ACH_RegistrationDuration");

            if (regAch == null)
            {
                ProgressUserAchievement(session, "ACH_RegistrationDuration", 1, true);
                return;
            }

            if (regAch.Level == 5)
                return;

            double sinceMember = Azure.GetUnixTimeStamp() - (int)session.GetHabbo().CreateDate;

            var daysSinceMember = Convert.ToInt32(Math.Round(sinceMember / 86400));

            if (daysSinceMember == regAch.Progress)
                return;

            var days = daysSinceMember - regAch.Progress;

            if (days < 1)
                return;

            ProgressUserAchievement(session, "ACH_RegistrationDuration", days);
        }

        /// <summary>
        ///     Tries the progress habbo club achievements.
        /// </summary>
        /// <param name="session">The session.</param>
        internal void TryProgressHabboClubAchievements(GameClient session)
        {
            if (session.GetHabbo() == null || !session.GetHabbo().GetSubscriptionManager().HasSubscription)
                return;

            var clubAch = session.GetHabbo().GetAchievementData("ACH_VipHC");

            if (clubAch == null)
            {
                ProgressUserAchievement(session, "ACH_VipHC", 1, true);
                ProgressUserAchievement(session, "ACH_BasicClub", 1, true);
                return;
            }

            if (clubAch.Level == 5)
                return;

            var subscription = session.GetHabbo().GetSubscriptionManager().GetSubscription();

            var sinceActivation = Azure.GetUnixTimeStamp() - subscription.ActivateTime;

            if (sinceActivation < 31556926)
                return;

            if (sinceActivation >= 31556926)
            {
                ProgressUserAchievement(session, "ACH_VipHC", 1);
                ProgressUserAchievement(session, "ACH_BasicClub", 1);
            }

            if (sinceActivation >= 63113851)
            {
                ProgressUserAchievement(session, "ACH_VipHC", 1);
                ProgressUserAchievement(session, "ACH_BasicClub", 1);
            }

            if (sinceActivation >= 94670777)
            {
                ProgressUserAchievement(session, "ACH_VipHC", 1);
                ProgressUserAchievement(session, "ACH_BasicClub", 1);
            }

            if (sinceActivation >= 126227704)
            {
                ProgressUserAchievement(session, "ACH_VipHC", 1);
                ProgressUserAchievement(session, "ACH_BasicClub", 1);
            }

            if (sinceActivation >= 157784630)
            {
                ProgressUserAchievement(session, "ACH_VipHC", 1);
                ProgressUserAchievement(session, "ACH_BasicClub", 1);
            }
        }

        /// <summary>
        ///     Progresses the user achievement.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="achievementGroup">The achievement group.</param>
        /// <param name="progressAmount">The progress amount.</param>
        /// <param name="fromZero">if set to <c>true</c> [from zero].</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool ProgressUserAchievement(GameClient session, string achievementGroup, int progressAmount, bool fromZero = false)
        {
            if (Achievements.ContainsKey(achievementGroup) && session?.GetHabbo() != null)
            {
                var achievement = Achievements[achievementGroup];
                var user = session.GetHabbo();
                var userAchievement = user.GetAchievementData(achievementGroup);

                if (userAchievement == null)
                {
                    userAchievement = new UserAchievement(achievementGroup, 0, 0);
                    user.Achievements.Add(achievementGroup, userAchievement);
                }

                var count = achievement.Levels.Count;

                if (userAchievement.Level == count)
                    return false;

                var acount = (userAchievement.Level + 1);

                if (acount > count)
                    acount = count;

                var targetLevelData = achievement.Levels[acount];

                var achievementColoc = session.GetHabbo().GetAchievementData(achievementGroup);

                if ((achievementColoc != null) && (fromZero))
                    fromZero = false;

                var progress = (fromZero) ? progressAmount : ((userAchievement.Progress + progressAmount));

                var level = userAchievement.Level;
                var num4 = level + 1;

                if (num4 > count)
                    num4 = count;

                if (progress >= targetLevelData.Requirement)
                {
                    level++;
                    num4++;
                    progress = 0;

                    var userBadgeComponent = user.GetBadgeComponent();

                    if (acount != 1)
                        userBadgeComponent.RemoveBadge(Convert.ToString($"{achievementGroup}{acount - 1}"), session);

                    userBadgeComponent.GiveBadge($"{achievementGroup}{acount}", true, session);

                    if (num4 > count)
                        num4 = count;

                    user.ActivityPoints += targetLevelData.RewardPixels;
                    user.NotifyNewPixels(targetLevelData.RewardPixels);
                    user.UpdateActivityPointsBalance();

                    session.SendMessage(AchievementUnlockedComposer.Compose(achievement, acount,
                        targetLevelData.RewardPoints, targetLevelData.RewardPixels));

                    using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
                    {
                        queryReactor.SetQuery(string.Concat("REPLACE INTO users_achievements VALUES (", user.Id, ", @group, ", level, ", ", progress, ")"));
                        queryReactor.AddParameter("group", achievementGroup);
                        queryReactor.RunQuery();
                    }

                    userAchievement.Level = level;
                    userAchievement.Progress = progress;
                    user.AchievementPoints += targetLevelData.RewardPoints;
                    user.NotifyNewPixels(targetLevelData.RewardPixels);
                    user.ActivityPoints += targetLevelData.RewardPixels;
                    user.UpdateActivityPointsBalance();

                    session.SendMessage(AchievementScoreUpdateComposer.Compose(user.AchievementPoints));
                    session.SendMessage(AchievementProgressComposer.Compose(achievement, num4, achievement.Levels[num4],
                        count, user.GetAchievementData(achievementGroup)));

                    Talent talent;

                    if (Azure.GetGame().GetTalentManager().TryGetTalent(achievementGroup, out talent))
                        Azure.GetGame().GetTalentManager().CompleteUserTalent(session, talent);

                    return true;
                }

                userAchievement.Level = level;
                userAchievement.Progress = progress;

                using (var queryreactor2 = Azure.GetDatabaseManager().GetQueryReactor())
                {
                    queryreactor2.SetQuery(string.Concat("REPLACE INTO users_achievements VALUES (", session.GetHabbo().Id, ", @group, ", level, ", ", progress, ")"));
                    queryreactor2.AddParameter("group", achievementGroup);
                    queryreactor2.RunQuery();
                }

                var messageHandler = session.GetMessageHandler();

                if (messageHandler != null)
                {
                    session.SendMessage(AchievementProgressComposer.Compose(achievement, acount, targetLevelData, count, user.GetAchievementData(achievementGroup)));
                    messageHandler.GetResponse().Init(LibraryParser.OutgoingRequest("UpdateUserDataMessageComposer"));
                    messageHandler.GetResponse().AppendInteger(-1);
                    messageHandler.GetResponse().AppendString(user.Look);
                    messageHandler.GetResponse().AppendString(user.Gender.ToLower());
                    messageHandler.GetResponse().AppendString(user.Motto);
                    messageHandler.GetResponse().AppendInteger(user.AchievementPoints);
                    messageHandler.SendResponse();

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Gets the achievement.
        /// </summary>
        /// <param name="achievementGroup">The achievement group.</param>
        /// <returns>Achievement.</returns>
        internal Achievement GetAchievement(string achievementGroup)
        {
            return Achievements.ContainsKey(achievementGroup) ? Achievements[achievementGroup] : new Achievement();
        }
    }
}