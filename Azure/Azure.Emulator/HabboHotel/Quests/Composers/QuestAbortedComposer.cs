#region

using Azure.Messages;
using Azure.Messages.Parsers;

#endregion

namespace Azure.HabboHotel.Quests.Composer
{
    /// <summary>
    /// Class QuestAbortedComposer.
    /// </summary>
    internal class QuestAbortedComposer
    {
        /// <summary>
        /// Composes this instance.
        /// </summary>
        /// <returns>ServerMessage.</returns>
        internal static ServerMessage Compose()
        {
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("QuestAbortedMessageComposer"));
            serverMessage.AppendBool(false);
            return serverMessage;
        }
    }
}