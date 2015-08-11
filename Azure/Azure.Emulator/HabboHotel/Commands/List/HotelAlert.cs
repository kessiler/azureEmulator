#region

using Azure.HabboHotel.GameClients;
using Azure.Messages;
using Azure.Messages.Parsers;

#endregion

namespace Azure.HabboHotel.Commands.List
{
    /// <summary>
    /// Class HotelAlert. This class cannot be inherited.
    /// </summary>
    internal sealed class HotelAlert : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HotelAlert"/> class.
        /// </summary>
        public HotelAlert()
        {
            MinRank = 5;
            Description = "Alerts the whole Hotel.";
            Usage = ":ha [MESSAGE]";
            MinParams = -1;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var str = string.Join(" ", pms);
            var message = new ServerMessage(LibraryParser.OutgoingRequest("BroadcastNotifMessageComposer"));
            message.AppendString(string.Format("{0}\r\n- {1}", str, session.GetHabbo().UserName));
            Azure.GetGame().GetClientManager().QueueBroadcaseMessage(message);

            return true;
        }
    }
}