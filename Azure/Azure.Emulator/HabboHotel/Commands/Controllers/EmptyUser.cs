﻿using Azure.HabboHotel.Commands.Interfaces;
using Azure.HabboHotel.GameClients.Interfaces;

namespace Azure.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class EmptyUser. This class cannot be inherited.
    /// </summary>
    internal sealed class EmptyUser : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="EmptyUser" /> class.
        /// </summary>
        public EmptyUser()
        {
            MinRank = 7;
            Description = "Clears all the items from a users inventory.";
            Usage = ":empty_user [USERNAME]";
            MinParams = -1;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var client = Azure.GetGame().GetClientManager().GetClientByUserName(pms[0]);
            if (client == null || client.GetHabbo().Rank >= session.GetHabbo().Rank)
            {
                session.SendWhisper(Azure.GetLanguage().GetVar("user_not_found"));
                return true;
            }
            client.GetHabbo().GetInventoryComponent().ClearItems();
            return true;
        }
    }
}