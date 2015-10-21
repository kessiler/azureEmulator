#region

using System;
using Azure.HabboHotel.Commands.Interfaces;
using Azure.HabboHotel.GameClients.Interfaces;
using Azure.Messages;
using Azure.Messages.Parsers;

#endregion

namespace Azure.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class FloodUser. This class cannot be inherited.
    /// </summary>
    internal sealed class FloodUser : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="FloodUser" /> class.
        /// </summary>
        public FloodUser()
        {
            MinRank = 5;
            Description = "Flood user.";
            Usage = ":flood [USERNAME] [TIME]";
            MinParams = 2;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var client = Azure.GetGame().GetClientManager().GetClientByUserName(pms[0]);
            if (client == null)
            {
                session.SendWhisper(Azure.GetLanguage().GetVar("user_not_found"));
                return true;
            }
            if (client.GetHabbo().Rank >= session.GetHabbo().Rank)
            {
                session.SendWhisper(Azure.GetLanguage().GetVar("user_is_higher_rank"));
                return true;
            }
            int time;
            if (!int.TryParse(pms[1], out time))
            {
                session.SendWhisper(Azure.GetLanguage().GetVar("enter_numbers"));
                return true;
            }

            client.GetHabbo().FloodTime = Azure.GetUnixTimeStamp() + Convert.ToInt32(pms[1]);
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("FloodFilterMessageComposer"));
            serverMessage.AppendInteger(Convert.ToInt32(pms[1]));
            client.SendMessage(serverMessage);
            return true;
        }
    }
}