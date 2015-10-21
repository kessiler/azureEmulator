using Azure.HabboHotel.Commands.Interfaces;
using Azure.HabboHotel.GameClients.Interfaces;
using Azure.Messages;
using Azure.Messages.Parsers;

namespace Azure.HabboHotel.Commands.Controllers
{
    internal sealed class WhisperRoom : Command
    {
        public WhisperRoom()
        {
            MinRank = 6;
            Description = "Susurrar para o Quarto Todo";
            Usage = ":whisperroom [MESSAGE]";
            MinParams = -1;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var room = session.GetHabbo().CurrentRoom;
            var message = string.Join(" ", pms);
            foreach (var client in Azure.GetGame().GetClientManager().Clients.Values)
            {
                var serverMessage = new ServerMessage();
                serverMessage.Init(LibraryParser.OutgoingRequest("WhisperMessageComposer"));
                serverMessage.AppendInteger(room.RoomId);
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