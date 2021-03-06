﻿using Azure.HabboHotel.Commands.Interfaces;
using Azure.HabboHotel.GameClients.Interfaces;

namespace Azure.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class MuteBots. This class cannot be inherited.
    /// </summary>
    internal sealed class MuteBots : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MuteBots" /> class.
        /// </summary>
        public MuteBots()
        {
            MinRank = -2;
            Description = "Mute bots in your own room.";
            Usage = ":mutebots";
            MinParams = 0;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var room = session.GetHabbo().CurrentRoom;
            room.MutedBots = !room.MutedBots;
            session.SendNotif(Azure.GetLanguage().GetVar("user_room_mute_bots"));

            return true;
        }
    }
}