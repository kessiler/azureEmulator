#region

using Azure.HabboHotel.GameClients;

#endregion

namespace Azure.HabboHotel.Commands.List
{
    /// <summary>
    /// Class RefreshAchievements. This class cannot be inherited.
    /// </summary>
    internal sealed class RefreshAchievements : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RefreshAchievements"/> class.
        /// </summary>
        public RefreshAchievements()
        {
            MinRank = 9;
            Description = "Refreshes Achievements from Database.";
            Usage = ":refresh_achievements";
            MinParams = 0;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            Azure.GetGame().GetAchievementManager().LoadAchievements(Azure.GetDatabaseManager().GetQueryReactor());
            session.SendNotif(Azure.GetLanguage().GetVar("command_refresh_achievements"));
            return true;
        }
    }
}