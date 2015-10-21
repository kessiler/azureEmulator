#region

using Azure.HabboHotel.Commands.Interfaces;
using Azure.HabboHotel.GameClients.Interfaces;

#endregion

namespace Azure.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class GiveDuckets. This class cannot be inherited.
    /// </summary>
    internal sealed class GiveDuckets : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GiveDuckets" /> class.
        /// </summary>
        public GiveDuckets()
        {
            MinRank = 5;
            Description = "Gives user Duckets.";
            Usage = ":duckets [USERNAME] [AMOUNT]";
            MinParams = 2;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var client = Azure.GetGame().GetClientManager().GetClientByUserName(pms[0]);
            if (client == null)
            {
                session.SendNotif(Azure.GetLanguage().GetVar("user_not_found"));
                return true;
            }
            int amount;
            if (!int.TryParse(pms[1], out amount))
            {
                session.SendNotif(Azure.GetLanguage().GetVar("enter_numbers"));
                return true;
            }
            client.GetHabbo().ActivityPoints += amount;
            client.GetHabbo().UpdateActivityPointsBalance();
            client.SendNotif(string.Format(Azure.GetLanguage().GetVar("staff_gives_duckets"),
                session.GetHabbo().UserName, amount));
            return true;
        }
    }
}