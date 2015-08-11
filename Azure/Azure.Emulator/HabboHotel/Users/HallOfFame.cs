#region

using System.Collections.Generic;
using System.Data;

#endregion

namespace Azure.HabboHotel.Users
{
    /// <summary>
    /// Class HallOfFame.
    /// </summary>
    internal class HallOfFame
    {
        internal List<HallOfFameElement> Rankings;

        internal HallOfFame()
        {
            Rankings = new List<HallOfFameElement>();
            RefreshHallOfFame();
        }

        public void RefreshHallOfFame()
        {
            Rankings.Clear();
            DataTable table;
            using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery("SELECT * FROM users_rankings ORDER BY score DESC");
                table = queryReactor.GetTable();
                if (table == null) return;
                foreach (DataRow row in table.Rows)
                {
                    Rankings.Add(new HallOfFameElement((uint)row["user_id"], (int)row["score"], (string)row["competition"]));
                }
            }
        }
    }

    /// <summary>
    /// Class HallOfFameElement.
    /// </summary>
    internal class HallOfFameElement
    {
        internal uint UserId;
        internal int Score;
        internal string Competition;

        internal HallOfFameElement(uint user_id, int score, string competition)
        {
            UserId = user_id;
            Score = score;
            Competition = competition;
        }
    }
}