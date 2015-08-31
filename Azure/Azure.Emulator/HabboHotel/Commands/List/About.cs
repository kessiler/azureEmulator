using System;
using System.Text;
using Azure.HabboHotel.GameClients;
using Azure.Messages;
using Azure.Messages.Parsers;

namespace Azure.HabboHotel.Commands.List
{
    /// <summary>
    /// Class About. This class cannot be inherited.
    /// </summary>
    internal sealed class About : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="About"/> class.
        /// </summary>
        public About()
        {
            MinRank = 1;
            Description = "Shows information about the server.";
            Usage = ":about";
            MinParams = 0;
        }

        public override bool Execute(GameClient client, string[] pms)
        {
            var message =
                new ServerMessage(LibraryParser.OutgoingRequest("SuperNotificationMessageComposer"));

            message.AppendString("Azure");
            message.AppendInteger(4);
            message.AppendString("title");
            message.AppendString("ManiaEVO 1.2");
            message.AppendString("message");
            var info = new StringBuilder();
            info.Append("<h5><b>ManiaEVO 1.2 - Base: Azure</b><h5></br></br>");
            info.Append("<br />");
            info.AppendFormat(
            "<b><br />Desenvolvedores:</b> <br />iPlezier <br />Claudi0<br />Kessiler <br /><br /> ");
            info.AppendFormat(
            "<b>Créditos:</b> <br />Jamal, Claudio, Boris, Lucca, Antoine, IhToN<br /> <br /> ");
            info.AppendFormat("<b>Estatisticas:</b> <br />");
            var userCount = Azure.GetGame().GetClientManager().Clients.Count;
            var roomsCount = Azure.GetGame().GetRoomManager().LoadedRooms.Count;
            info.AppendFormat("<b>Usuários:</b> {0} em {1}{2}.<br /><br /><br />", userCount, roomsCount,
                (roomsCount == 1) ? " Quarto" : " Quartos");
            message.AppendString(info.ToString());
            message.AppendString("linkUrl");
            message.AppendString("event:");
            message.AppendString("linkTitle");
            message.AppendString("ok");
            client.SendMessage(message);

            return true;
        }
    }
}