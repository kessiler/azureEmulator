using Azure.Game.Commands.Interfaces;
using Azure.Game.GameClients.Interfaces;

namespace Azure.Game.Commands.Controllers
{
    /// <summary>
    ///     Class RemoveBadge. This class cannot be inherited.
    /// </summary>
    internal sealed class RemoveBadge : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RemoveBadge" /> class.
        /// </summary>
        public RemoveBadge()
        {
            MinRank = 7;
            Description = "Remove the badge from user.";
            Usage = ":removebadge [USERNAME] [badgeId]";
            MinParams = 2;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var client = Azure.GetGame().GetClientManager().GetClientByUserName(pms[0]);
            if (client == null)
            {
                session.SendNotif(Azure.GetLanguage().GetVar("user_not_found"));
                return true;
            }
            if (!client.GetHabbo().GetBadgeComponent().HasBadge(pms[1]))
            {
                session.SendNotif(Azure.GetLanguage().GetVar("command_badge_remove_error"));
                return true;
            }
            client.GetHabbo().GetBadgeComponent().RemoveBadge(pms[1], client);
            session.SendNotif(Azure.GetLanguage().GetVar("command_badge_remove_done"));
            Azure.GetGame()
                .GetModerationTool()
                .LogStaffEntry(session.GetHabbo().UserName, client.GetHabbo().UserName,
                    "Badge Taken", string.Format("Badge taken from user [{0}]", pms[1]));
            return true;
        }
    }
}