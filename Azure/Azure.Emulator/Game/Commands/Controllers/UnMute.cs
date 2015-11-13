using Azure.Game.Commands.Interfaces;
using Azure.Game.GameClients.Interfaces;

namespace Azure.Game.Commands.Controllers
{
    /// <summary>
    ///     Class UnMute. This class cannot be inherited.
    /// </summary>
    internal sealed class UnMute : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="UnMute" /> class.
        /// </summary>
        public UnMute()
        {
            MinRank = 4;
            Description = "UnMutes the selected user.";
            Usage = ":unmute [USERNAME]";
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
                session.SendWhisper("You are not allowed to mute that user.");
            }

            Azure.GetGame()
                .GetModerationTool().LogStaffEntry(session.GetHabbo().UserName, client.GetHabbo().UserName,
                    "Unmute", "Unmuted user");
            client.GetHabbo().UnMute();
            return true;
        }
    }
}