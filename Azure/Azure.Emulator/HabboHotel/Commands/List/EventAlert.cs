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
    internal sealed class EventAlert : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventAlert"/> class.
        /// </summary>
        public EventAlert()
        {
            MinRank = 5;
            Description = "Alerts to all hotel a event.";
            Usage = ":eventha";
            MinParams = -1;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var message = new ServerMessage(LibraryParser.OutgoingRequest("SuperNotificationMessageComposer"));
            message.AppendString("events");
            message.AppendInteger(4);
            message.AppendString("title");
            message.AppendString(Azure.GetLanguage().GetVar("alert_event_title"));
            message.AppendString("message");
            message.AppendString(Azure.GetLanguage().GetVar("alert_event_message").Replace("{username}", session.GetHabbo().UserName).Replace("{extrainfo}", string.Join(" ", pms)));
            message.AppendString("linkUrl");
            message.AppendString("event:navigator/goto/" + session.GetHabbo().CurrentRoomId);
            message.AppendString("linkTitle");
            message.AppendString(Azure.GetLanguage().GetVar("alert_event_goRoom"));

            Azure.GetGame().GetClientManager().QueueBroadcaseMessage(message);
            return true;
        }
    }
}