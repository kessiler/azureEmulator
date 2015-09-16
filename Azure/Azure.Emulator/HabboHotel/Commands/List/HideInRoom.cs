﻿#region

using Azure.HabboHotel.GameClients;

#endregion

namespace Azure.HabboHotel.Commands.List
{
    /// <summary>
    /// Class Sit. This class cannot be inherited.
    /// </summary>
    internal sealed class HideInRoom : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Sit"/> class.
        /// </summary>
        public HideInRoom()
        {
            MinRank = 1;
            Description = "Make users can follow you or not";
            Usage = ":followable";
            MinParams = 0;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            session.GetHabbo().HideInRoom = !session.GetHabbo().HideInRoom;
            return true;
        }
    }
}