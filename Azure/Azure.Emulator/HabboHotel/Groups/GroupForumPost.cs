#region

using System.Data;

#endregion

namespace Azure.HabboHotel.Groups
{
    /// <summary>
    /// Class GroupForumPost.
    /// </summary>
    internal class GroupForumPost
    {
        /// <summary>
        /// The identifier
        /// </summary>
        internal uint Id;

        /// <summary>
        /// The parent identifier
        /// </summary>
        internal uint ParentId;

        /// <summary>
        /// The group identifier
        /// </summary>
        internal uint GroupId;

        /// <summary>
        /// The timestamp
        /// </summary>
        internal int Timestamp;

        /// <summary>
        /// The pinned
        /// </summary>
        internal bool Pinned;

        /// <summary>
        /// The locked
        /// </summary>
        internal bool Locked;

        /// <summary>
        /// The hidden
        /// </summary>
        internal bool Hidden;

        /// <summary>
        /// The poster identifier
        /// </summary>
        internal uint PosterId;

        /// <summary>
        /// The poster name
        /// </summary>
        internal string PosterName;

        /// <summary>
        /// The poster look
        /// </summary>
        internal string PosterLook;

        /// <summary>
        /// The subject
        /// </summary>
        internal string Subject;

        /// <summary>
        /// The post content
        /// </summary>
        internal string PostContent;

        /// <summary>
        /// The message count
        /// </summary>
        internal int MessageCount;

        /// <summary>
        /// The hider
        /// </summary>
        internal string Hider;

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupForumPost" /> class.
        /// </summary>
        /// <param name="row">The row.</param>
        internal GroupForumPost(DataRow row)
        {
            Id = uint.Parse(row["id"].ToString());
            ParentId = uint.Parse(row["parent_id"].ToString());
            GroupId = uint.Parse(row["group_id"].ToString());
            Timestamp = int.Parse(row["timestamp"].ToString());
            Pinned = row["pinned"].ToString() == "1";
            Locked = row["locked"].ToString() == "1";
            Hidden = row["hidden"].ToString() == "1";
            PosterId = uint.Parse(row["poster_id"].ToString());
            PosterName = row["poster_name"].ToString();
            PosterLook = row["poster_look"].ToString();
            Subject = row["subject"].ToString();
            PostContent = row["post_content"].ToString();
            Hider = row["post_hider"].ToString();
            MessageCount = 0;
            if (ParentId == 0)
                MessageCount = Azure.GetGame().GetGroupManager().GetMessageCountForThread(Id);
        }
    }
}