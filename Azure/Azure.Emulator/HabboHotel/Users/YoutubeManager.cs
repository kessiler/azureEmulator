#region

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Net;
using System.Web;
using System.Text.RegularExpressions;
using Azure.HabboHotel.GameClients;
using Azure.Messages;
using System;

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
        internal static readonly Regex YoutubeVideoRegex = new Regex(@"youtu(?:\.be|be\.com)/(?:.*v(?:/|=)|(?:.*/)?)([a-zA-Z0-9-_]+)", RegexOptions.IgnoreCase);

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

        public string GetTitle(string url)
        {
            string id = GetArgs(url, "v", '?');
            WebClient client = new WebClient();
            return GetArgs(client.DownloadString("http://youtube.com/get_video_info?video_id=" + id), "title", '&');
        }

        public string GetTitleById(string videoId)
        {
            WebClient client = new WebClient();
            return GetArgs(client.DownloadString("http://youtube.com/get_video_info?video_id=" + videoId), "title", '&');
        }

        private string GetArgs(string args, string key, char query)
        {
            int iqs = args.IndexOf(query);
            string querystring = null;

            if (iqs != -1)
            {
                querystring = (iqs < args.Length - 1) ? args.Substring(iqs + 1) : String.Empty;
                NameValueCollection nvcArgs = HttpUtility.ParseQueryString(querystring);
                return nvcArgs[key];
            }
            return String.Empty;
        }

        public void AddUserVideo(GameClient client, string video)
        {
            if (client != null)
            {
                Match youtubeMatch = YoutubeVideoRegex.Match(video);

                string id = string.Empty;
                string video_name = string.Empty;

                if (youtubeMatch.Success)
                {
                    id = youtubeMatch.Groups[1].Value;
                    video_name = GetTitleById(id);

                    if (String.IsNullOrEmpty(video_name))
                    {
                        client.SendWhisper("This Youtube Video doesn't Exists");
                        return;
                    }
                }
                else
                {
                    client.SendWhisper("This Youtube Url is Not Valid");
                    return;
                }


                UserId = client.GetHabbo().Id;
                using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
                {
                    queryReactor.SetQuery("INSERT INTO users_videos_youtube (user_id, video_id, name, description) VALUES (@user_id, @video_id, @name, @name)");
                    queryReactor.AddParameter("user_id", UserId);
                    queryReactor.AddParameter("video_id", id);
                    queryReactor.AddParameter("name", video_name);
                    queryReactor.RunQuery();
                }

                RefreshVideos();
                client.SendNotif("Youtube Video Added Sucessfully!");
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