#region

using Azure.HabboHotel.GameClients;
using Azure.HabboHotel.SoundMachine;

#endregion

namespace Azure.HabboHotel.Commands.List
{
    /// <summary>
    /// Class RefreshSongs. This class cannot be inherited.
    /// </summary>
    internal sealed class RefreshSongs : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RefreshSongs"/> class.
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