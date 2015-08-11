#region

using Azure.Messages;
using Azure.Messages.Parsers;

#endregion

namespace Azure.HabboHotel.Achievements.Composer
{
    /// <summary>
    /// Class AchievementProgressComposer.
    /// </summary>
    internal class AchievementProgressComposer
    {
        /// <summary>
        /// Composes the specified achievement.
        /// </summary>
        /// <param name="Achievement">The achievement.</param>
        /// <param name="TargetLevel">The target level.</param>
        /// <param name="TargetLevelData">The target level data.</param>
        /// <param name="TotalLevels">The total levels.</param>
        /// <param name="UserData">The user data.</param>
        /// <returns>ServerMessage.</returns>
        internal static ServerMessage Compose(Achievement Achievement, int TargetLevel, AchievementLevel TargetLevelData, int TotalLevels, UserAchievement UserData)
        {
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("AchievementProgressMessageComposer"));
            serverMessage.AppendInteger(Achievement.Id);
            serverMessage.AppendInteger(TargetLevel);
            serverMessage.AppendString(string.Format("{0}{1}", Achievement.GroupName, TargetLevel));
            serverMessage.AppendInteger(TargetLevelData.Requirement);
            serverMessage.AppendInteger(TargetLevelData.Requirement);
            serverMessage.AppendInteger(TargetLevelData.RewardPixels);
            serverMessage.AppendInteger(0);
            serverMessage.AppendInteger(UserData != null ? UserData.Progress : 0);
            serverMessage.AppendBool(UserData != null && UserData.Level >= TotalLevels);
            serverMessage.AppendString(Achievement.Category);
            serverMessage.AppendString(string.Empty);
            serverMessage.AppendInteger(TotalLevels);
            serverMessage.AppendInteger(0);
            return serverMessage;
        }
    }
}