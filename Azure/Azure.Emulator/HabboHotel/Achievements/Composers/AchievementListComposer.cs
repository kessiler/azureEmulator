using System.Collections.Generic;
using System.Linq;
using Azure.HabboHotel.Achievements.Structs;
using Azure.HabboHotel.GameClients.Interfaces;
using Azure.Messages;
using Azure.Messages.Parsers;

namespace Azure.HabboHotel.Achievements.Composers
{
    /// <summary>
    ///     Class AchievementListComposer.
    /// </summary>
    internal class AchievementListComposer
    {
        /// <summary>
        ///     Composes the specified session.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="achievements">The achievements.</param>
        /// <returns>ServerMessage.</returns>
        internal static ServerMessage Compose(GameClient session, List<Achievement> achievements)
        {
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("AchievementListMessageComposer"));

            serverMessage.AppendInteger(achievements.Count);

            foreach (Achievement achievement in achievements)
            {
                UserAchievement achievementData = session.GetHabbo().GetAchievementData(achievement.GroupName);

                int i = achievementData?.Level + 1 ?? 1;

                int count = achievement.Levels.Count;

                if (i > count)
                    i = count;

                AchievementLevel achievementLevel = achievement.Levels[i];

                AchievementLevel oldLevel = (achievement.Levels.ContainsKey(i - 1)) ? achievement.Levels[i - 1] : achievementLevel;

                serverMessage.AppendInteger(achievement.Id);
                serverMessage.AppendInteger(i);
                serverMessage.AppendString($"{achievement.GroupName}{i}");
                serverMessage.AppendInteger(oldLevel.Requirement);
                serverMessage.AppendInteger(achievementLevel.Requirement);
                serverMessage.AppendInteger(achievementLevel.RewardPoints);
                serverMessage.AppendInteger(0);
                serverMessage.AppendInteger(achievementData?.Progress ?? 0);
                serverMessage.AppendBool(!(achievementData == null || achievementData.Level < count));
                serverMessage.AppendString(achievement.Category);
                serverMessage.AppendString(string.Empty);
                serverMessage.AppendInteger(count);
                serverMessage.AppendInteger(0);
            }

            serverMessage.AppendString(string.Empty);

            return serverMessage;
        }
    }
}