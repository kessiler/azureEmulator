using System.Collections.Generic;
using Azure.HabboHotel.GameClients;
using Azure.Messages;
using Azure.Messages.Parsers;

namespace Azure.HabboHotel.Commands.List
{
    internal sealed class WhisperHotel : Command
    {

        public WhisperHotel()
        {
            MinRank = 7;
            Description = "Susurrar a Todo el Hotel";
            Usage = ":whisperhotel [MESSAGE]";
            MinParams = -1;
        }
        public override bool Execute(GameClient session, string[] pms)
        {
            var message = string.Join(" ", pms);
            if (string.IsNullOrEmpty(message)) return true;
            foreach (GameClient client in Azure.GetGame().GetClientManager().Clients.Values)
            {
                var serverMessage = new ServerMessage();
                serverMessage.Init(LibraryParser.OutgoingRequest("WhisperMessageComposer"));
                serverMessage.AppendInteger(client.CurrentRoomUserId);
                serverMessage.AppendString(message);
                serverMessage.AppendInteger(0);
                serverMessage.AppendInteger(36);
                serverMessage.AppendInteger(0);
                serverMessage.AppendInteger(-1);
                client.SendMessage(serverMessage);
            }
            return true;
        }
    }
}