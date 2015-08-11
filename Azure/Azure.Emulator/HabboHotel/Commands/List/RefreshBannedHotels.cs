#region

using Azure.HabboHotel.GameClients;
using Azure.Security;
using Azure.Security.BlackWords;

#endregion

namespace Azure.HabboHotel.Commands.List
{
    /// <summary>
    /// Class RefreshBannedHotels. This class cannot be inherited.
    /// </summary>
    internal sealed class RefreshBannedHotels : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RefreshBannedHotels"/> class.
        /// </summary>
        public RefreshBannedHotels()
        {
            MinRank = 9;
            Description = "Refreshes BlackWords filter from Database.";
            Usage = ":refresh_banned_hotels";
            MinParams = 0;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            Filter.Reload();
            BlackWordsManager.Reload();

            session.SendNotif(Azure.GetLanguage().GetVar("command_refresh_banned_hotels"));
            return true;
        }
    }
}