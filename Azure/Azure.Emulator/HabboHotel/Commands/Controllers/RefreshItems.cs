#region

using Azure.Configuration;
using Azure.HabboHotel.Commands.Interfaces;
using Azure.HabboHotel.GameClients.Interfaces;

#endregion

namespace Azure.HabboHotel.Commands.Controllers
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
            FurniDataParser.SetCache();
            Azure.GetGame().ReloadItems();
            FurniDataParser.Clear();
            session.SendNotif(Azure.GetLanguage().GetVar("command_refresh_items"));
            return true;
        }
    }
}