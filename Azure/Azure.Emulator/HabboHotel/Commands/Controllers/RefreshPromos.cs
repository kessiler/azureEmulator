#region

using Azure.HabboHotel.Commands.Interfaces;
using Azure.HabboHotel.GameClients.Interfaces;

#endregion

namespace Azure.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class HotelAlert. This class cannot be inherited.
    /// </summary>
    internal sealed class RefreshPromos : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RefreshPromos" /> class.
        /// </summary>
        public RefreshPromos()
        {
            MinRank = 5;
            Description = "Refresh promos cache.";
            Usage = ":refresh_promos";
            MinParams = 0;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            Azure.GetGame().GetHotelView().RefreshPromoList();
            return true;
        }
    }
}