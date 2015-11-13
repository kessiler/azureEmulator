using System.Linq;
using Azure.Game.Commands.Interfaces;
using Azure.Game.GameClients.Interfaces;

namespace Azure.Game.Commands.Controllers
{
    /// <summary>
    ///     Class MassBadge. This class cannot be inherited.
    /// </summary>
    internal sealed class MassBadge : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MassBadge" /> class.
        /// </summary>
        public MassBadge()
        {
            MinRank = 7;
            Description = "Give Badges to the users that is Online.";
            Usage = ":massbadge [badgeId]";
            MinParams = 1;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            foreach (
                var client in
                    Azure.GetGame()
                        .GetClientManager()
                        .Clients.Values.Where(client => client != null && client.GetHabbo() != null))
                client.GetHabbo().GetBadgeComponent().GiveBadge(pms[0], true, client);

            session.SendNotif(Azure.GetLanguage().GetVar("command_badge_give_done"));
            Azure.GetGame()
                .GetModerationTool()
                .LogStaffEntry(session.GetHabbo().UserName, "All",
                    "Badge", "Badge [" + pms[0] + "] given to all online users ATM");
            return true;
        }
    }
}