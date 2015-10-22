using Azure.HabboHotel.Commands.Interfaces;
using Azure.HabboHotel.GameClients.Interfaces;
using Azure.HabboHotel.SoundMachine;

namespace Azure.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class RefreshSongs. This class cannot be inherited.
    /// </summary>
    internal sealed class RefreshSongs : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RefreshSongs" /> class.
        /// </summary>
        public RefreshSongs()
        {
            MinRank = 9;
            Description = "Refreshes Songs from Database.";
            Usage = ":refresh_songs";
            MinParams = 0;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            SongManager.Initialize();
            session.SendNotif(Azure.GetLanguage().GetVar("command_refresh_songs"));
            return true;
        }
    }
}