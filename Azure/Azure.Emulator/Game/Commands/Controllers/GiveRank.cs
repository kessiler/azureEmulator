using Azure.Game.Commands.Interfaces;
using Azure.Game.GameClients.Interfaces;

namespace Azure.Game.Commands.Controllers
{
    /// <summary>
    ///     Class GiveCredits. This class cannot be inherited.
    /// </summary>
    internal sealed class GiveRank : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GiveCredits" /> class.
        /// </summary>
        public GiveRank()
        {
            MinRank = 9;
            Description = "Dar Rango al Usuario.";
            Usage = ":giverank [USERNAME] [RANK]";
            MinParams = 2;
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

            var userName = pms[0];
            var rank = pms[1];
            using (var adapter = Azure.GetDatabaseManager().GetQueryReactor())
            {
                adapter.SetQuery("UPDATE users SET rank=@rank WHERE username=@user LIMIT 1");
                adapter.AddParameter("user", userName);
                adapter.AddParameter("rank", rank);
                adapter.RunQuery();
            }

            session.SendWhisper(Azure.GetLanguage().GetVar("user_rank_update"));
            return true;
        }
    }
}