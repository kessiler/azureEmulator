using System.Collections.Generic;
using Azure.HabboHotel.GameClients;
using Azure.Messages;
using Azure.Messages.Parsers;

namespace Azure.HabboHotel.Commands.List
{
    internal sealed class WhisperRoom : Command
    {

        public WhisperRoom()
        {
            MinRank = 6;
            Description = "Susurrar a Toda la Sala";
            Usage = ":whisperroom [MESSAGE]";
            MinParams = -1;
        }
        public override bool Execute(GameClient session, string[] pms)
        {
            var room = session.GetHabbo().CurrentRoom;
            var message = string.Join(" ", pms);
            foreach (GameClient client in Azure.GetGame().GetClientManager().Clients.Values)
            {
                var serverMessage = new ServerMessage();
                serverMessage.Init(LibraryParser.OutgoingRequest("WhisperMessageComposer"));
                serverMessage.AppendInteger(room.RoomId);
                serverMessage.AppendString(message);
                serverMessage.AppendInteger(0);
                serverMessage.AppendInteger(23);
                serverMessage.AppendInteger(0);
                serverMessage.AppendInteger(-1);
                room.SendMessage(serverMessage);
            }
            return true;
        }
    }
}