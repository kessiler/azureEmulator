#region

using System.Linq;
using Azure.HabboHotel.GameClients;

#endregion

namespace Azure.HabboHotel.Commands.List
{
    /// <summary>
    /// Class BanUser. This class cannot be inherited.
    /// </summary>
    internal sealed class BanUserIp : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BanUser"/> class.
        /// </summary>
        public BanUserIp()
        {
            MinRank = 4;
            Description = "Ban a user by IP!";
            Usage = ":ipban [USERNAME] [REASON]";
            MinParams = -1;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var user = Azure.GetGame().GetClientManager().GetClientByUserName(pms[0]);

            if (user == null)
            {
                session.SendWhisper(Azure.GetLanguage().GetVar("user_not_found"));
                return true;
            }
            if (user.GetHabbo().Rank >= session.GetHabbo().Rank)
            {
                session.SendWhisper(Azure.GetLanguage().GetVar("user_is_higher_rank"));
                return true;
            }
            try
            {
                Azure.GetGame()
                    .GetBanManager()
                    .BanUser(user, session.GetHabbo().UserName, 788922000.0, string.Join(" ", pms.Skip(2)),
                        true, false);
            }
            catch
            {
                Writer.Writer.LogException("Error while banning");
            }

            return true;
        }
    }
}