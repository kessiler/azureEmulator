﻿using Azure.HabboHotel.Commands.Interfaces;
using Azure.HabboHotel.GameClients.Interfaces;
using Azure.Messages;
using Azure.Messages.Parsers;

namespace Azure.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class MassDance. This class cannot be inherited.
    /// </summary>
    internal sealed class MassDance : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MassDance" /> class.
        /// </summary>
        public MassDance()
        {
            MinRank = 7;
            Description = "Enable dance Id for the whole room.";
            Usage = ":massdance [danceId(0 - 4)]";
            MinParams = 1;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            ushort danceId;
            ushort.TryParse(pms[0], out danceId);

            if (danceId > 4)
            {
                session.SendWhisper(Azure.GetLanguage().GetVar("command_dance_wrong_syntax"));
                return true;
            }
            var room = Azure.GetGame().GetRoomManager().GetRoom(session.GetHabbo().CurrentRoomId);
            var roomUsers = room.GetRoomUserManager().GetRoomUsers();

            foreach (var roomUser in roomUsers)
            {
                var message =
                    new ServerMessage(LibraryParser.OutgoingRequest("DanceStatusMessageComposer"));
                message.AppendInteger(roomUser.VirtualId);
                message.AppendInteger(danceId);
                room.SendMessage(message);
                roomUser.DanceId = danceId;
            }
            return true;
        }
    }
}