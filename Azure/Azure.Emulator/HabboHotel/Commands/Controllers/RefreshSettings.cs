#region

using Azure.Configuration;
using Azure.HabboHotel.Commands.Interfaces;
using Azure.HabboHotel.GameClients.Interfaces;

#endregion

namespace Azure.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class RefreshSettings. This class cannot be inherited.
    /// </summary>
    internal sealed class RefreshSettings : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RefreshSettings" /> class.
        /// </summary>
        public RefreshSettings()
        {
            MinRank = 9;
            Description = "Refreshes Settings from Database.";
            Usage = ":refresh_settings";
            MinParams = 0;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            using (var adapter = Azure.GetDatabaseManager().GetQueryReactor())
                Azure.ConfigData = new ConfigData(adapter);
            session.SendNotif(Azure.GetLanguage().GetVar("command_refresh_settings"));
            return true;
        }
    }
}