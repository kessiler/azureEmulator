#region

using System.Collections.Generic;
using Azure.HabboHotel.GameClients;
using Azure.Messages;
using Azure.Messages.Parsers;

#endregion

namespace Azure.HabboHotel.Achievements.Composer
{
    /// <summary>
    /// Class AchievementListComposer.
    /// </summary>
    internal class AchievementListComposer
    {
        /// <summary>
        /// Composes the specified session.
        /// </summary>
        /// <param name="Session">The session.</param>
        /// <param name="Achievements">The achievements.</param>
        /// <returns>ServerMessage.</returns>
        internal static ServerMessage Compose(GameClient Session, List<Achievement> Achievements)
        {
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("AchievementListMessageComposer"));
            serverMessage.AppendInteger(Achievements.Count);
            foreach (Achievement achievement in Achievements)
            {
                UserAchievement achievementData = Session.GetHabbo().GetAchievementData(achievement.GroupName);
                int i = achievementData != null ? (achievementData.Level + 1) : 1;

                int count = achievement.Levels.Count;
                if (i > count)
                    i = count;
                AchievementLevel achievementLevel = achievement.Levels[i];
                AchievementLevel oldLevel = (achievement.Levels.ContainsKey(i - 1)) ? achievement.Levels[i - 1] : achievementLevel;
                serverMessage.AppendInteger(achievement.Id);
                serverMessage.AppendInteger(i);
                serverMessage.AppendString(string.Format("{0}{1}", achievement.GroupName, i));
                serverMessage.AppendInteger(oldLevel.Requirement);
                serverMessage.AppendInteger(achievementLevel.Requirement);
                serverMessage.AppendInteger(achievementLevel.RewardPoints);
                serverMessage.AppendInteger(0);
                serverMessage.AppendInteger(achievementData != null ? achievementData.Progress : 0);
                if (achievementData == null)
                    serverMessage.AppendBool(false);
                else if (achievementData.Level >= count)
                    serverMessage.AppendBool(true);
                else
                    serverMessage.AppendBool(false);
                serverMessage.AppendString(achievement.Category);
                serverMessage.AppendString(string.Empty);
                serverMessage.AppendInteger(count);
                serverMessage.AppendInteger(0);
            }
            serverMessage.AppendString("");
            return serverMessage;
        }
    }
}