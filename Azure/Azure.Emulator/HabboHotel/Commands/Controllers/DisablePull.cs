﻿using Azure.HabboHotel.Commands.Interfaces;
using Azure.HabboHotel.GameClients.Interfaces;

namespace Azure.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class Sit. This class cannot be inherited.
    /// </summary>
    internal sealed class DisablePull : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Sit" /> class.
        /// </summary>
        public DisablePull()
        {
            MinRank = -2;
            Description = "Disable/Enable Pull Users in Room";
            Usage = ":disablepull";
            MinParams = 0;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var room = session.GetHabbo().CurrentRoom;
            room.RoomData.DisablePull = !room.RoomData.DisablePull;
            return true;
        }
    }
}