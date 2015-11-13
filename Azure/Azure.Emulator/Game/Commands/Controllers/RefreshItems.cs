using Azure.Data;
using Azure.Game.Commands.Interfaces;
using Azure.Game.GameClients.Interfaces;

namespace Azure.Game.Commands.Controllers
{
    /// <summary>
    ///     Class RefreshItems. This class cannot be inherited.
    /// </summary>
    internal sealed class RefreshItems : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RefreshItems" /> class.
        /// </summary>
        public RefreshItems()
        {
            MinRank = 9;
            Description = "Refreshes Items from Database.";
            Usage = ":refresh_items";
            MinParams = 0;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            FurnitureDataManager.SetCache();
            Azure.GetGame().ReloadItems();
            FurnitureDataManager.Clear();
            session.SendNotif(Azure.GetLanguage().GetVar("command_refresh_items"));
            return true;
        }
    }
}