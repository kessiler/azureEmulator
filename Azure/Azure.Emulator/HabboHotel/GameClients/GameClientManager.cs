#region

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Azure.Configuration;
using Azure.Connection.Connection;
using Azure.HabboHotel.Users.Messenger;
using Azure.Messages;
using Azure.Messages.Parsers;

#endregion

namespace Azure.HabboHotel.GameClients
{
    /// <summary>
    /// Class GameClientManager..
    /// </summary>
    internal class GameClientManager
    {
        /// <summary>
        /// The clients
        /// </summary>
        internal ConcurrentDictionary<uint, GameClient> Clients;

        /// <summary>
        /// The _badge queue
        /// </summary>
        private readonly Queue _badgeQueue;

        /// <summary>
        /// The _broadcast queue
        /// </summary>
        private readonly ConcurrentQueue<byte[]> _broadcastQueue;

        /// <summary>
        /// The _user name register
        /// </summary>
        private readonly HybridDictionary _userNameRegister;

        /// <summary>
        /// The _user identifier register
        /// </summary>
        private readonly HybridDictionary _userIdRegister;

        /// <summary>
        /// The _user name identifier register
        /// </summary>
        private readonly HybridDictionary _userNameIdRegister;

        /// <summary>
        /// The _id user name register
        /// </summary>
        private readonly HybridDictionary _idUserNameRegister;

        private readonly ConcurrentQueue<GameClient> clientsAddQueue;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameClientManager"/> class.
        /// </summary>
        internal GameClientManager()
        {
            Clients = new ConcurrentDictionary<uint, GameClient>();
            clientsAddQueue = new ConcurrentQueue<GameClient>();
            _badgeQueue = new Queue();
            _broadcastQueue = new ConcurrentQueue<byte[]>();
            _userNameRegister = new HybridDictionary();
            _userIdRegister = new HybridDictionary();
            _userNameIdRegister = new HybridDictionary();
            _idUserNameRegister = new HybridDictionary();
        }

        /// <summary>
        /// Gets the client count.
        /// </summary>
        /// <value>The client count.</value>
        internal int ClientCount()
        {
            return Clients.Count;
        }


        /// <summary>
        /// Gets the client by user identifier.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>GameClient.</returns>
        internal GameClient GetClientByUserId(uint userId)
        {
            return _userIdRegister.Contains(userId) ? (GameClient)_userIdRegister[userId] : null;
        }

        /// <summary>
        /// Gets the name of the client by user.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns>GameClient.</returns>
        internal GameClient GetClientByUserName(string userName)
        {
            return _userNameRegister.Contains(userName.ToLower())
                ? (GameClient)_userNameRegister[userName.ToLower()]
                : null;
        }

        /// <summary>
        /// Gets the client.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <returns>GameClient.</returns>
        internal GameClient GetClient(uint clientId)
        {
            return Clients.ContainsKey(clientId) ? Clients[clientId] : null;
        }

        /// <summary>
        /// Gets the name by identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>System.String.</returns>
        internal string GetNameById(uint id)
        {
            var clientByUserId = GetClientByUserId(id);
            if (clientByUserId != null)
                return clientByUserId.GetHabbo().UserName;
            string String;
            using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery("SELECT username FROM users WHERE id = " + id);
                String = queryReactor.GetString();
            }
            return String;
        }

        /// <summary>
        /// Gets the clients by identifier.
        /// </summary>
        /// <param name="users">The users.</param>
        /// <returns>IEnumerable&lt;GameClient&gt;.</returns>
        internal IEnumerable<GameClient> GetClientsById(Dictionary<uint, MessengerBuddy>.KeyCollection users)
        {
            return users.Select(GetClientByUserId).Where(clientByUserId => clientByUserId != null);
        }

        /// <summary>
        /// Sends the super notif.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="notice">The notice.</param>
        /// <param name="picture">The picture.</param>
        /// <param name="client">The client.</param>
        /// <param name="link">The link.</param>
        /// <param name="linkTitle">The link title.</param>
        /// <param name="broadCast">if set to <c>true</c> [broad cast].</param>
        /// <param name="Event">if set to <c>true</c> [event].</param>
        internal void SendSuperNotif(string title, string notice, string picture, GameClient client, string link,
            string linkTitle, bool broadCast, bool Event)
        {
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("SuperNotificationMessageComposer"));
            serverMessage.AppendString(picture);
            serverMessage.AppendInteger(4);
            serverMessage.AppendString("title");
            serverMessage.AppendString(title);
            serverMessage.AppendString("message");
            if (broadCast)
                if (Event)
                {
                    var text1 = Azure.GetLanguage().GetVar("ha_event_one");
                    var text2 = Azure.GetLanguage().GetVar("ha_event_two");
                    var text3 = Azure.GetLanguage().GetVar("ha_event_three");
                    serverMessage.AppendString(string.Format("<b>{0} {1}!</b>\r\n {2} .\r\n<b>{3}</b>\r\n{4}", text1,
                        client.GetHabbo().CurrentRoom.RoomData.Owner, text2, text3, notice));
                }
                else
                {
                    var text4 = Azure.GetLanguage().GetVar("ha_title");
                    serverMessage.AppendString(string.Concat("<b>" + text4 + "</b>\r\n", notice, "\r\n- <i>", client.GetHabbo().UserName, "</i>"));
                }
            else
                serverMessage.AppendString(notice);
            if (link != string.Empty)
            {
                serverMessage.AppendString("linkUrl");
                serverMessage.AppendString(link);
                serverMessage.AppendString("linkTitle");
                serverMessage.AppendString(linkTitle);
            }
            else
            {
                serverMessage.AppendString("linkUrl");
                serverMessage.AppendString("event:");
                serverMessage.AppendString("linkTitle");
                serverMessage.AppendString("ok");
            }

            if (broadCast)
            {
                QueueBroadcaseMessage(serverMessage);
                return;
            }
            client.SendMessage(serverMessage);
        }

        /// <summary>
        /// Called when [cycle].
        /// </summary>
        internal void OnCycle()
        {
            try
            {
                AddClients();
                GiveBadges();
                BroadcastPackets();
                Azure.GetGame().ClientManagerCycleEnded = true;
            }
            catch (Exception ex)
            {
                Logging.LogThreadException(ex.ToString(), "GameClientManager.OnCycle Exception --> Not inclusive");
            }
        }

        /// <summary>
        /// Staffs the alert.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="exclude">The exclude.</param>
        internal void StaffAlert(ServerMessage message, uint exclude = 0u)
        {
            var gameClients =
                Clients.Values
                    .Where(
                        x =>
                            x.GetHabbo() != null && x.GetHabbo().Rank >= Azure.StaffAlertMinRank &&
                            x.GetHabbo().Id != exclude);
            foreach (var current in gameClients)
                current.SendMessage(message);
        }

        /// <summary>
        /// Mods the alert.
        /// </summary>
        /// <param name="message">The message.</param>
        internal void ModAlert(ServerMessage message)
        {
            var bytes = message.GetReversedBytes();
            foreach (
                var current in
                    Clients.Values.Where(current => current != null && current.GetHabbo() != null))
            {
                if (current.GetHabbo().Rank != 4u && current.GetHabbo().Rank != 5u)
                    if (current.GetHabbo().Rank != 6u)
                        continue;
                try
                {
                    current.GetConnection().SendData(bytes);
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// Creates the and start client.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="connection">The connection.</param>
        internal void CreateAndStartClient(uint clientId, ConnectionInformation connection)
        {
            var gameClient = new GameClient(clientId, connection);
            Clients.AddOrUpdate(clientId, gameClient, (key, value) => gameClient);
            clientsAddQueue.Enqueue(gameClient);
        }

        /// <summary>
        /// Disposes the connection.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        internal void DisposeConnection(uint clientId)
        {
            GameClient client = GetClient(clientId);

            if (client != null)
                client.Stop();
            Clients.TryRemove(clientId, out client);
        }

        /// <summary>
        /// Queues the broadcase message.
        /// </summary>
        /// <param name="message">The message.</param>
        internal void QueueBroadcaseMessage(ServerMessage message)
        {
            _broadcastQueue.Enqueue(message.GetReversedBytes());
        }

        /// <summary>
        /// Queues the badge update.
        /// </summary>
        /// <param name="badge">The badge.</param>
        internal void QueueBadgeUpdate(string badge)
        {
            lock (_badgeQueue.SyncRoot)
                _badgeQueue.Enqueue(badge);
        }

        /// <summary>
        /// Logs the clones out.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        internal void LogClonesOut(uint userId)
        {
            var clientByUserId = GetClientByUserId(userId);
            if (clientByUserId != null)
                clientByUserId.Disconnect("user null LogClonesOut");
        }

        /// <summary>
        /// Registers the client.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="userName">Name of the user.</param>
        internal void RegisterClient(GameClient client, uint userId, string userName)
        {
            if (_userNameRegister.Contains(userName.ToLower()))
                _userNameRegister[userName.ToLower()] = client;
            else
                _userNameRegister.Add(userName.ToLower(), client);
            if (_userIdRegister.Contains(userId))
                _userIdRegister[userId] = client;
            else
                _userIdRegister.Add(userId, client);
            if (!_userNameIdRegister.Contains(userName))
                _userNameIdRegister.Add(userName, userId);
            if (!_idUserNameRegister.Contains(userId))
                _idUserNameRegister.Add(userId, userName);
            using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
                queryReactor.SetQuery(string.Format("UPDATE users SET online='1' WHERE id={0} LIMIT 1", userId));
        }

        /// <summary>
        /// Unregisters the client.
        /// </summary>
        /// <param name="userid">The userid.</param>
        /// <param name="userName">The username.</param>
        internal void UnregisterClient(uint userid, string userName)
        {
            _userIdRegister.Remove(userid);
            _userNameRegister.Remove(userName.ToLower());
            using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
                queryReactor.SetQuery(string.Format("UPDATE users SET online='0' WHERE id={0} LIMIT 1", userid));
        }

        /// <summary>
        /// Closes all.
        /// </summary>
        internal void CloseAll()
        {
            var stringBuilder = new StringBuilder();
            var flag = false;

            Out.WriteLine("Saving Inventary Content....", "Azure.Boot", ConsoleColor.DarkCyan);
            foreach (var current2 in Clients.Values.Where(current2 => current2.GetHabbo() != null))
            {
                try
                {
                    current2.GetHabbo().GetInventoryComponent().RunDbUpdate();
                    current2.GetHabbo().RunDbUpdate(Azure.GetDatabaseManager().GetQueryReactor());
                    stringBuilder.Append(current2.GetHabbo().GetQueryString);
                    flag = true;
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                }
                catch
                {
                }
            }
            Out.WriteLine("Inventary Content Saved!", "Azure.Boot", ConsoleColor.DarkCyan);
            if (flag)
                try
                {
                    if (stringBuilder.Length > 0) using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor()) queryReactor.RunFastQuery(stringBuilder.ToString());
                }
                catch (Exception pException)
                {
                    Logging.HandleException(pException, "GameClientManager.CloseAll()");
                }
            try
            {
                Out.WriteLine("Closing Connection Manager...", "Azure.Boot", ConsoleColor.DarkMagenta);
                foreach (GameClient current3 in Clients.Values.Where(current3 => current3.GetConnection() != null))
                {
                    try
                    {
                        current3.GetConnection().Dispose();
                    }
                    catch
                    {
                    }
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    Out.WriteLine("Connection Manager Closed!", "Azure.Boot", ConsoleColor.DarkMagenta);
                }
            }
            catch (Exception ex)
            {
                Logging.LogCriticalException(ex.ToString());
            }
            Clients.Clear();
            Out.WriteLine("Connections closed", "Azure.Conn", ConsoleColor.DarkYellow);
        }

        /// <summary>
        /// Updates the client.
        /// </summary>
        /// <param name="oldName">The old name.</param>
        /// <param name="newName">The new name.</param>
        internal void UpdateClient(string oldName, string newName)
        {
            if (!_userNameRegister.Contains(oldName.ToLower()))
                return;
            var old = (GameClient)_userNameRegister[oldName.ToLower()];
            _userNameRegister.Remove(oldName.ToLower());
            _userNameRegister.Add(newName.ToLower(), old);
        }

        private void AddClients()
        {
            if (clientsAddQueue.Count > 0)
            {
                GameClient client;
                while (clientsAddQueue.TryDequeue(out client))
                {
                    client.StartConnection();
                }
            }
        }

        /// <summary>
        /// Gives the badges.
        /// </summary>
        private void GiveBadges()
        {
            try
            {
                var now = DateTime.Now;
                if (_badgeQueue.Count > 0)
                    lock (_badgeQueue.SyncRoot)
                        while (_badgeQueue.Count > 0)
                        {
                            var badge = (string)_badgeQueue.Dequeue();
                            foreach (
                                var current in
                                    Clients.Values.Where(current => current.GetHabbo() != null))
                                try
                                {
                                    current.GetHabbo().GetBadgeComponent().GiveBadge(badge, true, current, false);
                                    current.SendNotif(Azure.GetLanguage().GetVar("user_earn_badge"));
                                }
                                catch
                                {
                                }
                        }
                var timeSpan = DateTime.Now - now;
                if (timeSpan.TotalSeconds > 3.0)
                    Console.WriteLine("GameClientManager.GiveBadges spent: {0} seconds in working.",
                        timeSpan.TotalSeconds);
            }
            catch (Exception ex)
            {
                Logging.LogThreadException(ex.ToString(), "GameClientManager.GiveBadges Exception --> Not inclusive");
            }
        }

        /// <summary>
        /// Broadcasts the packets.
        /// </summary>
        private void BroadcastPackets()
        {
            try
            {
                if (!_broadcastQueue.Any()) return;
                var now = DateTime.Now;
                byte[] bytes;

                _broadcastQueue.TryDequeue(out bytes);

                foreach (GameClient current in Clients.Values)
                {
                    if (current == null || current.GetConnection() == null)
                        continue;
                    current.GetConnection().SendData(bytes);
                }

                var timeSpan = DateTime.Now - now;
                if (timeSpan.TotalSeconds > 3.0)
                    Console.WriteLine("GameClientManager.BroadcastPackets spent: {0} seconds in working.",
                        timeSpan.TotalSeconds);
            }
            catch (Exception ex)
            {
                Logging.LogThreadException(ex.ToString(),
                    "GameClientManager.BroadcastPackets Exception --> Not inclusive");
            }
        }
    }
}