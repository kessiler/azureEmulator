﻿using System.Threading.Tasks;
using Azure.HabboHotel.Commands.Interfaces;
using Azure.HabboHotel.GameClients.Interfaces;

namespace Azure.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class Shutdown. This class cannot be inherited.
    /// </summary>
    internal sealed class Shutdown : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Shutdown" /> class.
        /// </summary>
        public Shutdown()
        {
            MinRank = 9;
            Description = "Shutdown the Server.";
            Usage = ":shutdown";
            MinParams = 0;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            Azure.GetGame()
                .GetModerationTool()
                .LogStaffEntry(session.GetHabbo().UserName, string.Empty, "Shutdown",
                    "Issued shutdown command!");
            new Task(Azure.PerformShutDown).Start();
            return true;
        }
    }
}