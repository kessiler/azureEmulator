#region

using System.Collections.Generic;
using System.Data;
using Azure.Messages;

#endregion

namespace Azure.HabboHotel.Items
{
    /// <summary>
    /// Class HighscoreData.
    /// </summary>
    internal class HighscoreData
    {
        /// <summary>
        /// The lines
        /// </summary>
        internal Dictionary<int, HighScoreLine> Lines;

        /// <summary>
        /// The last identifier
        /// </summary>
        internal int LastId;

        /// <summary>
        /// Initializes a new instance of the <see cref="HighscoreData"/> class.
        /// </summary>
        /// <param name="roomItem">The room item.</param>
        internal HighscoreData(RoomItem roomItem)
        {
            Lines = new Dictionary<int, HighScoreLine>();
            uint itemId = roomItem.Id;
            using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery("SELECT * FROM items_highscores WHERE item_id=" + itemId + " ORDER BY score DESC");
                DataTable table = queryReactor.GetTable();
                if (table == null)
                    return;
                foreach (DataRow Row in table.Rows)
                {
                    Lines.Add((int)Row["id"], new HighScoreLine((string)Row["username"], (int)Row["score"]));
                    LastId = (int)Row["id"];
                }
            }
        }

        /// <summary>
        /// Generates the extra data.
        /// </summary>
        /// <param name="Item">The item.</param>
        /// <param name="Message">The message.</param>
        /// <returns>ServerMessage.</returns>
        internal ServerMessage GenerateExtraData(RoomItem Item, ServerMessage Message)
        {
            Message.AppendInteger(6);
            Message.AppendString(Item.ExtraData);//Ouvert/fermé
            if (Item.GetBaseItem().Name.StartsWith("highscore_classic"))
                Message.AppendInteger(2);
            else if (Item.GetBaseItem().Name.StartsWith("highscore_mostwin"))
                Message.AppendInteger(1);
            else if (Item.GetBaseItem().Name.StartsWith("highscore_perteam"))
                Message.AppendInteger(0);
            Message.AppendInteger(0);//Time : ["alltime", "daily", "weekly", "monthly"]
            Message.AppendInteger(Lines.Count);//Count
            foreach (var line in Lines)
            {
                Message.AppendInteger(line.Value.Score);
                Message.AppendInteger(1);
                Message.AppendString(line.Value.Username);
            }
            return Message;
        }

        /// <summary>
        /// Adds the user score.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="username">The username.</param>
        /// <param name="score">The score.</param>
        internal void addUserScore(RoomItem item, string username, int score)
        {
            try
            {
                using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
                {
                    if (item.GetBaseItem().Name.StartsWith("highscore_classic"))
                    {
                        queryReactor.SetQuery("INSERT INTO items_highscores (item_id,username,score) VALUES (@itemid,@username,@score)");
                        queryReactor.AddParameter("itemid", item.Id);
                        queryReactor.AddParameter("username", username);
                        queryReactor.AddParameter("score", score);
                        queryReactor.RunQuery();
                    }
                    else if (item.GetBaseItem().Name.StartsWith("highscore_mostwin"))
                    {
                        score = 1;
                        queryReactor.SetQuery("SELECT id,score FROM items_highscores WHERE username = @username AND item_id = @itemid");
                        queryReactor.AddParameter("itemid", item.Id);
                        queryReactor.AddParameter("username", username);
                        var row = queryReactor.GetRow();
                        if (row != null)
                        {
                            queryReactor.SetQuery("UPDATE items_highscores SET score = score + 1 WHERE username = @username AND item_id = @itemid");
                            queryReactor.AddParameter("itemid", item.Id);
                            queryReactor.AddParameter("username", username);
                            queryReactor.RunQuery();
                            Lines.Remove((int)row["id"]);
                            score = (int)row["score"] + 1;
                        }
                        else
                        {
                            queryReactor.SetQuery("INSERT INTO items_highscores (item_id,username,score) VALUES (@itemid,@username,@score)");
                            queryReactor.AddParameter("itemid", item.Id);
                            queryReactor.AddParameter("username", username);
                            queryReactor.AddParameter("score", score);
                            queryReactor.RunQuery();
                        }
                    }
                    LastId++;
                    Lines.Add(LastId, new HighScoreLine(username, score));
                }
            }
            catch
            {
            }
        }
    }
}