using Azure.Game.Commands.Interfaces;
using Azure.Game.GameClients.Interfaces;

namespace Azure.Game.Commands.Controllers
{
    /// <summary>
    ///     Class UnBanUser. This class cannot be inherited.
    /// </summary>
    internal sealed class UnBanUser : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="UnBanUser" /> class.
        /// </summary>
        public UnBanUser()
        {
            MinRank = 4;
            Description = "Unban a user!";
            Usage = ":unban [USERNAME]";
            MinParams = 1;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var user = Azure.GetHabboForName(pms[0]);

            if (user == null)
            {
                session.SendWhisper(Azure.GetLanguage().GetVar("user_not_found"));
                return true;
            }
            if (user.Rank >= session.GetHabbo().Rank)
            {
                session.SendWhisper(Azure.GetLanguage().GetVar("user_is_higher_rank"));
                return true;
            }
            using (var adapter = Azure.GetDatabaseManager().GetQueryReactor())
            {
                adapter.SetQuery("DELETE FROM users_bans WHERE value = @name");
                adapter.AddParameter("name", user.UserName);
                adapter.RunQuery();
                Azure.GetGame()
                    .GetModerationTool()
                    .LogStaffEntry(session.GetHabbo().UserName, user.UserName, "Unban",
                        $"User has been Unbanned [{pms[0]}]");
                return true;
            }
        }
    }
}