using Azure.HabboHotel.Commands.Interfaces;
using Azure.HabboHotel.GameClients.Interfaces;

namespace Azure.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class Mute. This class cannot be inherited.
    /// </summary>
    internal sealed class Mute : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Mute" /> class.
        /// </summary>
        public Mute()
        {
            MinRank = 4;
            Description = "Mute a selected user.";
            Usage = ":mute [USERNAME]";
            MinParams = 1;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var client = Azure.GetGame().GetClientManager().GetClientByUserName(pms[0]);
            if (client == null || client.GetHabbo() == null)
            {
                session.SendWhisper(Azure.GetLanguage().GetVar("user_not_found"));
                return true;
            }
            if (client.GetHabbo().Rank >= 4)
            {
                session.SendNotif(Azure.GetLanguage().GetVar("user_is_higher_rank"));
            }
            Azure.GetGame()
                .GetModerationTool().LogStaffEntry(session.GetHabbo().UserName, client.GetHabbo().UserName,
                    "Mute", "Muted user");
            client.GetHabbo().Mute();
            return true;
        }
    }
}