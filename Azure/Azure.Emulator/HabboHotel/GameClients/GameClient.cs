#region

using System;
using System.Linq;
using Azure.Configuration;
using Azure.Connection.Connection;
using Azure.Connection.Net;
using Azure.HabboHotel.Misc;
using Azure.HabboHotel.Users;
using Azure.HabboHotel.Users.UserDataManagement;
using Azure.Messages;
using Azure.Messages.Handlers;
using Azure.Messages.Parsers;
using Azure.Security.BlackWords;

#endregion

namespace Azure.HabboHotel.GameClients
{
    /// <summary>
    /// Class GameClient.
    /// </summary>
    public class GameClient
    {
        /// <summary>
        /// The publicist count
        /// </summary>
        internal byte PublicistCount;

        /// <summary>
        /// The time pinged received
        /// </summary>
        internal DateTime TimePingedReceived;

        /// <summary>
        /// The packet parser
        /// </summary>
        internal GamePacketParser PacketParser;

        /// <summary>
        /// The designed handler
        /// </summary>
        internal int DesignedHandler = 1;

        /// <summary>
        /// The current room user identifier
        /// </summary>
        internal int CurrentRoomUserId;

        /// <summary>
        /// The machine identifier
        /// </summary>
        internal string MachineId;

        /// <summary>
        /// The _connection
        /// </summary>
        private ConnectionInformation _connection;

        /// <summary>
        /// The _message handler
        /// </summary>
        private GameClientMessageHandler _messageHandler;

        /// <summary>
        /// The _habbo
        /// </summary>
        private Habbo _habbo;

        /// <summary>
        /// The _disconnected
        /// </summary>
        private bool _disconnected;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameClient"/> class.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="connection">The connection.</param>
        internal GameClient(uint clientId, ConnectionInformation connection)
        {
            ConnectionId = clientId;
            _connection = connection;
            CurrentRoomUserId = -1;
            PacketParser = new GamePacketParser(this);
        }

        /// <summary>
        /// Gets the connection identifier.
        /// </summary>
        /// <value>The connection identifier.</value>
        internal uint ConnectionId { get; private set; }

        /// <summary>
        /// Handles the publicist.
        /// </summary>
        /// <param name="word">The word.</param>
        /// <param name="message">The message.</param>
        /// <param name="method">The method.</param>
        /// <param name="settings">The settings.</param>
        internal void HandlePublicist(string word, string message, string method, BlackWordTypeSettings settings)
        {
            ServerMessage serverMessage;
            if (GetHabbo().Rank < 5 && settings.MaxAdvices == PublicistCount++ && settings.AutoBan)
            {
                serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("SuperNotificationMessageComposer"));
                serverMessage.AppendString("staffcloud");
                serverMessage.AppendInteger(2);
                serverMessage.AppendString("title");
                serverMessage.AppendString("Staff Internal Alert");
                serverMessage.AppendString("message");
                serverMessage.AppendString("El usuario " + GetHabbo().UserName + " ha sido baneado por decir muchas palabras no permitidas. Última palabra: " + word + " en la frase: " + message);

                Azure.GetGame().GetClientManager().StaffAlert(serverMessage, 0U);

                Azure.GetGame()
                    .GetBanManager()
                    .BanUser(this, GetHabbo().UserName, 3600,
                        "You're passing too many spams from other hotels. For this reason we penalize you for 1 hour so that you learn to control yourself.",
                        false, false);
                return;
            }

            if (PublicistCount > 4)
                return;

            var alert = settings.Alert.Replace("{0}", GetHabbo().UserName);
            alert = alert.Replace("{1}", GetHabbo().Id.ToString());
            alert = alert.Replace("{2}", word);
            alert = alert.Replace("{3}", message);
            alert = alert.Replace("{4}", method);

            serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("UsersClassificationMessageComposer"));
            serverMessage.AppendInteger(1);

            serverMessage.AppendInteger(GetHabbo().Id);
            serverMessage.AppendString(GetHabbo().UserName);
            serverMessage.AppendString("BadWord: " + word);

            Azure.GetGame().GetClientManager().StaffAlert(serverMessage, 0U);

            serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("SuperNotificationMessageComposer"));
            serverMessage.AppendString(settings.ImageAlert);
            serverMessage.AppendInteger(4);
            serverMessage.AppendString("title");
            serverMessage.AppendString("${generic.notice}");
            serverMessage.AppendString("message");
            serverMessage.AppendString(alert);
            serverMessage.AppendString("link");
            serverMessage.AppendString("event:");
            serverMessage.AppendString("linkTitle");
            serverMessage.AppendString("ok");
            Azure.GetGame().GetClientManager().StaffAlert(serverMessage, 0U);
        }

        /// <summary>
        /// Gets the connection.
        /// </summary>
        /// <returns>ConnectionInformation.</returns>
        internal ConnectionInformation GetConnection()
        {
            return _connection;
        }

        /// <summary>
        /// Gets the message handler.
        /// </summary>
        /// <returns>GameClientMessageHandler.</returns>
        internal GameClientMessageHandler GetMessageHandler()
        {
            return _messageHandler;
        }

        /// <summary>
        /// Gets the habbo.
        /// </summary>
        /// <returns>Habbo.</returns>
        internal Habbo GetHabbo()
        {
            return _habbo;
        }

        /// <summary>
        /// Starts the connection.
        /// </summary>
        internal void StartConnection()
        {
            if (_connection == null)
                return;
            TimePingedReceived = DateTime.Now;
            (_connection.Parser as InitialPacketParser).PolicyRequest += PolicyRequest;
            (_connection.Parser as InitialPacketParser).SwitchParserRequest += SwitchParserRequest;
            _connection.StartPacketProcessing();
        }

        /// <summary>
        /// Initializes the handler.
        /// </summary>
        internal void InitHandler()
        {
            _messageHandler = new GameClientMessageHandler(this);
        }

        /// <summary>
        /// Tries the login.
        /// </summary>
        /// <param name="authTicket">The authentication ticket.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool TryLogin(string authTicket)
        {
            try
            {
                string ip = GetConnection().GetIp();
                uint errorCode = 0;
                var userData = UserDataFactory.GetUserData(authTicket, out errorCode);
                if (errorCode == 1 || errorCode == 2) return false;
                Azure.GetGame()
                    .GetClientManager()
                    .RegisterClient(this, userData.UserId, userData.User.UserName);
                _habbo = userData.User;
                userData.User.LoadData(userData);
                var banReason = Azure.GetGame()
                    .GetBanManager()
                    .GetBanReason(userData.User.UserName, ip, MachineId);
                if (!string.IsNullOrEmpty(banReason) || userData.User.UserName == null)
                {
                    SendNotifWithScroll(banReason);
                    using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
                    {
                        queryReactor.SetQuery(string.Format("SELECT ip_last FROM users WHERE id={0} LIMIT 1",
                            GetHabbo().Id));
                        var @string = queryReactor.GetString();
                        queryReactor.SetQuery(
                            string.Format("SELECT COUNT(0) FROM users_bans_access WHERE user_id={0} LIMIT 1", _habbo.Id));
                        var integer = queryReactor.GetInteger();
                        if (integer > 0)
                            queryReactor.RunFastQuery("UPDATE users_bans_access SET attempts = attempts + 1, ip='" +
                                                      @string + "' WHERE user_id=" + GetHabbo().Id + " LIMIT 1");
                        else
                            queryReactor.RunFastQuery("INSERT INTO users_bans_access (user_id, ip) VALUES (" +
                                                      GetHabbo().Id + ", '" + @string + "')");
                    }
                    return false;
                }
                userData.User.Init(this, userData);
                var queuedServerMessage = new QueuedServerMessage(_connection);
                var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("UniqueMachineIDMessageComposer"));
                serverMessage.AppendString(MachineId);
                queuedServerMessage.AppendResponse(serverMessage);
                queuedServerMessage.AppendResponse(new ServerMessage(LibraryParser.OutgoingRequest("AuthenticationOKMessageComposer")));
                // @issue #99
                if (_habbo != null)
                {
                    var serverMessage2 = new ServerMessage(LibraryParser.OutgoingRequest("HomeRoomMessageComposer"));
                    serverMessage2.AppendInteger(_habbo.HomeRoom);
                    serverMessage2.AppendInteger(_habbo.HomeRoom);
                    queuedServerMessage.AppendResponse(serverMessage2);
                }
                serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("MinimailCountMessageComposer"));
                serverMessage.AppendInteger(_habbo.MinimailUnreadMessages);
                queuedServerMessage.AppendResponse(serverMessage);

                serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("FavouriteRoomsMessageComposer"));
                serverMessage.AppendInteger(30);

                if (userData.User.FavoriteRooms == null || !userData.User.FavoriteRooms.Any()) serverMessage.AppendInteger(0);
                else
                {
                    serverMessage.AppendInteger(userData.User.FavoriteRooms.Count);
                    foreach (uint i in userData.User.FavoriteRooms) serverMessage.AppendInteger(i);
                }
                queuedServerMessage.AppendResponse(serverMessage);

                var rightsMessage = new ServerMessage(LibraryParser.OutgoingRequest("UserClubRightsMessageComposer"));
                rightsMessage.AppendInteger(userData.User.GetSubscriptionManager().HasSubscription ? 2 : 0);
                rightsMessage.AppendInteger(userData.User.Rank);
                rightsMessage.AppendInteger(0);
                queuedServerMessage.AppendResponse(rightsMessage);

                serverMessage =
                    new ServerMessage(LibraryParser.OutgoingRequest("EnableNotificationsMessageComposer"));
                serverMessage.AppendBool(true); //isOpen
                serverMessage.AppendBool(false);
                queuedServerMessage.AppendResponse(serverMessage);

                serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("EnableTradingMessageComposer"));
                serverMessage.AppendBool(true);
                queuedServerMessage.AppendResponse(serverMessage);
                userData.User.UpdateCreditsBalance();

                serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("ActivityPointsMessageComposer"));
                serverMessage.AppendInteger(2);
                serverMessage.AppendInteger(0);
                serverMessage.AppendInteger(userData.User.ActivityPoints);
                serverMessage.AppendInteger(5);
                serverMessage.AppendInteger(userData.User.Diamonds);
                queuedServerMessage.AppendResponse(serverMessage);

                if (userData.User.HasFuse("fuse_mod")) queuedServerMessage.AppendResponse(Azure.GetGame().GetModerationTool().SerializeTool(this));
                queuedServerMessage.AppendResponse(
                    Azure.GetGame().GetAchievementManager().AchievementDataCached);

                if (!GetHabbo().NuxPassed && ExtraSettings.NEW_users_gifts_ENABLED)
                    queuedServerMessage.AppendResponse(
                        new ServerMessage(LibraryParser.OutgoingRequest("NuxSuggestFreeGiftsMessageComposer")));
                queuedServerMessage.AppendResponse(GetHabbo().GetAvatarEffectsInventoryComponent().GetPacket());
                queuedServerMessage.SendResponse();
                Azure.GetGame().GetAchievementManager().TryProgressHabboClubAchievements(this);
                Azure.GetGame().GetAchievementManager().TryProgressRegistrationAchievements(this);
                Azure.GetGame().GetAchievementManager().TryProgressLoginAchievements(this);
                return true;
            }
            catch (Exception ex)
            {
                Logging.LogCriticalException(string.Format("Bug during user login: {0}", ex));
            }
            return false;
        }

        /// <summary>
        /// Sends the notif with scroll.
        /// </summary>
        /// <param name="message">The message.</param>
        internal void SendNotifWithScroll(string message)
        {
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("MOTDNotificationMessageComposer"));
            serverMessage.AppendInteger(1);
            serverMessage.AppendString(message);
            SendMessage(serverMessage);
        }

        /// <summary>
        /// Sends the broadcast message.
        /// </summary>
        /// <param name="message">The message.</param>
        internal void SendBroadcastMessage(string message)
        {
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("BroadcastNotifMessageComposer"));
            serverMessage.AppendString(message);
            serverMessage.AppendString(string.Empty);
            SendMessage(serverMessage);
        }

        /// <summary>
        /// Sends the moderator message.
        /// </summary>
        /// <param name="message">The message.</param>
        internal void SendModeratorMessage(string message)
        {
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("AlertNotificationMessageComposer"));
            serverMessage.AppendString(message);
            serverMessage.AppendString(string.Empty);
            SendMessage(serverMessage);
        }

        /// <summary>
        /// Sends the whisper.
        /// </summary>
        /// <param name="message">The message.</param>
        internal void SendWhisper(string message, bool fromWired = false)
        {
            if (GetHabbo() == null || GetHabbo().CurrentRoom == null)
                return;
            var roomUserByHabbo =
                GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(GetHabbo().UserName);
            if (roomUserByHabbo == null)
                return;
            var whisp = new ServerMessage(LibraryParser.OutgoingRequest("WhisperMessageComposer"));
            whisp.AppendInteger(roomUserByHabbo.VirtualId);
            whisp.AppendString(message);
            whisp.AppendInteger(0);
            whisp.AppendInteger(fromWired ? 34 : roomUserByHabbo.LastBubble);
            whisp.AppendInteger(0);
            whisp.AppendInteger(fromWired);
            SendMessage(whisp);
        }

        /// <summary>
        /// Sends the notif.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="title">The title.</param>
        /// <param name="picture">The picture.</param>
        internal void SendNotif(string message, string title = "Notification", string picture = "")
        {
            SendMessage(GetBytesNotif(message, title, picture));
        }

        /// <summary>
        /// Gets the bytes notif.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="title">The title.</param>
        /// <param name="picture">The picture.</param>
        /// <returns>System.Byte[].</returns>
        public static byte[] GetBytesNotif(string message, string title = "Notification", string picture = "")
        {
            using (
                var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("SuperNotificationMessageComposer"))
                )
            {
                serverMessage.AppendString(picture);
                serverMessage.AppendInteger(4);
                serverMessage.AppendString("title");
                serverMessage.AppendString(title);
                serverMessage.AppendString("message");
                serverMessage.AppendString(message);
                serverMessage.AppendString("linkUrl");
                serverMessage.AppendString("event:");
                serverMessage.AppendString("linkTitle");
                serverMessage.AppendString("ok");

                return serverMessage.GetReversedBytes();
            }
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        internal void Stop()
        {
            if (GetMessageHandler() != null)
                _messageHandler.Destroy();
            if (GetHabbo() != null)
                _habbo.OnDisconnect("disconnect");
            CurrentRoomUserId = -1;
            _messageHandler = null;
            _habbo = null;
            _connection = null;
        }

        /// <summary>
        /// Disconnects the specified reason.
        /// </summary>
        /// <param name="reason">The reason.</param>
        internal void Disconnect(string reason)
        {
            if (GetHabbo() != null)
            {
                using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
                    queryReactor.RunFastQuery(GetHabbo().GetQueryString);
                GetHabbo().OnDisconnect(reason);
            }
            if (_disconnected)
                return;
            if (_connection != null)
                _connection.Dispose();
            _disconnected = true;
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="message">The message.</param>
        internal void SendMessage(ServerMessage message)
        {
            if (message == null)
                return;
            var bytes = message.GetReversedBytes();
            if (GetConnection() == null)
                return;
            GetConnection().SendData(bytes);
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        internal void SendMessage(byte[] bytes)
        {
            if (GetConnection() == null)
                return;
            GetConnection().SendData(bytes);
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="type">The type.</param>
        internal void SendMessage(StaticMessage type)
        {
            if (GetConnection() == null)
                return;
            GetConnection().SendData(StaticMessagesManager.Get(type));
        }

        /// <summary>
        /// Switches the parser request.
        /// </summary>
        private void SwitchParserRequest(byte[] data, int amountOfBytes)
        {
            if (_connection == null)
                return;
            if (_messageHandler == null)
                InitHandler();
            PacketParser.SetConnection(_connection);
            _connection.Parser.Dispose();
            _connection.Parser = PacketParser;
            _connection.Parser.HandlePacketData(data, amountOfBytes);
        }

        /// <summary>
        /// Policies the request.
        /// </summary>
        private void PolicyRequest()
        {
            _connection.SendData(CrossDomainPolicy.XmlPolicyBytes);
        }
    }
}