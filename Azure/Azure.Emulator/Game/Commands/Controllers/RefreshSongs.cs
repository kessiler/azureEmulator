using Azure.Game.Commands.Interfaces;
using Azure.Game.GameClients.Interfaces;
using Azure.Game.SoundMachine;

namespace Azure.Game.Commands.Controllers
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
            SoundMachineSongManager.Initialize();
            session.SendNotif(Azure.GetLanguage().GetVar("command_refresh_songs"));
            return true;
        }
    }
}