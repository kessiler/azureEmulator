#region

using System.Collections.Generic;
using System.Data;
using Azure.Database.Manager.Database.Session_Details.Interfaces;
using Azure.HabboHotel.GameClients;
using Azure.HabboHotel.Items;
using Azure.Messages;
using Azure.Messages.Parsers;

#endregion

namespace Azure.HabboHotel.Achievements
{
    /// <summary>
    /// Class TalentManager.
    /// </summary>
    internal class TalentManager
    {
        /// <summary>
        /// The talents
        /// </summary>
        internal Dictionary<int, Talent> Talents;

        /// <summary>
        /// Initializes a new instance of the <see cref="TalentManager"/> class.
        /// </summary>
        internal TalentManager()
        {
            Talents = new Dictionary<int, Talent>();
        }

        /// <summary>
        /// Initializes the specified database client.
        /// </summary>
        /// <param name="dbClient">The database client.</param>
        internal void Initialize(IQueryAdapter dbClient)
        {
            dbClient.SetQuery("SELECT * FROM achievements_talents ORDER BY `order_num` ASC");
            DataTable table = dbClient.GetTable();
            foreach (DataRow dataRow in table.Rows)
            {
                var talent = new Talent((int)dataRow["id"], (string)dataRow["type"], (int)dataRow["parent_category"], (int)dataRow["level"], (string)dataRow["achievement_group"], (int)dataRow["achievement_level"], (string)dataRow["prize"], (uint)dataRow["prize_baseitem"]);
                Talents.Add(talent.Id, talent);
            }
        }

        /// <summary>
        /// Gets the talent.
        /// </summary>
        /// <param name="TalentId">The talent identifier.</param>
        /// <returns>Talent.</returns>
        internal Talent GetTalent(int TalentId)
        {
            return Talents[TalentId];
        }

        /// <summary>
        /// Levels the is completed.
        /// </summary>
        /// <param name="Session">The session.</param>
        /// <param name="TrackType">Type of the track.</param>
        /// <param name="TalentLevel">The talent level.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool LevelIsCompleted(GameClient Session, string TrackType, int TalentLevel)
        {
            foreach (Talent current in GetTalents(TrackType, TalentLevel))
            {
                if (Session.GetHabbo().GetAchievementData(current.AchievementGroup) != null && Session.GetHabbo().GetAchievementData(current.AchievementGroup).Level >= current.AchievementLevel)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Completes the user talent.
        /// </summary>
        /// <param name="Session">The session.</param>
        /// <param name="Talent">The talent.</param>
        internal void CompleteUserTalent(GameClient Session, Talent Talent)
        {
            if (Session == null || Session.GetHabbo() == null || Session.GetHabbo().CurrentTalentLevel < Talent.Level || Session.GetHabbo().Talents.ContainsKey(Talent.Id))
                return;
            if (!LevelIsCompleted(Session, Talent.Type, Talent.Level))
                return;
            if (!string.IsNullOrEmpty(Talent.Prize) && Talent.PrizeBaseItem > 0u)
            {
                Item item = Azure.GetGame().GetItemManager().GetItem(Talent.PrizeBaseItem);
                Azure.GetGame().GetCatalog().DeliverItems(Session, item, 1, "", 0, 0, "");
            }
            var value = new UserTalent(Talent.Id, 1);
            Session.GetHabbo().Talents.Add(Talent.Id, value);
            using (IQueryAdapter queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.RunFastQuery(string.Concat("REPLACE INTO users_talents VALUES (", Session.GetHabbo().Id, ", ", Talent.Id, ", ", 1, ");"));
            }
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("TalentLevelUpMessageComposer"));
            serverMessage.AppendString(Talent.Type);
            serverMessage.AppendInteger(Talent.Level);
            serverMessage.AppendInteger(0);
            if (Talent.Type == "citizenship" && Talent.Level == 4)
            {
                serverMessage.AppendInteger(2);
                serverMessage.AppendString("HABBO_CLUB_VIP_7_DAYS");
                serverMessage.AppendInteger(7);
                serverMessage.AppendString(Talent.Prize);
                serverMessage.AppendInteger(0);
            }
            else
            {
                serverMessage.AppendInteger(1);
                serverMessage.AppendString(Talent.Prize);
                serverMessage.AppendInteger(0);
            }

            Session.SendMessage(serverMessage);

            if (Talent.Type == "citizenship")
            {
                if (Talent.Level == 3)
                    Azure.GetGame().GetAchievementManager().ProgressUserAchievement(Session, "ACH_Citizenship", 1);
                else if (Talent.Level == 4)
                {
                    Session.GetHabbo().GetSubscriptionManager().AddSubscription(7);
                    using (IQueryAdapter queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
                    {
                        queryReactor.RunFastQuery(string.Concat(new object[]
                        {
                            "UPDATE users SET talent_status = 'helper' WHERE id = ",
                            Session.GetHabbo().Id,
                            ";"
                        }));
                    }
                }
            }
        }

        /// <summary>
        /// Tries the get talent.
        /// </summary>
        /// <param name="AchGroup">The ach group.</param>
        /// <param name="talent">The talent.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool TryGetTalent(string AchGroup, out Talent talent)
        {
            foreach (Talent current in Talents.Values)
            {
                if (current.AchievementGroup == AchGroup)
                {
                    talent = current;
                    return true;
                }
            }
            talent = null;
            return false;
        }

        /// <summary>
        /// Gets all talents.
        /// </summary>
        /// <returns>Dictionary&lt;System.Int32, Talent&gt;.</returns>
        internal Dictionary<int, Talent> GetAllTalents()
        {
            return Talents;
        }

        /// <summary>
        /// Gets the talents.
        /// </summary>
        /// <param name="TrackType">Type of the track.</param>
        /// <param name="ParentCategory">The parent category.</param>
        /// <returns>List&lt;Talent&gt;.</returns>
        internal List<Talent> GetTalents(string TrackType, int ParentCategory)
        {
            var list = new List<Talent>();
            foreach (Talent current in Talents.Values)
            {
                if (current.Type == TrackType && current.ParentCategory == ParentCategory)
                {
                    list.Add(current);
                }
            }
            return list;
        }
    }
}