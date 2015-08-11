#region

using System.Collections.Generic;
using System.Data;
using Azure.Messages;

#endregion

namespace Azure.HabboHotel.Users
{
    /// <summary>
    /// Class YoutubeManager.
    /// </summary>
    internal class YoutubeManager
    {
        internal Dictionary<string, YoutubeVideo> Videos;
        internal uint UserId;

        internal YoutubeManager(uint id)
        {
            UserId = id;
            Videos = new Dictionary<string, YoutubeVideo>();
            RefreshVideos();
        }

        public void RefreshVideos()
        {
            Videos.Clear();
            using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery("SELECT * FROM users_videos_youtube WHERE user_id = @user_id");
                queryReactor.AddParameter("user_id", UserId);
                var table = queryReactor.GetTable();
                if (table == null) return;
                foreach (DataRow row in table.Rows)
                {
                    Videos.Add((string)row["video_id"], new YoutubeVideo((string)row["video_id"], (string)row["name"], (string)row["description"]));
                }
            }
        }
    }

    /// <summary>
    /// Class YoutubeVideo.
    /// </summary>
    internal class YoutubeVideo
    {
        internal string VideoId;
        internal string Name;
        internal string Description;

        internal YoutubeVideo(string video_id, string name, string description)
        {
            VideoId = video_id;
            Name = name;
            Description = description;
        }

        internal void Serialize(ServerMessage message)
        {
            message.AppendString(VideoId);
            message.AppendString(Name);
            message.AppendString(Description);
        }
    }
}