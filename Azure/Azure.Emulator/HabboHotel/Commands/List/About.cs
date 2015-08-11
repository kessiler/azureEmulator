#region

using System.Text;
using Azure.HabboHotel.GameClients;
using Azure.Messages;
using Azure.Messages.Parsers;

#endregion

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
            message.AppendString("Azure Emulator Information");
            message.AppendString("message");
            var info = new StringBuilder();
            info.Append("<h5><b>Azure Emulator 2.0 Beta</b><h5></br></br>");
            info.Append("<br />");
            info.AppendFormat(
            "<b>[Developers]</b> <br />- Jamal <br />- Claudio<br />- Boris <br />- Lucca <br />- Antoine <br />- Diesel <br />- Jaden<br />- IhToN<br /> <br /> ");
            info.AppendFormat("<b>[Hotel Statistics]</b> <br />");
            var userCount = Azure.GetGame().GetClientManager().Clients.Count;
            var roomsCount = Azure.GetGame().GetRoomManager().LoadedRooms.Count;
            info.AppendFormat("<b>Users:</b> {0} in {1}{2}.<br /><br /><br />", userCount, roomsCount,
                (roomsCount == 1) ? " Room" : " Rooms");
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