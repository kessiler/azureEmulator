﻿using Azure.HabboHotel.Commands.Interfaces;
using Azure.HabboHotel.GameClients.Interfaces;
using Azure.Security;
using Azure.Security.BlackWords;

namespace Azure.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class RefreshBannedHotels. This class cannot be inherited.
    /// </summary>
    internal sealed class RefreshBannedHotels : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RefreshBannedHotels" /> class.
        /// </summary>
        public RefreshBannedHotels()
        {
            MinRank = 9;
            Description = "Refreshes BlackWords filter from Database.";
            Usage = ":refresh_banned_hotels";
            MinParams = 0;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            Filter.Reload();
            BlackWordsManager.Reload();

            session.SendNotif(Azure.GetLanguage().GetVar("command_refresh_banned_hotels"));
            return true;
        }
    }
}