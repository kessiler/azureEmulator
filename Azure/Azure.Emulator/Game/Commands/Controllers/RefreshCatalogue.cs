using Azure.Data;
using Azure.Game.Commands.Interfaces;
using Azure.Game.GameClients.Interfaces;
using Azure.Messages;
using Azure.Messages.Parsers;

namespace Azure.Game.Commands.Controllers
{
    /// <summary>
    ///     Class RefreshCatalogue. This class cannot be inherited.
    /// </summary>
    internal sealed class RefreshCatalogue : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RefreshCatalogue" /> class.
        /// </summary>
        public RefreshCatalogue()
        {
            MinRank = 9;
            Description = "Refreshes Catalogue from Database.";
            Usage = ":refresh_catalogue";
            MinParams = 0;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            using (var adapter = Azure.GetDatabaseManager().GetQueryReactor())
            {
                FurnitureDataManager.SetCache();
                Azure.GetGame().GetItemManager().LoadItems(adapter);
                Azure.GetGame().GetCatalog().Initialize(adapter);
                FurnitureDataManager.Clear();
            }
            Azure.GetGame()
                .GetClientManager()
                .QueueBroadcaseMessage(
                    new ServerMessage(LibraryParser.OutgoingRequest("PublishShopMessageComposer")));
            return true;
        }
    }
}