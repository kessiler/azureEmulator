﻿#region

using System.Linq;
using Azure.HabboHotel.GameClients;

#endregion

namespace Azure.HabboHotel.Commands.List
{
    /// <summary>
    /// Class Alert. This class cannot be inherited.
    /// </summary>
    internal sealed class Alert : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Alert"/> class.
        /// </summary>
        public Alert()
        {
            MinRank = 5;
            Description = "Alerts a User.";
            Usage = ":alert [USERNAME] [MESSAGE]";
            MinParams = -1;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var userName = pms[0];
            var msg = string.Join(" ", pms.Skip(1));

            var client = Azure.GetGame().GetClientManager().GetClientByUserName(userName);
            if (client == null)
            {
                session.SendWhisper(Azure.GetLanguage().GetVar("user_not_found"));
                return true;
            }
            client.SendNotif(string.Format("{0} \r\r-{1}", msg, session.GetHabbo().UserName));
            return true;
        }
    }
}