﻿using Azure.HabboHotel.Commands.Interfaces;
using Azure.HabboHotel.GameClients.Interfaces;

namespace Azure.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class RoomMute. This class cannot be inherited.
    /// </summary>
    internal sealed class RoomMute : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RoomMute" /> class.
        /// </summary>
        public RoomMute()
        {
            MinRank = 5;
            Description = "Mutes the whole room.";
            Usage = ":roommute [reason]";
            MinParams = -1;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var room = session.GetHabbo().CurrentRoom;
            if (room.RoomMuted)
            {
                session.SendWhisper("Room is already muted.");
                return true;
            }

            session.GetHabbo().CurrentRoom.RoomMuted = true;

            /*
            var message = new ServerMessage(LibraryParser.OutgoingRequest("AlertNotificationMessageComposer"));
            message.AppendString(string.Format("The room was muted due to:\r{0}", string.Join(" ", pms)));
            message.AppendString(string.Empty);
            room.SendMessage(message);*/

            room.SendMessage(GameClient.GetBytesNotif(
                $"Este quarto foi silenciado pelo motivo:\r{string.Join(" ", pms)}"));

            Azure.GetGame()
                .GetModerationTool().LogStaffEntry(session.GetHabbo().UserName, string.Empty,
                    "Room Mute", "Room muted");
            return true;
        }
    }
}