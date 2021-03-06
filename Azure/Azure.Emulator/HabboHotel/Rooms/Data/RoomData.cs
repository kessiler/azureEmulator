using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using Azure.Configuration;
using Azure.HabboHotel.GameClients.Interfaces;
using Azure.HabboHotel.Groups.Interfaces;
using Azure.HabboHotel.Rooms.Chat;
using Azure.Messages;
using Azure.Messages.Parsers;

namespace Azure.HabboHotel.Rooms.Data
{
    /// <summary>
    ///     Class RoomData.
    /// </summary>
    internal class RoomData
    {
        /// <summary>
        ///     The _model
        /// </summary>
        private RoomModel _model;

        /// <summary>
        ///     The allow pets
        /// </summary>
        internal bool AllowPets, AllowPetsEating, AllowWalkThrough, HideWall;

        /// <summary>
        ///     The allow rights override
        /// </summary>
        internal bool AllowRightsOverride;

        /// <summary>
        ///     The category
        /// </summary>
        internal int Category;

        /// <summary>
        ///     The cc ts
        /// </summary>
        internal string CcTs;

        /// <summary>
        ///     The chat balloon
        /// </summary>
        internal uint ChatBalloon, ChatSpeed, ChatMaxDistance, ChatFloodProtection;

        /// <summary>
        ///     The chat type
        /// </summary>
        internal int ChatType;

        internal int CompetitionStatus, CompetitionVotes;

        /// <summary>
        ///     The description
        /// </summary>
        internal string Description;

        internal bool DisablePull = false;

        internal bool DisablePush = false;

        /// <summary>
        ///     The event
        /// </summary>
        internal RoomEvent Event;

        /// <summary>
        ///     The floor
        /// </summary>
        internal string Floor;

        /// <summary>
        ///     The floor thickness
        /// </summary>
        internal int FloorThickness;

        /// <summary>
        ///     The game identifier
        /// </summary>
        internal int GameId;

        /// <summary>
        ///     The group
        /// </summary>
        internal Guild Group;

        /// <summary>
        ///     The group identifier
        /// </summary>
        internal uint GroupId;

        /// <summary>
        ///     The identifier
        /// </summary>
        internal uint Id;

        /// <summary>
        ///     The land scape
        /// </summary>
        internal string LandScape;

        public DateTime LastUsed = DateTime.Now;

        /// <summary>
        ///     The model name
        /// </summary>
        internal string ModelName;

        /// <summary>
        ///     The name
        /// </summary>
        internal string Name;

        /// <summary>
        ///     The owner
        /// </summary>
        internal string Owner;

        /// <summary>
        ///     The owner identifier
        /// </summary>
        internal int OwnerId;

        /// <summary>
        ///     The pass word
        /// </summary>
        internal string PassWord;

        /// <summary>
        ///     The room chat
        /// </summary>
        internal ConcurrentStack<Chatlog> RoomChat;

        /// <summary>
        ///     The score
        /// </summary>
        internal int Score;

        /// <summary>
        ///     The state
        /// </summary>
        internal int State;

        /// <summary>
        ///     The tags
        /// </summary>
        internal List<string> Tags;

        /// <summary>
        ///     The trade state
        /// </summary>
        internal int TradeState;

        /// <summary>
        ///     The type
        /// </summary>
        internal string Type;

        /// <summary>
        ///     The users now
        /// </summary>
        internal uint UsersNow, UsersMax;

        /// <summary>
        ///     The wall height
        /// </summary>
        internal int WallHeight;

        /// <summary>
        ///     The wall paper
        /// </summary>
        internal string WallPaper;

        /// <summary>
        ///     The wall thickness
        /// </summary>
        internal int WallThickness;

        /// <summary>
        ///     The who can ban
        /// </summary>
        internal int WhoCanBan;

        /// <summary>
        ///     The who can kick
        /// </summary>
        internal int WhoCanKick;

        /// <summary>
        ///     The who can mute
        /// </summary>
        internal int WhoCanMute;

        /// <summary>
        ///     The word filter
        /// </summary>
        internal List<string> WordFilter;

        /// <summary>
        ///     Gets the tag count.
        /// </summary>
        /// <value>The tag count.</value>
        internal int TagCount => Tags.Count;

        /// <summary>
        ///     Gets a value indicating whether this instance has event.
        /// </summary>
        /// <value><c>true</c> if this instance has event; otherwise, <c>false</c>.</value>
        internal bool HasEvent => false;

        /// <summary>
        ///     Gets the model.
        /// </summary>
        /// <value>The model.</value>
        internal RoomModel Model => _model ?? (_model = Azure.GetGame().GetRoomManager().GetModel(ModelName, Id));

        /// <summary>
        ///     Resets the model.
        /// </summary>
        internal void ResetModel()
        {
            _model = Azure.GetGame().GetRoomManager().GetModel(ModelName, Id);
        }

        /// <summary>
        ///     Fills the null.
        /// </summary>
        /// <param name="id">The identifier.</param>
        internal void FillNull(uint id)
        {
            Id = id;
            Name = "Unknown Room";
            Description = "-";
            Type = "private";
            Owner = "-";
            Category = 0;
            UsersNow = 0;
            UsersMax = 0;
            ModelName = "NO_MODEL";
            CcTs = string.Empty;
            Score = 0;
            Tags = new List<string>();
            AllowPets = true;
            AllowPetsEating = false;
            AllowWalkThrough = true;
            HideWall = false;
            PassWord = string.Empty;
            WallPaper = "0.0";
            Floor = "0.0";
            LandScape = "0.0";
            WallThickness = 0;
            FloorThickness = 0;
            Group = null;
            AllowRightsOverride = false;
            Event = null;
            GameId = 0;
            WhoCanBan = 0;
            WhoCanKick = 0;
            WhoCanMute = 0;
            TradeState = 2;
            State = 0;
            RoomChat = new ConcurrentStack<Chatlog>();
            WordFilter = new List<string>();
            WallHeight = -1;
            _model = Azure.GetGame().GetRoomManager().GetModel(ModelName, Id);
            CompetitionStatus = 0;
        }

        /// <summary>
        ///     Fills the specified row.
        /// </summary>
        /// <param name="row">The row.</param>
        internal void Fill(DataRow row)
        {
            try
            {
                Id = Convert.ToUInt32(row["id"]);
                Name = (string) row["caption"];
                PassWord = (string) row["password"];
                Description = (string) row["description"];
                Type = (string) row["roomtype"];
                Owner = (string) row["owner"];
                OwnerId = 0;
                RoomChat = new ConcurrentStack<Chatlog>();
                WordFilter = new List<string>();

                using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
                {
                    queryReactor.SetQuery("SELECT id FROM users WHERE username = @name");
                    queryReactor.AddParameter("name", Owner);

                    var integer = Convert.ToUInt32(queryReactor.GetInteger());

                    OwnerId = integer != uint.MinValue ? Convert.ToInt32(integer) : 0;

                    queryReactor.SetQuery($"SELECT user_id, message, timestamp FROM users_chatlogs WHERE room_id = {Id} ORDER BY timestamp ASC LIMIT 150");
                    var table = queryReactor.GetTable();

                    foreach (DataRow dataRow in table.Rows)
                        RoomChat.Push(new Chatlog((uint) dataRow[0], (string) dataRow[1], Azure.UnixToDateTime(int.Parse(dataRow[2].ToString())), false));

                    queryReactor.SetQuery($"SELECT word FROM rooms_wordfilter WHERE room_id = {Id}");
                    var tableFilter = queryReactor.GetTable();

                    foreach (DataRow dataRow in tableFilter.Rows)
                        WordFilter.Add(dataRow["word"].ToString());
                }

                var roomState = row["state"].ToString().ToLower();

                switch (roomState)
                {
                    case "locked":
                        State = 1;
                        break;
                    case "password":
                        State = 2;
                        break;
                    default:
                        State = 0;
                        break;
                }

                ModelName = (string) row["model_name"];
                WallPaper = (string) row["wallpaper"];
                Floor = (string) row["floor"];
                LandScape = (string) row["landscape"];
                CcTs = (string) row["public_ccts"];

                int.TryParse(row["trade_state"].ToString(), out TradeState);
                int.TryParse(row["category"].ToString(), out Category);
                int.TryParse(row["walls_height"].ToString(), out WallHeight);
                int.TryParse(row["score"].ToString(), out Score);
                int.TryParse(row["floorthick"].ToString(), out FloorThickness);
                int.TryParse(row["wallthick"].ToString(), out WallThickness);
                int.TryParse(row["chat_type"].ToString(), out ChatType);
                int.TryParse(row["game_id"].ToString(), out GameId);
                int.TryParse(row["mute_settings"].ToString(), out WhoCanMute);
                int.TryParse(row["kick_settings"].ToString(), out WhoCanKick);
                int.TryParse(row["ban_settings"].ToString(), out WhoCanBan);

                uint.TryParse(row["users_now"].ToString(), out UsersNow);
                uint.TryParse(row["users_max"].ToString(), out UsersMax);
                uint.TryParse(row["group_id"].ToString(), out GroupId);
                uint.TryParse(row["chat_balloon"].ToString(), out ChatBalloon);
                uint.TryParse(row["chat_speed"].ToString(), out ChatSpeed);
                uint.TryParse(row["chat_max_distance"].ToString(), out ChatMaxDistance);
                uint.TryParse(row["chat_flood_protection"].ToString(), out ChatFloodProtection);

                AllowPets = Azure.EnumToBool(row["allow_pets"].ToString());
                AllowPetsEating = Azure.EnumToBool(row["allow_pets_eat"].ToString());
                AllowWalkThrough = Azure.EnumToBool(row["allow_walkthrough"].ToString());
                HideWall = Azure.EnumToBool(row["hidewall"].ToString());

                AllowRightsOverride = false;

                Group = Azure.GetGame().GetGroupManager().GetGroup(GroupId);
                Event = Azure.GetGame().GetRoomEvents().GetEvent(Id);
                _model = Azure.GetGame().GetRoomManager().GetModel(ModelName, Id);
                CompetitionStatus = 0;

                /*
                var dictionary = new Dictionary<int, int>();
                if (!string.IsNullOrEmpty(row["icon_items"].ToString()))
                {
                    string[] array = row["icon_items"].ToString().Split('|');
                    foreach (string text in array)
                    {
                        if (string.IsNullOrEmpty(text))
                        {
                            continue;
                        }
                        string[] array2 = text.Replace('.', ',').Split(',');
                        int key = 0;
                        int value = 0;
                        int.TryParse(array2[0], out key);
                        if (array2.Length > 1)
                        {
                            int.TryParse(array2[1], out value);
                        }
                        try
                        {
                            if (!dictionary.ContainsKey(key))
                            {
                                dictionary.Add(key, value);
                            }
                        }
                        catch (Exception ex)
                        {
                            Logging.LogException("Exception: " + ex + "[" + text + "]");
                        }
                    }
                }
                 */

                Tags = new List<string>();

                //if (row.IsNull("tags") || !string.IsNullOrEmpty(row["tags"].ToString()))
                // @issue 96
                if (row.IsNull("tags") || string.IsNullOrEmpty(row["tags"].ToString()))
                    return;

                foreach (var item in row["tags"].ToString().Split(','))
                    Tags.Add(item);
            }
            catch (Exception ex)
            {
                Logging.LogException("Exception on RoomData Loading (Fill Void): " + ex);
                Logging.HandleException(ex, "Azure.HabboHotel.Rooms.RoomData");
            }
        }

        /// <summary>
        ///     Serializes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="showEvents">if set to <c>true</c> [show events].</param>
        /// <param name="enterRoom"></param>
        internal void Serialize(ServerMessage message, bool showEvents = false, bool enterRoom = false)
        {
            message.AppendInteger(Id);
            message.AppendString(Name);
            message.AppendInteger(OwnerId);
            message.AppendString(Owner);
            message.AppendInteger(State);
            message.AppendInteger(UsersNow);
            message.AppendInteger(UsersMax);
            message.AppendString(Description);
            message.AppendInteger(TradeState);
            message.AppendInteger(Score);
            message.AppendInteger(0); // Ranking
            message.AppendInteger(Category);

            message.AppendInteger(TagCount);
            foreach (var current in Tags) message.AppendString(current);

            string imageData = null;

            var enumType = enterRoom ? 32 : 0;
            var publicItem = Azure.GetGame().GetNavigator().GetPublicItem(Id);
            if (publicItem != null && !string.IsNullOrEmpty(publicItem.Image))
            {
                imageData = publicItem.Image;
                enumType += 1;
            }

            if (Group != null) enumType += 2;
            if (showEvents && Event != null) enumType += 4;
            if (Type == "private") enumType += 8;
            if (AllowPets) enumType += 16;
            message.AppendInteger(enumType);

            if (imageData != null)
            {
                message.AppendString(imageData);
            }
            if (Group != null)
            {
                message.AppendInteger(Group.Id);
                message.AppendString(Group.Name);
                message.AppendString(Group.Badge);
            }
            if (showEvents && Event != null)
            {
                message.AppendString(Event.Name);
                message.AppendString(Event.Description);
                message.AppendInteger((int) Math.Floor((Event.Time - Azure.GetUnixTimeStamp())/60.0));
            }
        }

        /// <summary>
        ///     Serializes the room data.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="session">The session.</param>
        /// <param name="isNotReload">if set to <c>true</c> [from view].</param>
        /// <param name="sendRoom">if set to <c>true</c> [send room].</param>
        /// <param name="show">if set to <c>true</c> [show].</param>
        internal void SerializeRoomData(ServerMessage message, GameClient session, bool isNotReload,
            bool? sendRoom = false, bool show = true)
        {
            var room = Azure.GetGame().GetRoomManager().GetRoom(session.GetHabbo().CurrentRoomId);

            message.Init(LibraryParser.OutgoingRequest("RoomDataMessageComposer"));
            message.AppendBool(show); //flatId
            Serialize(message, true, !isNotReload);
            message.AppendBool(isNotReload);
            message.AppendBool(Azure.GetGame().GetNavigator() != null &&
                               Azure.GetGame().GetNavigator().GetPublicItem(Id) != null); // staffPick
            message.AppendBool(!isNotReload || session.GetHabbo().HasFuse("fuse_mod")); // bypass bell, pass ...
            message.AppendBool(room != null && room.RoomMuted); //roomMuted
            message.AppendInteger(WhoCanMute);
            message.AppendInteger(WhoCanKick);
            message.AppendInteger(WhoCanBan);
            message.AppendBool(room != null && room.CheckRights(session, true));
            message.AppendInteger(ChatType);
            message.AppendInteger(ChatBalloon);
            message.AppendInteger(ChatSpeed);
            message.AppendInteger(ChatMaxDistance);
            message.AppendInteger(ChatFloodProtection);
            if (sendRoom == null) return;

            if (sendRoom.Value)
            {
                if (Azure.GetGame().GetRoomManager().GetRoom(Id) != null)
                    Azure.GetGame().GetRoomManager().GetRoom(Id).SendMessage(message);
            }
            else session.SendMessage(message);
        }
    }
}