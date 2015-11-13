using Azure.Game.Commands.Interfaces;
using Azure.Game.GameClients.Interfaces;

namespace Azure.Game.Commands.Controllers
{
    /// <summary>
    ///     Class HotelAlert. This class cannot be inherited.
    /// </summary>
    internal sealed class RefreshExtraThings : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RefreshExtraThings" /> class.
        /// </summary>
        public RefreshExtraThings()
        {
            MinRank = 5;
            Description = "Refresh Extra things cache.";
            Usage = ":refresh_extrathings";
            MinParams = 0;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            Azure.GetGame().GetHallOfFame().RefreshHallOfFame();
            Azure.GetGame().GetRoomManager().GetCompetitionManager().RefreshCompetitions();
            Azure.GetGame().GetTargetedOfferManager().LoadOffer();
            return true;
        }
    }
}