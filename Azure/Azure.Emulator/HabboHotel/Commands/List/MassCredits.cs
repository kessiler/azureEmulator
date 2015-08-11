#region

using Azure.HabboHotel.GameClients;

#endregion

namespace Azure.HabboHotel.Commands.List
{
    /// <summary>
    /// Class MassCredits. This class cannot be inherited.
    /// </summary>
    internal sealed class MassCredits : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MassCredits"/> class.
        /// </summary>
        public MassCredits()
        {
            MinRank = 8;
            Description = "Gives all the users online credits.";
            Usage = ":masscredits [AMOUNT]";
            MinParams = 1;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            int amount;
            if (!int.TryParse(pms[0], out amount))
            {
                session.SendNotif(Azure.GetLanguage().GetVar("enter_numbers"));
                return true;
            }
            foreach (GameClient client in Azure.GetGame().GetClientManager().Clients.Values)
            {
                if (client == null || client.GetHabbo() == null) continue;
                var habbo = client.GetHabbo();
                client.GetHabbo().Credits += amount;
                client.GetHabbo().UpdateCreditsBalance();
                client.SendNotif(Azure.GetLanguage().GetVar("command_mass_credits_one_give") + amount + (Azure.GetLanguage().GetVar("command_mass_credits_two_give")));
            }
            return true;
        }
    }
}