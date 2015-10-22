using Azure.HabboHotel.Commands.Interfaces;
using Azure.HabboHotel.GameClients.Interfaces;

namespace Azure.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class RefreshQuests. This class cannot be inherited.
    /// </summary>
    internal sealed class RefreshQuests : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RefreshQuests" /> class.
        /// </summary>
        public RefreshQuests()
        {
            MinRank = 9;
            Description = "Refreshes navigator from Database.";
            Usage = ":refresh_quests";
            MinParams = 0;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            Azure.GetGame().GetQuestManager().Initialize(Azure.GetDatabaseManager().GetQueryReactor());
            session.SendNotif(Azure.GetLanguage().GetVar("command_refresh_quests"));
            return true;
        }
    }
}