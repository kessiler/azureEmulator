﻿using Azure.HabboHotel.Commands.Interfaces;
using Azure.HabboHotel.GameClients.Interfaces;

namespace Azure.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class RefreshGroups. This class cannot be inherited.
    /// </summary>
    internal sealed class RefreshGroups : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RefreshGroups" /> class.
        /// </summary>
        public RefreshGroups()
        {
            MinRank = 9;
            Description = "Refreshes Groups from Database.";
            Usage = ":refresh_groups";
            MinParams = 0;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            Azure.GetGame().GetGroupManager().InitGroups();
            session.SendNotif(Azure.GetLanguage().GetVar("command_refresh_groups"));
            return true;
        }
    }
}