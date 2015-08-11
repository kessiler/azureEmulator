#region

using Azure.Messages;
using Azure.Messages.Parsers;

#endregion

namespace Azure.HabboHotel.Achievements.Composer
{
    /// <summary>
    /// Class AchievementUnlockedComposer.
    /// </summary>
    internal class AchievementUnlockedComposer
    {
        /// <summary>
        /// Composes the specified achievement.
        /// </summary>
        /// <param name="Achievement">The achievement.</param>
        /// <param name="Level">The level.</param>
        /// <param name="PointReward">The point reward.</param>
        /// <param name="PixelReward">The pixel reward.</param>
        /// <returns>ServerMessage.</returns>
        internal static ServerMessage Compose(Achievement Achievement, int Level, int PointReward, int PixelReward)
        {
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("UnlockAchievementMessageComposer"));
            serverMessage.AppendInteger(Achievement.Id);
            serverMessage.AppendInteger(Level);
            serverMessage.AppendInteger(144);
            serverMessage.AppendString(string.Format("{0}{1}", Achievement.GroupName, Level));
            serverMessage.AppendInteger(PointReward);
            serverMessage.AppendInteger(PixelReward);
            serverMessage.AppendInteger(0);
            serverMessage.AppendInteger(10);
            serverMessage.AppendInteger(21);
            serverMessage.AppendString(Level > 1 ? string.Format("{0}{1}", Achievement.GroupName, (Level - 1)) : string.Empty);

            serverMessage.AppendString(Achievement.Category);
            serverMessage.AppendBool(true);
            return serverMessage;
        }
    }
}