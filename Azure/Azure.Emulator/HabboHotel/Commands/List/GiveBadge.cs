#region

using Azure.HabboHotel.GameClients;

#endregion

namespace Azure.HabboHotel.Commands.List
{
    /// <summary>
    /// Class GiveBadge. This class cannot be inherited.
    /// </summary>
    internal sealed class GiveBadge : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GiveBadge"/> class.
        /// </summary>
        public GiveBadge()
        {
            MinRank = 5;
            Description = "Give user a badge.";
            Usage = ":givebadge [USERNAME] [badgeCode]";
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
            client.GetHabbo().GetBadgeComponent().GiveBadge(pms[1], true, client, false);
            session.SendNotif(Azure.GetLanguage().GetVar("command_badge_give_done"));
            Azure.GetGame()
                .GetModerationTool()
                .LogStaffEntry(session.GetHabbo().UserName, client.GetHabbo().UserName,
                    "Badge", string.Format("Badge given to user [{0}]", pms[1]));
            return true;
        }
    }
}