#region

using System.Collections.Generic;
using Azure.Database.Manager.Database.Session_Details.Interfaces;
using Azure.Messages;

#endregion

namespace Azure.HabboHotel.Support
{
    /// <summary>
    /// Class SupportTicket.
    /// </summary>
    internal class SupportTicket
    {
        /// <summary>
        /// The score
        /// </summary>
        internal int Score;

        /// <summary>
        /// The category
        /// </summary>
        internal int Category;

        /// <summary>
        /// The type
        /// </summary>
        internal int Type;

        /// <summary>
        /// The status
        /// </summary>
        internal TicketStatus Status;

        /// <summary>
        /// The sender identifier
        /// </summary>
        internal uint SenderId;

        /// <summary>
        /// The reported identifier
        /// </summary>
        internal uint ReportedId;

        /// <summary>
        /// The moderator identifier
        /// </summary>
        internal uint ModeratorId;

        /// <summary>
        /// The message
        /// </summary>
        internal string Message;

        /// <summary>
        /// The room identifier
        /// </summary>
        internal uint RoomId;

        /// <summary>
        /// The room name
        /// </summary>
        internal string RoomName;

        /// <summary>
        /// The timestamp
        /// </summary>
        internal double Timestamp;

        /// <summary>
        /// The reported chats
        /// </summary>
        internal List<string> ReportedChats;

        /// <summary>
        /// The _sender name
        /// </summary>
        private readonly string _senderName;

        /// <summary>
        /// The _reported name
        /// </summary>
        private readonly string _reportedName;

        /// <summary>
        /// The _mod name
        /// </summary>
        private string _modName;

        /// <summary>
        /// Initializes a new instance of the <see cref="SupportTicket"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="score">The score.</param>
        /// <param name="category">The category.</param>
        /// <param name="type">The type.</param>
        /// <param name="senderId">The sender identifier.</param>
        /// <param name="reportedId">The reported identifier.</param>
        /// <param name="message">The message.</param>
        /// <param name="roomId">The room identifier.</param>
        /// <param name="roomName">Name of the room.</param>
        /// <param name="timestamp">The timestamp.</param>
        /// <param name="reportedChats">The reported chats.</param>
        internal SupportTicket(uint id, int score, int category, int type, uint senderId, uint reportedId, string message, uint roomId,
            string roomName, double timestamp, List<string> reportedChats)
        {
            TicketId = id;
            Score = score;
            Category = category;
            Type = type;
            Status = TicketStatus.Open;
            SenderId = senderId;
            ReportedId = reportedId;
            ModeratorId = 0u;
            Message = message;
            RoomId = roomId;
            RoomName = roomName;
            Timestamp = timestamp;
            _senderName = Azure.GetGame().GetClientManager().GetNameById(senderId);
            _reportedName = Azure.GetGame().GetClientManager().GetNameById(reportedId);
            _modName = Azure.GetGame().GetClientManager().GetNameById(ModeratorId);
            ReportedChats = reportedChats;
        }

        /// <summary>
        /// Gets the tab identifier.
        /// </summary>
        /// <value>The tab identifier.</value>
        internal int TabId
        {
            get
            {
                if (Status == TicketStatus.Open)
                    return 1;
                if (Status == TicketStatus.Picked)
                    return 2;
                if ((Status == TicketStatus.Abusive) || (Status == TicketStatus.Invalid) || (Status == TicketStatus.Resolved))
                    return 0;
                if (Status == TicketStatus.Deleted)
                    return 0;
                return 0;
            }
        }

        /// <summary>
        /// Gets the ticket identifier.
        /// </summary>
        /// <value>The ticket identifier.</value>
        internal uint TicketId { get; private set; }

        /// <summary>
        /// Picks the specified p moderator identifier.
        /// </summary>
        /// <param name="pModeratorId">The p moderator identifier.</param>
        /// <param name="updateInDb">if set to <c>true</c> [update in database].</param>
        internal void Pick(uint pModeratorId, bool updateInDb)
        {
            Status = TicketStatus.Picked;
            ModeratorId = pModeratorId;
            _modName = Azure.GetHabboById(pModeratorId).UserName;

            if (!updateInDb)
                return;
            using (IQueryAdapter queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.RunFastQuery(string.Concat("UPDATE moderation_tickets SET status = 'picked', moderator_id = ", pModeratorId, ", timestamp = '", Azure.GetUnixTimeStamp(), "' WHERE id = ", TicketId));
            }
        }

        /// <summary>
        /// Closes the specified new status.
        /// </summary>
        /// <param name="newStatus">The new status.</param>
        /// <param name="updateInDb">if set to <c>true</c> [update in database].</param>
        internal void Close(TicketStatus newStatus, bool updateInDb)
        {
            Status = newStatus;

            if (!updateInDb)
                return;

            string text = "resolved";

            switch (newStatus)
            {
                case TicketStatus.Abusive:
                    text = "abusive";
                    break;
                case TicketStatus.Invalid:
                    text = "invalid";
                    break;
                case TicketStatus.Resolved:
                    text = "resolved";
                    break;            
            }

            using (IQueryAdapter queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.RunFastQuery(string.Concat("UPDATE moderation_tickets SET status = '", text, "' WHERE id = ", TicketId));
            }
        }

        /// <summary>
        /// Releases the specified update in database.
        /// </summary>
        /// <param name="updateInDb">if set to <c>true</c> [update in database].</param>
        internal void Release(bool updateInDb)
        {
            Status = TicketStatus.Open;

            if (!updateInDb)
                return;

            using (IQueryAdapter queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.RunFastQuery(string.Format("UPDATE moderation_tickets SET status = 'open' WHERE id = {0}", TicketId));
            }
        }

        /// <summary>
        /// Deletes the specified update in database.
        /// </summary>
        /// <param name="updateInDb">if set to <c>true</c> [update in database].</param>
        internal void Delete(bool updateInDb)
        {
            Status = TicketStatus.Deleted;
            if (!updateInDb)
                return;

            using (IQueryAdapter queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.RunFastQuery(string.Format("UPDATE moderation_tickets SET status = 'deleted' WHERE id = {0}", TicketId));
            }
        }

        /// <summary>
        /// Serializes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>ServerMessage.</returns>
        internal ServerMessage Serialize(ServerMessage message)
        {
            message.AppendInteger(TicketId); // id
            message.AppendInteger(TabId); // state
            message.AppendInteger(3); // type (3 or 4 for new style)
            message.AppendInteger(Category);
            message.AppendInteger((Azure.GetUnixTimeStamp() - (int)Timestamp) * 1000); // -->> timestamp
            message.AppendInteger(Score); // priority
            message.AppendInteger(1); // ensures that more tickets of the same reporter/reported user get merged
            message.AppendInteger(SenderId); // sender id 8 ints
            message.AppendString(_senderName); // sender name
            message.AppendInteger(ReportedId);
            message.AppendString(_reportedName);
            message.AppendInteger((Status == TicketStatus.Picked) ? ModeratorId : 0); // mod id
            message.AppendString(_modName); // mod name
            message.AppendString(Message); // issue message
            message.AppendInteger(0); // is room public?

            message.AppendInteger(ReportedChats.Count);

            foreach (string str in ReportedChats)
            {
                message.AppendString(str);
                message.AppendInteger(-1);
                message.AppendInteger(-1);
            }

            return message;
        }
    }
}