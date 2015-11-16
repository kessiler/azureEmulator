using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Azure.Game.Achievements.Structs;
using Azure.Game.Catalogs;
using Azure.Game.Groups.Interfaces;
using Azure.Game.Items.Interfaces;
using Azure.Game.Pets;
using Azure.Game.RoomBots;
using Azure.Game.Rooms.Data;
using Azure.Game.Users.Authenticator;
using Azure.Game.Users.Badges;
using Azure.Game.Users.Inventory;
using Azure.Game.Users.Messenger;
using Azure.Game.Users.Relationships;
using Azure.Game.Users.Subscriptions;

namespace Azure.Game.Users.UserDataManagement
{
    /// <summary>
    ///     Class UserDataFactory.
    /// </summary>
    internal class UserDataFactory
    {
        /// <summary>
        ///     Gets the user data.
        /// </summary>
        /// <param name="sessionTicket">The session ticket.</param>
        /// <param name="errorCode">The error code.</param>
        /// <returns>UserData.</returns>
        /// <exception cref="UserDataNotFoundException"></exception>
        internal static UserData GetUserData(string sessionTicket, out uint errorCode)
        {
            uint miniMailCount = 0;
            errorCode = 1;

            DataTable groupsTable;
            DataRow dataRow;
            DataTable achievementsTable;
            DataTable talentsTable;
            DataRow statsTable;
            DataTable favoritesTable;
            DataTable ignoresTable;
            DataTable tagsTable;
            DataRow subscriptionsRow;
            DataTable badgesTable;
            DataTable itemsTable;
            DataTable effectsTable;
            DataTable pollsTable;
            DataTable friendsTable;
            DataTable friendsRequestsTable;

            DataTable relationShipsTable;
            DataTable botsTable;
            DataTable questsTable;
            DataTable petsTable;

            DataTable myRoomsTable;

            uint userid;
            string userName;
            string look;

            using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery("SELECT * FROM users WHERE auth_ticket = @ticket");
                queryReactor.AddParameter("ticket", sessionTicket);
                dataRow = queryReactor.GetRow();
                if (dataRow == null)
                    return null;

                userid = Convert.ToUInt32(dataRow["id"]);
                userName = dataRow["username"].ToString();
                look = dataRow["look"].ToString();

                queryReactor.RunFastQuery($"UPDATE users SET online = '1' WHERE id = '{userid}'");

                if (Azure.GetGame().GetClientManager().GetClientByUserId(userid) != null)
                    Azure.GetGame()
                        .GetClientManager()
                        .GetClientByUserId(userid)
                        .Disconnect("User connected in other place");

                queryReactor.SetQuery(
                    $"SELECT `group`, `level`, progress FROM users_achievements WHERE userid = {Convert.ToUInt32(userid)}");
                achievementsTable = queryReactor.GetTable();

                queryReactor.SetQuery(
                    $"SELECT talent_id, talent_state FROM users_talents WHERE userid = {Convert.ToUInt32(userid)}");
                talentsTable = queryReactor.GetTable();

                queryReactor.SetQuery($"SELECT COUNT(*) FROM users_stats WHERE id = {Convert.ToUInt32(userid)}");

                if (int.Parse(queryReactor.GetString()) == 0)
                    queryReactor.RunFastQuery($"INSERT INTO users_stats (id) VALUES ({Convert.ToUInt32(userid)})");

                queryReactor.SetQuery($"SELECT room_id FROM users_favorites WHERE user_id = {Convert.ToUInt32(userid)}");
                favoritesTable = queryReactor.GetTable();

                queryReactor.SetQuery($"SELECT ignore_id FROM users_ignores WHERE user_id = {Convert.ToUInt32(userid)}");
                ignoresTable = queryReactor.GetTable();

                queryReactor.SetQuery($"SELECT tag FROM users_tags WHERE user_id = {Convert.ToUInt32(userid)}");
                tagsTable = queryReactor.GetTable();

                queryReactor.SetQuery(
                    $"SELECT subscription_id, timestamp_activated, timestamp_expire, timestamp_lastgift FROM users_subscriptions WHERE user_id = {Convert.ToUInt32(userid)} AND timestamp_expire > UNIX_TIMESTAMP() ORDER BY subscription_id DESC LIMIT 1");
                subscriptionsRow = queryReactor.GetRow();

                queryReactor.SetQuery($"SELECT * FROM users_badges WHERE user_id = {Convert.ToUInt32(userid)}");
                badgesTable = queryReactor.GetTable();

                queryReactor.SetQuery(
                    $"SELECT `items_rooms`.* , COALESCE(`items_groups`.`group_id`, 0) AS group_id FROM `items_rooms` LEFT OUTER JOIN `items_groups` ON `items_rooms`.`id` = `items_groups`.`id` WHERE room_id='0' AND user_id={Convert.ToUInt32(userid)} LIMIT 8000");
                itemsTable = queryReactor.GetTable();

                queryReactor.SetQuery($"SELECT * FROM users_effects WHERE user_id = {Convert.ToUInt32(userid)}");
                effectsTable = queryReactor.GetTable();

                queryReactor.SetQuery(
                    $"SELECT poll_id FROM users_polls WHERE user_id = {Convert.ToUInt32(userid)} GROUP BY poll_id;");
                pollsTable = queryReactor.GetTable();

                queryReactor.SetQuery(
                    string.Format(
                        "SELECT users.id,users.username,users.motto,users.look,users.last_online,users.hide_inroom,users.hide_online FROM users JOIN messenger_friendships ON users.id = messenger_friendships.user_one_id WHERE messenger_friendships.user_two_id = {0} UNION ALL SELECT users.id,users.username,users.motto,users.look,users.last_online,users.hide_inroom,users.hide_online FROM users JOIN messenger_friendships ON users.id = messenger_friendships.user_two_id WHERE messenger_friendships.user_one_id = {0}",
                        Convert.ToUInt32(userid)));
                friendsTable = queryReactor.GetTable();

                queryReactor.SetQuery($"SELECT * FROM users_stats WHERE id = {Convert.ToUInt32(userid)}");
                statsTable = queryReactor.GetRow();

                queryReactor.SetQuery(
                    $"SELECT messenger_requests.from_id,messenger_requests.to_id,users.Username, users.Look FROM users JOIN messenger_requests ON users.id = messenger_requests.from_id WHERE messenger_requests.to_id = {Convert.ToUInt32(userid)}");
                friendsRequestsTable = queryReactor.GetTable();

                queryReactor.SetQuery("SELECT * FROM rooms_data WHERE owner = @name LIMIT 150");
                queryReactor.AddParameter("name", userName);
                myRoomsTable = queryReactor.GetTable();

                queryReactor.SetQuery(
                    $"SELECT * FROM bots WHERE user_id = {Convert.ToUInt32(userid)} AND room_id = 0 AND ai_type='pet'");
                petsTable = queryReactor.GetTable();

                queryReactor.SetQuery(
                    $"SELECT quest_id, progress FROM users_quests_data WHERE user_id = {Convert.ToUInt32(userid)}");
                questsTable = queryReactor.GetTable();

                queryReactor.SetQuery(
                    $"SELECT * FROM bots WHERE user_id = {Convert.ToUInt32(userid)} AND room_id=0 AND ai_type='generic'");
                botsTable = queryReactor.GetTable();

                queryReactor.SetQuery(
                    $"SELECT group_id, rank, date_join FROM groups_members WHERE user_id = {Convert.ToUInt32(userid)}");
                groupsTable = queryReactor.GetTable();

                queryReactor.SetQuery(
                    "REPLACE INTO users_info(user_id, login_timestamp) VALUES(@userid, @login_timestamp)");
                queryReactor.AddParameter("userid", Convert.ToUInt32(userid));
                queryReactor.AddParameter("login_timestamp", Azure.GetUnixTimeStamp());
                queryReactor.RunQuery();

                queryReactor.SetQuery($"SELECT * FROM users_relationships WHERE user_id = {Convert.ToUInt32(userid)}");
                relationShipsTable = queryReactor.GetTable();

                queryReactor.RunFastQuery($"UPDATE users SET online='1' WHERE id = {Convert.ToUInt32(userid)} LIMIT 1");
            }

            var achievements = new Dictionary<string, UserAchievement>();

            foreach (DataRow row in achievementsTable.Rows)
            {
                var text = (string) row["group"];
                var level = (int) row["level"];
                var progress = (int) row["progress"];
                var value = new UserAchievement(text, level, progress);
                achievements.Add(text, value);
            }

            var talents = new Dictionary<int, UserTalent>();

            foreach (DataRow row in talentsTable.Rows)
            {
                var num2 = (int) row["talent_id"];
                var state = (int) row["talent_state"];
                var value2 = new UserTalent(num2, state);
                talents.Add(num2, value2);
            }

            var favorites = (from DataRow row in favoritesTable.Rows select (uint) row["room_id"]).ToList();
            var ignoreUsers = (from DataRow row in ignoresTable.Rows select (uint) row["ignore_id"]).ToList();
            var tags = (from DataRow row in tagsTable.Rows select row["tag"].ToString().Replace(" ", "")).ToList();

            var inventoryBots =
                (botsTable.Rows.Cast<DataRow>().Select(BotManager.GenerateBotFromRow)).ToDictionary(
                    roomBot => roomBot.BotId);
            var badges =
                (badgesTable.Rows.Cast<DataRow>()
                    .Select(dataRow8 => new Badge((string) dataRow8["badge_id"], (int) dataRow8["badge_slot"]))).ToList();

            Subscription subscriptions = null;

            if (subscriptionsRow != null)
                subscriptions = new Subscription((int) subscriptionsRow["subscription_id"],
                    (int) subscriptionsRow["timestamp_activated"], (int) subscriptionsRow["timestamp_expire"],
                    (int) subscriptionsRow["timestamp_lastgift"]);

            var items = (from DataRow row in itemsTable.Rows
                let id = Convert.ToUInt32(row["id"])
                let itemName = row["item_name"].ToString()
                where Azure.GetGame().GetItemManager().ContainsItemByName(itemName)
                let extraData = !DBNull.Value.Equals(row[4]) ? (string) row[4] : string.Empty
                let theGroup = Convert.ToUInt32(row["group_id"])
                let songCode = (string) row["songcode"]
                select new UserItem(id, itemName, extraData, theGroup, songCode)).ToList();

            var effects = (from DataRow row in effectsTable.Rows
                let effectId = (int) row["effect_id"]
                let totalDuration = (int) row["total_duration"]
                let activated = Azure.EnumToBool((string) row["is_activated"])
                let activateTimestamp = (double) row["activated_stamp"]
                let type = Convert.ToInt16(row["type"])
                select new AvatarEffect(effectId, totalDuration, activated, activateTimestamp, type)).ToList();

            var pollSuggested = new HashSet<uint>();

            foreach (var pId in from DataRow row in pollsTable.Rows select (uint) row["poll_id"])
                pollSuggested.Add(pId);

            var friends = new Dictionary<uint, MessengerBuddy>();
            var limit = (friendsTable.Rows.Count - 700);

            if (limit > 0)
            {
                using (var queryreactor2 = Azure.GetDatabaseManager().GetQueryReactor())
                {
                    queryreactor2.RunFastQuery(string.Concat("DELETE FROM messenger_friendships WHERE user_one_id=",
                        userid, " OR user_two_id=", userid, " LIMIT ", limit));
                    queryreactor2.SetQuery(
                        string.Concat(
                            "SELECT users.id,users.username,users.motto,users.look,users.last_online,users.hide_inroom,users.hide_online FROM users JOIN messenger_friendships ON users.id = messenger_friendships.user_one_id WHERE messenger_friendships.user_two_id = ",
                            userid,
                            " UNION ALL SELECT users.id,users.username,users.motto,users.look,users.last_online,users.hide_inroom,users.hide_online FROM users JOIN messenger_friendships ON users.id = messenger_friendships.user_two_id WHERE messenger_friendships.user_one_id = ",
                            userid));
                    friendsTable = queryreactor2.GetTable();
                }
            }

            foreach (DataRow row in friendsTable.Rows)
            {
                var num4 = Convert.ToUInt32(row["id"]);
                var pUsername = (string) row["username"];
                var pLook = (string) row["look"];
                var pMotto = (string) row["motto"];
                var pLastOnline = Convert.ToInt32(row["last_online"]);
                var pAppearOffline = Azure.EnumToBool(row["hide_online"].ToString());
                var pHideInroom = Azure.EnumToBool(row["hide_inroom"].ToString());

                if (num4 != userid && !friends.ContainsKey(num4))
                    friends.Add(num4,
                        new MessengerBuddy(num4, pUsername, pLook, pMotto, pLastOnline, pAppearOffline, pHideInroom));
            }

            var friendsRequests = new Dictionary<uint, MessengerRequest>();

            foreach (DataRow row in friendsRequestsTable.Rows)
            {
                var num5 = Convert.ToUInt32(row["from_id"]);
                var num6 = Convert.ToUInt32(row["to_id"]);
                var pUsername2 = row["username"].ToString();
                var pLook = row["look"].ToString();

                if (num5 != userid)
                {
                    if (!friendsRequests.ContainsKey(num5))
                        friendsRequests.Add(num5, new MessengerRequest(userid, num5, pUsername2, pLook));
                    else if (!friendsRequests.ContainsKey(num6))
                        friendsRequests.Add(num6, new MessengerRequest(userid, num6, pUsername2, pLook));
                }
            }

            var myRooms = new HashSet<RoomData>();

            foreach (DataRow row in myRoomsTable.Rows)
            {
                var roomId = Convert.ToUInt32(row["id"]);
                myRooms.Add(Azure.GetGame().GetRoomManager().FetchRoomData(roomId, row));
            }

            var pets = new Dictionary<uint, Pet>();

            foreach (DataRow row in petsTable.Rows)
            {
                using (var queryreactor3 = Azure.GetDatabaseManager().GetQueryReactor())
                {
                    queryreactor3.SetQuery($"SELECT * FROM pets_data WHERE id={row[0]} LIMIT 1");
                    var row3 = queryreactor3.GetRow();
                    if (row3 == null)
                        continue;
                    var pet = CatalogManager.GeneratePetFromRow(row, row3);
                    pets.Add(pet.PetId, pet);
                }
            }

            var quests = new Dictionary<uint, int>();
            foreach (DataRow row in questsTable.Rows)
            {
                var key = Convert.ToUInt32(row["quest_id"]);
                var value3 = (int) row["progress"];

                if (quests.ContainsKey(key))
                    quests.Remove(key);

                quests.Add(key, value3);
            }

            var groups = new HashSet<GroupMember>();

            foreach (DataRow row in groupsTable.Rows)
                groups.Add(new GroupMember(userid, userName, look, (uint) row["group_id"], Convert.ToInt16(row["rank"]),
                    (int) row["date_join"]));

            var relationShips = relationShipsTable.Rows.Cast<DataRow>()
                .ToDictionary(row => (int) row[0],
                    row => new Relationship((int) row[0], (int) row[2], Convert.ToInt32(row[3].ToString())));

            var user = HabboFactory.GenerateHabbo(dataRow, statsTable, groups);
            errorCode = 0;

            if (user.Rank >= Azure.StaffAlertMinRank)
                friends.Add(0,
                    new MessengerBuddy(0, "Staff Chat",
                        "hr-831-45.fa-1206-91.sh-290-1331.ha-3129-100.hd-180-2.cc-3039-73.ch-3215-92.lg-270-73",
                        string.Empty, 0, false, true));

            return new UserData(userid, achievements, talents, favorites, ignoreUsers, tags, subscriptions, badges,
                items, effects, friends, friendsRequests, myRooms, pets, quests, user, inventoryBots, relationShips,
                pollSuggested, miniMailCount);
        }

        /// <summary>
        ///     Gets the user data.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>UserData.</returns>
        internal static UserData GetUserData(int userId)
        {
            DataRow dataRow;
            uint num;
            DataRow row;
            DataTable table;

            using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery($"SELECT * FROM users WHERE id = '{userId}'");

                dataRow = queryReactor.GetRow();
                Azure.GetGame().GetClientManager().LogClonesOut(Convert.ToUInt32(userId));

                if (dataRow == null)
                    return null;

                num = Convert.ToUInt32(dataRow["id"]);

                if (Azure.GetGame().GetClientManager().GetClientByUserId(num) != null)
                    return null;

                queryReactor.SetQuery($"SELECT group_id,rank,date_join FROM groups_members WHERE user_id={userId}");
                queryReactor.GetTable();
                queryReactor.SetQuery($"SELECT * FROM users_stats WHERE id={num} LIMIT 1");
                row = queryReactor.GetRow();

                queryReactor.SetQuery("SELECT * FROM users_relationships WHERE user_id=@id");
                queryReactor.AddParameter("id", num);
                table = queryReactor.GetTable();
            }

            var achievements = new Dictionary<string, UserAchievement>();
            var talents = new Dictionary<int, UserTalent>();
            var favouritedRooms = new List<uint>();
            var ignores = new List<uint>();
            var tags = new List<string>();
            var badges = new List<Badge>();
            var inventory = new List<UserItem>();
            var effects = new List<AvatarEffect>();
            var friends = new Dictionary<uint, MessengerBuddy>();
            var requests = new Dictionary<uint, MessengerRequest>();
            var rooms = new HashSet<RoomData>();
            var pets = new Dictionary<uint, Pet>();
            var quests = new Dictionary<uint, int>();
            var bots = new Dictionary<uint, RoomBot>();
            var group = new HashSet<GroupMember>();
            var pollData = new HashSet<uint>();

            var dictionary = table.Rows.Cast<DataRow>()
                .ToDictionary(dataRow2 => (int) dataRow2[0],
                    dataRow2 =>
                        new Relationship((int) dataRow2[0], (int) dataRow2[2], Convert.ToInt32(dataRow2[3].ToString())));
            var user = HabboFactory.GenerateHabbo(dataRow, row, group);

            return new UserData(num, achievements, talents, favouritedRooms, ignores, tags, null, badges, inventory,
                effects, friends, requests, rooms, pets, quests, user, bots, dictionary, pollData, 0);
        }
    }
}