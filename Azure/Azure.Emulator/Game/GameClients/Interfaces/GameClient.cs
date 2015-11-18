using System;
using System.Linq;
using Azure.Data;
using Azure.Game.Users;
using Azure.Game.Users.Factories;
using Azure.Game.Users.UserDataManagement;
using Azure.Messages;
using Azure.Messages.Enums;
using Azure.Messages.Handlers;
using Azure.Messages.Parsers;
using Azure.Net.Connection;
using Azure.Net.Packets;
using Azure.Security;
using Azure.Security.BlackWords.Structs;
using Azure.Settings;

namespace Azure.Game.GameClients.Interfaces
{
    /// <summary>
    ///     Class GameClient.
    /// </summary>
    public class GameClient
    {
        /// <summary>
        ///     The _connection
        /// </summary>
        private ConnectionData _connection;

        /// <summary>
        ///     The _disconnected
        /// </summary>
        private bool _disconnected;

        /// <summary>
        ///     The _habbo
        /// </summary>
        private Habbo _habbo;

        /// <summary>
        ///     The _message handler
        /// </summary>
        private GameClientMessageHandler _messageHandler;

        /// <summary>
        ///     The current room user identifier
        /// </summary>
        internal int CurrentRoomUserId;

        /// <summary>
        ///     The designed handler
        /// </summary>
        internal int DesignedHandler = 1;

        /// <summary>
        ///     The machine identifier
        /// </summary>
        internal string MachineId;

        /// <summary>
        ///     The packet parser
        /// </summary>
        internal ServerPacketParser PacketParser;

        /// <summary>
        ///     The publicist count
        /// </summary>
        internal byte PublicistCount;

        /// <summary>
        ///     The time pinged received
        /// </summary>
        internal DateTime TimePingedReceived;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameClient" /> class.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="connection">The connection.</param>
        internal GameClient(uint clientId, ConnectionData connection)
        {
            ConnectionId = clientId;
            _connection = connection;
            CurrentRoomUserId = -1;
            PacketParser = new ServerPacketParser();
        }

        /// <summary>
        ///     Gets the connection identifier.
        /// </summary>
        /// <value>The connection identifier.</value>
        internal uint ConnectionId { get; private set; }

        /// <summary>
        ///     Handles the publicist.
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
                serverMessage.AppendString("O usu�rio " + GetHabbo().UserName + " Foi banido por enviar repetidamente palavras repetidas. A �ltima palavra foi: " +
                                           word + ", na frase: " + message);

                Azure.GetGame().GetClientManager().StaffAlert(serverMessage);

                Azure.GetGame().GetBanManager().BanUser(this, GetHabbo().UserName, 3600, "Voc� est� passando muitos spams de outros hot�is. Por esta raz�o, sancion�-lo por 1 hora, de modo que voc� aprender a controlar-se.", false, false);
                return;
            }

            //if (PublicistCount > 4)
            //    return;

            // Queremos que os Staffs Saibam desses dados.

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

            Azure.GetGame().GetClientManager().StaffAlert(serverMessage);

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

            Azure.GetGame().GetClientManager().StaffAlert(serverMessage);
        }

        /// <summary>
        ///     Gets the connection.
        /// </summary>
        /// <returns>ConnectionInformation.</returns>
        internal ConnectionData GetConnection() => _connection;

        /// <summary>
        ///     Gets the message handler.
        /// </summary>
        /// <returns>GameClientMessageHandler.</returns>
        internal GameClientMessageHandler GetMessageHandler() => _messageHandler;

        /// <summary>
        ///     Gets the habbo.
        /// </summary>
        /// <returns>Habbo.</returns>
        internal Habbo GetHabbo() => _habbo;

        /// <summary>
        ///     Starts the connection.
        /// </summary>
        internal void StartConnection()
        {
            if (_connection == null)
                return;

            TimePingedReceived = DateTime.Now;

            var packetParser = _connection.Parser as InitialPacketParser;

            if (packetParser != null)
                packetParser.PolicyRequest += PolicyRequest;

            var initialPacketParser = _connection.Parser as InitialPacketParser;

            if (initialPacketParser != null)
                initialPacketParser.SwitchParserRequest += SwitchParserRequest;

            _connection.StartPacketProcessing();
        }

        /// <summary>
        ///     Initializes the handler.
        /// </summary>
        internal void InitHandler()
        {
            _messageHandler = new GameClientMessageHandler(this);
        }

        /// <summary>
        ///     Tries the login.
        /// </summary>
        /// <param name="authTicket">The authentication ticket.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool TryLogin(string authTicket)
        {
            try
            {
                var ip = GetConnection().GetIp();

                uint errorCode;

                var userData = UserDataFactory.GetUserData(authTicket, out errorCode);

                if (errorCode == 1 || errorCode == 2)
                    return false;

                Azure.GetGame().GetClientManager().RegisterClient(this, userData.UserId, userData.User.UserName);

                _habbo = userData.User;
                userData.User.LoadData(userData);

                var banReason = Azure.GetGame().GetBanManager().GetBanReason(userData.User.UserName, ip, MachineId);

                if (!string.IsNullOrEmpty(banReason) || userData.User.UserName == null)
                {
                    SendNotifWithScroll(banReason);
                    using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
                    {
                        queryReactor.SetQuery($"SELECT ip_last FROM users WHERE id={GetHabbo().Id} LIMIT 1");

                        var supaString = queryReactor.GetString();

                        queryReactor.SetQuery($"SELECT COUNT(0) FROM users_bans_access WHERE user_id={_habbo.Id} LIMIT 1");
                        var integer = queryReactor.GetInteger();

                        if (integer > 0)
                            queryReactor.RunFastQuery("UPDATE users_bans_access SET attempts = attempts + 1, ip='" + supaString + "' WHERE user_id=" + GetHabbo().Id + " LIMIT 1");
                        else
                            queryReactor.RunFastQuery("INSERT INTO users_bans_access (user_id, ip) VALUES (" + GetHabbo().Id + ", '" + supaString + "')");
                    }

                    return false;
                }

                using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
                    queryReactor.RunFastQuery($"UPDATE users SET ip_last='{ip}' WHERE id={GetHabbo().Id}");

                userData.User.Init(this, userData);

                var queuedServerMessage = new QueuedServerMessage(_connection);

                var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("UniqueMachineIDMessageComposer"));

                serverMessage.AppendString(MachineId);
                queuedServerMessage.AppendResponse(serverMessage);

                queuedServerMessage.AppendResponse(new ServerMessage(LibraryParser.OutgoingRequest("AuthenticationOKMessageComposer")));

                var serverMessage2 = new ServerMessage(LibraryParser.OutgoingRequest("HomeRoomMessageComposer"));

                serverMessage2.AppendInteger(_habbo.HomeRoom);
                serverMessage2.AppendInteger(_habbo.HomeRoom);
                queuedServerMessage.AppendResponse(serverMessage2);

                serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("MinimailCountMessageComposer"));

                serverMessage.AppendInteger(_habbo.MinimailUnreadMessages);
                queuedServerMessage.AppendResponse(serverMessage);

                serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("FavouriteRoomsMessageComposer"));

                serverMessage.AppendInteger(30);

                if (userData.User.FavoriteRooms == null || !userData.User.FavoriteRooms.Any())
                    serverMessage.AppendInteger(0);
                else
                {
                    serverMessage.AppendInteger(userData.User.FavoriteRooms.Count);

                    foreach (var i in userData.User.FavoriteRooms)
                        serverMessage.AppendInteger(i);
                }

                queuedServerMessage.AppendResponse(serverMessage);

                var rightsMessage = new ServerMessage(LibraryParser.OutgoingRequest("UserClubRightsMessageComposer"));

                rightsMessage.AppendInteger(userData.User.GetSubscriptionManager().HasSubscription ? 2 : 0);
                rightsMessage.AppendInteger(userData.User.Rank);
                rightsMessage.AppendInteger(0);
                queuedServerMessage.AppendResponse(rightsMessage);

                serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("EnableNotificationsMessageComposer"));
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

                if (userData.User.HasFuse("fuse_mod"))
                    queuedServerMessage.AppendResponse(Azure.GetGame().GetModerationTool().SerializeTool(this));

                queuedServerMessage.AppendResponse(Azure.GetGame().GetAchievementManager().AchievementDataCached);

                if (!GetHabbo().NuxPassed && ServerExtraSettings.NewUsersGiftsEnabled)
                    queuedServerMessage.AppendResponse( new ServerMessage(LibraryParser.OutgoingRequest("NuxSuggestFreeGiftsMessageComposer")));

                queuedServerMessage.AppendResponse(GetHabbo().GetAvatarEffectsInventoryComponent().GetPacket());
                queuedServerMessage.SendResponse();

                Azure.GetGame().GetAchievementManager().TryProgressHabboClubAchievements(this);
                Azure.GetGame().GetAchievementManager().TryProgressRegistrationAchievements(this);
                Azure.GetGame().GetAchievementManager().TryProgressLoginAchievements(this);

                return true;
            }
            catch (Exception ex)
            {
                ServerLogManager.LogCriticalException($"Bug during user login: {ex}");
            }

            return false;
        }

        /// <summary>
        ///     Sends the notif with scroll.
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
        ///     Sends the broadcast message.
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
        ///     Sends the moderator message.
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
        ///     Sends the whisper.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="fromWired"></param>
        internal void SendWhisper(string message, bool fromWired = false)
        {
            if (GetHabbo() == null || GetHabbo().CurrentRoom == null)
                return;

            var roomUserByHabbo = GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(GetHabbo().UserName);

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
        ///     Sends the notif.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="title">The title.</param>
        /// <param name="picture">The picture.</param>
        internal void SendNotif(string message, string title = "Aviso", string picture = "")
        {
            SendMessage(GetBytesNotif(message, title, picture));
        }

        /// <summary>
        ///     Gets the bytes notif.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="title">The title.</param>
        /// <param name="picture">The picture.</param>
        /// <returns>System.Byte[].</returns>
        public static byte[] GetBytesNotif(string message, string title = "Aviso", string picture = "")
        {
            using (var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("SuperNotificationMessageComposer")))
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
        ///     Stops this instance.
        /// </summary>
        internal void Stop()
        {
            if (GetMessageHandler() != null)
                GetMessageHandler().Destroy();

            if (GetHabbo() != null)
                GetHabbo().OnDisconnect("disconnect");

            CurrentRoomUserId = -1;
            _messageHandler = null;
            _habbo = null;
            _connection = null;
        }

        /// <summary>
        ///     Disconnects the specified reason.
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

            _connection?.Dispose();
            _disconnected = true;
        }

        /// <summary>
        ///     Sends the message.
        /// </summary>
        /// <param name="message">The message.</param>
        internal void SendMessage(ServerMessage message)
        {
            if (message == null)
                return;

            if (GetConnection() == null)
                return;

            var bytes = message.GetReversedBytes();

            GetConnection().SendData(bytes);
        }

        /// <summary>
        ///     Sends the message.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        internal void SendMessage(byte[] bytes)
        {
            if (GetConnection() == null)
                return;

            GetConnection().SendData(bytes);
        }

        /// <summary>
        ///     Sends the message.
        /// </summary>
        /// <param name="type">The type.</param>
        internal void SendMessage(StaticMessage type)
        {
            if (GetConnection() == null)
                return;

            GetConnection().SendData(StaticMessagesManager.Get(type));
        }

        /// <summary>
        ///     Switches the parser request.
        /// </summary>
        private void SwitchParserRequest(byte[] data, int amountOfBytes)
        {
            if (_connection == null)
                return;

            if (_messageHandler == null)
                InitHandler();

            PacketParser.SetConnection(_connection, this);

            _connection.Parser.Dispose();
            _connection.Parser = PacketParser;
            _connection.Parser.HandlePacketData(data, amountOfBytes);
        }

        /// <summary>
        ///     Policies the request.
        /// </summary>
        private void PolicyRequest()
        {
            _connection.SendData(CrossDomainSettings.XmlPolicyBytes);
        }
    }
}