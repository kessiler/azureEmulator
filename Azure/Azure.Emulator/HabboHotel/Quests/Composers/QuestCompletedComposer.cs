#region

using Azure.HabboHotel.GameClients;
using Azure.Messages;
using Azure.Messages.Parsers;

#endregion

namespace Azure.HabboHotel.Quests.Composer
{
    /// <summary>
    /// Class QuestCompletedComposer.
    /// </summary>
    internal class QuestCompletedComposer
    {
        /// <summary>
        /// Composes the specified session.
        /// </summary>
        /// <param name="Session">The session.</param>
        /// <param name="Quest">The quest.</param>
        /// <returns>ServerMessage.</returns>
        internal static ServerMessage Compose(GameClient Session, Quest Quest)
        {
            int amountOfQuestsInCategory = Azure.GetGame().GetQuestManager().GetAmountOfQuestsInCategory(Quest.Category);
            int i = (Quest == null) ? amountOfQuestsInCategory : Quest.Number;
            int i2 = (Quest == null) ? 0 : Session.GetHabbo().GetQuestProgress(Quest.Id);
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("QuestCompletedMessageComposer"));
            serverMessage.AppendString(Quest.Category);
            serverMessage.AppendInteger(i);
            serverMessage.AppendInteger(Quest.Name.Contains("xmas2012") ? 1 : amountOfQuestsInCategory);
            serverMessage.AppendInteger((Quest == null) ? 3 : Quest.RewardType);
            serverMessage.AppendInteger((Quest == null) ? 0u : Quest.Id);
            serverMessage.AppendBool(Quest != null && Session.GetHabbo().CurrentQuestId == Quest.Id);
            serverMessage.AppendString((Quest == null) ? string.Empty : Quest.ActionName);
            serverMessage.AppendString((Quest == null) ? string.Empty : Quest.DataBit);
            serverMessage.AppendInteger((Quest == null) ? 0 : Quest.Reward);
            serverMessage.AppendString((Quest == null) ? string.Empty : Quest.Name);
            serverMessage.AppendInteger(i2);
            serverMessage.AppendInteger((Quest == null) ? 0u : Quest.GoalData);
            serverMessage.AppendInteger((Quest == null) ? 0 : Quest.TimeUnlock);
            serverMessage.AppendString("");
            serverMessage.AppendString("");
            serverMessage.AppendBool(true);
            serverMessage.AppendBool(true);
            return serverMessage;
        }
    }
}