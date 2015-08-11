#region

using System.Collections.Generic;
using Azure.HabboHotel.GameClients;
using Azure.Messages;
using Azure.Messages.Parsers;

#endregion

namespace Azure.HabboHotel.Quests.Composer
{
    /// <summary>
    /// Class QuestListComposer.
    /// </summary>
    internal class QuestListComposer
    {
        /// <summary>
        /// Composes the specified session.
        /// </summary>
        /// <param name="Session">The session.</param>
        /// <param name="Quests">The quests.</param>
        /// <param name="Send">if set to <c>true</c> [send].</param>
        /// <returns>ServerMessage.</returns>
        internal static ServerMessage Compose(GameClient Session, List<Quest> Quests, bool Send)
        {
            var dictionary = new Dictionary<string, int>();
            var dictionary2 = new Dictionary<string, Quest>();
            foreach (Quest current in Quests)
            {
                if (!current.Category.Contains("xmas2012"))
                {
                    if (!dictionary.ContainsKey(current.Category))
                    {
                        dictionary.Add(current.Category, 1);
                        dictionary2.Add(current.Category, null);
                    }
                    if (current.Number >= dictionary[current.Category])
                    {
                        int questProgress = Session.GetHabbo().GetQuestProgress(current.Id);
                        if (Session.GetHabbo().CurrentQuestId != current.Id && questProgress >= current.GoalData)
                        {
                            dictionary[current.Category] = (current.Number + 1);
                        }
                    }
                }
            }
            foreach (Quest current2 in Quests)
            {
                foreach (KeyValuePair<string, int> current3 in dictionary)
                {
                    if (!current2.Category.Contains("xmas2012") && current2.Category == current3.Key && current2.Number == current3.Value)
                    {
                        dictionary2[current3.Key] = current2;
                        break;
                    }
                }
            }
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("QuestListMessageComposer"));
            serverMessage.AppendInteger(dictionary2.Count);
            foreach (KeyValuePair<string, Quest> current4 in dictionary2)
            {
                if (current4.Value != null)
                {
                    SerializeQuest(serverMessage, Session, current4.Value, current4.Key);
                }
            }
            foreach (KeyValuePair<string, Quest> current5 in dictionary2)
            {
                if (current5.Value == null)
                {
                    SerializeQuest(serverMessage, Session, current5.Value, current5.Key);
                }
            }
            serverMessage.AppendBool(Send);
            return serverMessage;
        }

        /// <summary>
        /// Serializes the quest.
        /// </summary>
        /// <param name="Message">The message.</param>
        /// <param name="Session">The session.</param>
        /// <param name="Quest">The quest.</param>
        /// <param name="Category">The category.</param>
        internal static void SerializeQuest(ServerMessage Message, GameClient Session, Quest Quest, string Category)
        {
            if (Message == null || Session == null)
            {
                return;
            }
            int amountOfQuestsInCategory = Azure.GetGame().GetQuestManager().GetAmountOfQuestsInCategory(Category);

            {
                int num = (Quest == null) ? amountOfQuestsInCategory : (Quest.Number - 1);
                int num2 = (Quest == null) ? 0 : Session.GetHabbo().GetQuestProgress(Quest.Id);
                if (Quest != null && Quest.IsCompleted(num2))
                {
                    num++;
                }
                Message.AppendString(Category);
                Message.AppendInteger((Quest == null) ? 0 : (Quest.Category.Contains("xmas2012") ? 0 : num));
                Message.AppendInteger((Quest == null) ? 0 : (Quest.Category.Contains("xmas2012") ? 0 : amountOfQuestsInCategory));
                Message.AppendInteger((Quest == null) ? 3 : Quest.RewardType);
                Message.AppendInteger((Quest == null) ? 0u : Quest.Id);
                Message.AppendBool(Quest != null && Session.GetHabbo().CurrentQuestId == Quest.Id);
                Message.AppendString((Quest == null) ? string.Empty : Quest.ActionName);
                Message.AppendString((Quest == null) ? string.Empty : Quest.DataBit);
                Message.AppendInteger((Quest == null) ? 0 : Quest.Reward);
                Message.AppendString((Quest == null) ? string.Empty : Quest.Name);
                Message.AppendInteger(num2);
                Message.AppendInteger((Quest == null) ? 0u : Quest.GoalData);
                Message.AppendInteger((Quest == null) ? 0 : Quest.TimeUnlock);
                Message.AppendString("");
                Message.AppendString("");
                Message.AppendBool(true);
            }
        }
    }
}