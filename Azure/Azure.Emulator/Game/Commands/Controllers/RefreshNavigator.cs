using Azure.Game.Commands.Interfaces;
using Azure.Game.GameClients.Interfaces;

namespace Azure.Game.Commands.Controllers
{
    /// <summary>
    ///     Class RefreshNavigator. This class cannot be inherited.
    /// </summary>
    internal sealed class RefreshNavigator : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RefreshNavigator" /> class.
        /// </summary>
        public RefreshNavigator()
        {
            MinRank = 9;
            Description = "Refreshes navigator from Database.";
            Usage = ":refresh_navigator";
            MinParams = 0;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            using (var adapter = Azure.GetDatabaseManager().GetQueryReactor())
            {
                Azure.GetGame().GetNavigator().Initialize(adapter);
                Azure.GetGame().GetRoomManager().LoadModels(adapter);
            }
            session.SendNotif(Azure.GetLanguage().GetVar("command_refresh_navigator"));
            return true;
        }
    }
}