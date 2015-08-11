#region

using System.Linq;
using Azure.HabboHotel.GameClients;

#endregion

namespace Azure.HabboHotel.Commands.List
{
    /// <summary>
    /// Class SuperBan. This class cannot be inherited.
    /// </summary>
    internal sealed class SuperBan : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SuperBan"/> class.
        /// </summary>
        public SuperBan()
        {
            MinRank = 5;
            Description = "Super ban a user!";
            Usage = ":superban [USERNAME] [REASON]";
            MinParams = -1;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var client = Azure.GetGame().GetClientManager().GetClientByUserName(pms[0]);
            if (client == null)
            {
                session.SendNotif(Azure.GetLanguage().GetVar("user_not_found"));
                return true;
            }

            if (client.GetHabbo().Rank >= session.GetHabbo().Rank)
            {
                session.SendNotif(Azure.GetLanguage().GetVar("user_is_higher_rank"));
                return true;
            }
            Azure.GetGame()
                .GetModerationTool()
                .LogStaffEntry(session.GetHabbo().UserName, client.GetHabbo().UserName, "Ban",
                    "User has received a Super ban.");
            Azure.GetGame()
                .GetBanManager()
                .BanUser(client, session.GetHabbo().UserName, 788922000.0, string.Join(" ", pms.Skip(1)),
                    false, false);
            return true;
        }
    }
}