#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Azure.HabboHotel.GameClients;
using Azure.HabboHotel.Rooms;
using Azure.Messages;

#endregion

namespace Azure.HabboHotel.Navigators
{
    /// <summary>
    /// Class SearchResultList.
    /// </summary>
    internal class SearchResultList
    {
        /// <summary>
        /// Serializes the search result list flatcats.
        /// </summary>
        /// <param name="flatCatId">The flat cat identifier.</param>
        /// <param name="direct">if set to <c>true</c> [direct].</param>
        /// <param name="message">The message.</param>
        internal static void SerializeSearchResultListFlatcats(int flatCatId, bool direct, ServerMessage message)
        {
            var flatCat = Azure.GetGame().GetNavigator().GetFlatCat(flatCatId);
            if (flatCat == null) return;
            message.AppendString(string.Format("category__{0}", flatCat.Caption));
            message.AppendString(flatCat.Caption);
            message.AppendInteger(0);
            message.AppendBool(true);
            message.AppendInteger(-1);
            try
            {
                var rooms = Azure.GetGame().GetRoomManager().GetActiveRooms();
                Azure.GetGame()
                    .GetNavigator()
                    .SerializeNavigatorPopularRoomsNews(ref message, rooms, flatCatId, direct);
            }
            catch
            {
                message.AppendInteger(0);
            }
        }

        /// <summary>
        /// Serializes the promotions result list flatcats.
        /// </summary>
        /// <param name="flatCatId">The flat cat identifier.</param>
        /// <param name="direct">if set to <c>true</c> [direct].</param>
        /// <param name="message">The message.</param>
        internal static void SerializePromotionsResultListFlatcats(int flatCatId, bool direct, ServerMessage message)
        {
            var flatCat = Azure.GetGame().GetNavigator().GetFlatCat(flatCatId);
            message.AppendString("new_ads");
            message.AppendString(flatCat.Caption);
            message.AppendInteger(0);
            message.AppendBool(true);
            message.AppendInteger(-1);
            try
            {
                var rooms = Azure.GetGame().GetRoomManager().GetEventRooms();
                Azure.GetGame()
                    .GetNavigator()
                    .SerializeNavigatorPopularRoomsNews(ref message, rooms, flatCatId, direct);
            }
            catch
            {
                message.AppendInteger(0);
            }
        }

        /// <summary>
        /// Serializes the search result list statics.
        /// </summary>
        /// <param name="staticId">The static identifier.</param>
        /// <param name="direct">if set to <c>true</c> [direct].</param>
        /// <param name="message">The message.</param>
        /// <param name="session">The session.</param>
        internal static void SerializeSearchResultListStatics(string staticId, bool direct, ServerMessage message,
                                                              GameClient session)
        {
            if (string.IsNullOrEmpty(staticId) || staticId == "official") staticId = "official_view";
            if (staticId != "hotel_view" && staticId != "roomads_view" && staticId != "myworld_view" &&
                !staticId.StartsWith("category__") && staticId != "official_view")
            {
                message.AppendString(staticId); // code
                message.AppendString(""); // title
                message.AppendInteger(1); // 0 : no button - 1 : Show More - 2 : Show Back button
                message.AppendBool(staticId != "my" && staticId != "popular" && staticId != "official-root"); // collapsed
                message.AppendInteger(staticId == "official-root" ? 1 : 0); // 0 : list - 1 : thumbnail
            }
            KeyValuePair<RoomData, uint>[] rooms;
            switch (staticId)
            {
                case "hotel_view":
                    {
                        SerializeSearchResultListStatics("popular", false, message, session);
                        foreach (FlatCat flat in Azure.GetGame().GetNavigator().PrivateCategories.Values) SerializeSearchResultListFlatcats(flat.Id, false, message);
                        break;
                    }
                case "myworld_view":
                    {
                        SerializeSearchResultListStatics("my", false, message, session);
                        SerializeSearchResultListStatics("favorites", false, message, session);
                        SerializeSearchResultListStatics("my_groups", false, message, session);
                        SerializeSearchResultListStatics("history", false, message, session);
                        SerializeSearchResultListStatics("friends_rooms", false, message, session);
                        break;
                    }
                case "roomads_view":
                    {
                        foreach (FlatCat flat in Azure.GetGame().GetNavigator().PrivateCategories.Values) SerializePromotionsResultListFlatcats(flat.Id, false, message);
                        SerializeSearchResultListStatics("top_promotions", false, message, session);
                        break;
                    }
                case "official_view":
                    {
                        SerializeSearchResultListStatics("official-root", false, message, session);
                        SerializeSearchResultListStatics("staffpicks", false, message, session);
                        break;
                    }
                case "official-root":
                    {
                        message.AppendServerMessage(Azure.GetGame().GetNavigator().NewPublicRooms);
                        break;
                    }
                case "staffpicks":
                    {
                        message.AppendServerMessage(Azure.GetGame().GetNavigator().NewStaffPicks);
                        break;
                    }
                case "my":
                    {
                        var i = 0;
                        message.StartArray();
                        foreach (var data in session.GetHabbo().UsersRooms.Where(data => data != null))
                        {
                            data.Serialize(message, false);
                            message.SaveArray();
                            if (i++ == (direct ? 100 : 8)) break;
                        }
                        message.EndArray();
                        break;
                    }
                case "favorites":
                    {
                        if (session.GetHabbo().FavoriteRooms == null)
                        {
                            message.AppendInteger(0);
                            return;
                        }

                        var i = 0;
                        message.AppendInteger(session.GetHabbo().FavoriteRooms.Count > (direct ? 40 : 8)
                            ? (direct ? 40 : 8)
                            : session.GetHabbo().FavoriteRooms.Count);
                        foreach (
                            var data in
                                session.GetHabbo()
                                    .FavoriteRooms.Select(
                                        dataId => Azure.GetGame().GetRoomManager().GenerateRoomData(dataId))
                                    .Where(data => data != null))
                        {
                            data.Serialize(message, false);
                            i++;
                            if (i == (direct ? 40 : 8)) break;
                        }
                        break;
                    }
                case "friends_rooms":
                    {
                        var i = 0;
                        if (session == null || session.GetHabbo() == null || session.GetHabbo().GetMessenger() == null ||
                            session.GetHabbo().GetMessenger().GetActiveFriendsRooms() == null)
                        {
                            message.AppendInteger(0);
                            return;
                        }
                        var roomsFriends =
                            session.GetHabbo()
                                .GetMessenger()
                                .GetActiveFriendsRooms()
                                .OrderByDescending(p => p.UsersNow)
                                .Take((direct ? 40 : 8))
                                .ToList();
                        message.AppendInteger(roomsFriends.Count);
                        foreach (var data in roomsFriends.Where(data => data != null))
                        {
                            data.Serialize(message, false);

                            i++;
                            if (i == (direct ? 40 : 8)) break;
                        }
                        break;
                    }
                case "recommended":
                    {
                        break;
                    }
                case "popular":
                    {
                        try
                        {
                            rooms = Azure.GetGame().GetRoomManager().GetActiveRooms();
                            if (rooms == null)
                            {
                                message.AppendInteger(0);
                                break;
                            }
                            message.AppendInteger(rooms.Length);
                            foreach (var room in rooms) room.Key.Serialize(message, false);
                        }
                        catch (Exception e)
                        {
                            Writer.Writer.LogException(e.ToString());
                            message.AppendInteger(0);
                        }
                        break;
                    }
                case "top_promotions":
                    {
                        try
                        {
                            rooms = Azure.GetGame().GetRoomManager().GetEventRooms();
                            message.AppendInteger(rooms.Length);
                            foreach (var room in rooms) room.Key.Serialize(message, false);
                        }
                        catch
                        {
                            message.AppendInteger(0);
                        }
                        break;
                    }
                case "my_groups":
                    {
                        var i = 0;
                        message.StartArray();
                        foreach (var data in from xGroupId in session.GetHabbo().MyGroups
                                             select Azure.GetGame().GetGroupManager().GetGroup(xGroupId)
                                                 into xGroup
                                             where xGroup != null
                                             select Azure.GetGame().GetRoomManager().GenerateRoomData(xGroup.RoomId)
                                                     into data
                                             where data != null
                                             select data)
                        {
                            data.Serialize(message, false);
                            message.SaveArray();
                            if (i++ == (direct ? 40 : 8)) break;
                        }
                        message.EndArray();
                        break;
                    }
                case "history":
                    {
                        var i = 0;
                        message.StartArray();
                        foreach (var roomData in session.GetHabbo()
                            .RecentlyVisitedRooms.Select(
                                roomId => Azure.GetGame().GetRoomManager().GenerateRoomData(roomId)).Where(roomData => roomData != null))
                        {
                            roomData.Serialize(message, false);
                            message.SaveArray();

                            if (i++ == (direct ? 40 : 8)) break;
                        }

                        message.EndArray();
                        break;
                    }
                default:
                    {
                        if (staticId.StartsWith("category__"))
                        {
                            SerializeSearchResultListFlatcats(
                                Azure.GetGame()
                                    .GetNavigator()
                                    .GetFlatCatIdByName(staticId.Replace("category__", "")), true, message);
                        }
                        else message.AppendInteger(0);
                        break;
                    }
            }
        }

        /// <summary>
        /// Serializes the searches.
        /// </summary>
        /// <param name="searchQuery">The search query.</param>
        /// <param name="message">The message.</param>
        /// <param name="session">The session.</param>
        internal static void SerializeSearches(string searchQuery, ServerMessage message, GameClient session)
        {
            message.AppendString("");
            message.AppendString(searchQuery);
            message.AppendInteger(2);
            message.AppendBool(false);
            message.AppendInteger(0);
            var containsOwner = false;
            var containsGroup = false;
            if (searchQuery.StartsWith("owner:"))
            {
                searchQuery = searchQuery.Replace("owner:", string.Empty);
                containsOwner = true;
            }
            else if (searchQuery.StartsWith("group:"))
            {
                searchQuery = searchQuery.Replace("group:", string.Empty);
                containsGroup = true;
            }
            var rooms = new List<RoomData>();
            if (!containsOwner)
            {
                var initForeach = false;
                try
                {
                    if (Azure.GetGame().GetRoomManager().GetActiveRooms() != null &&
                        Azure.GetGame().GetRoomManager().GetActiveRooms().Any())
                        initForeach = true;
                }
                catch
                {
                    initForeach = false;
                }
                if (initForeach)
                {
                    foreach (var rms in Azure.GetGame().GetRoomManager().GetActiveRooms())
                    {
                        if (rms.Key.Name.ToLower().Contains(searchQuery.ToLower()) && rooms.Count <= 50) rooms.Add(rms.Key);
                        else break;
                    }
                }
            }
            if (rooms.Count < 50 || containsOwner || containsGroup)
            {
                DataTable dTable;
                using (var dbClient = Azure.GetDatabaseManager().GetQueryReactor())
                {
                    if (containsOwner)
                    {
                        dbClient.SetQuery(
                            "SELECT * FROM rooms_data WHERE owner LIKE @query AND roomtype = 'private' LIMIT 50");
                        dbClient.AddParameter("query", searchQuery);
                        dTable = dbClient.GetTable();
                    }
                    else if (containsGroup)
                    {
                        dbClient.SetQuery(
                            "SELECT * FROM rooms_data JOIN groups_data ON rooms_data.id = groups_data.room_id WHERE groups_data.name LIKE @query AND roomtype = 'private' LIMIT 50");
                        dbClient.AddParameter("query", string.Format("%{0}%", searchQuery));
                        dTable = dbClient.GetTable();
                    }
                    else
                    {
                        dbClient.SetQuery(
                            string.Format(
                                "SELECT * FROM rooms_data WHERE caption LIKE @query AND roomtype = 'private' LIMIT {0}",
                                50 - rooms.Count));
                        dbClient.AddParameter("query", searchQuery);
                        dTable = dbClient.GetTable();
                    }
                }
                if (dTable != null)
                {
                    foreach (
                        var rData in
                            dTable.Rows.Cast<DataRow>()
                                .Select(
                                    row =>
                                        Azure.GetGame().GetRoomManager().FetchRoomData(Convert.ToUInt32(row["id"]), row))
                                .Where(rData => !rooms.Contains(rData)))
                        rooms.Add(rData);
                }
            }
            message.AppendInteger(rooms.Count);
            foreach (var data in rooms.Where(data => data != null)) data.Serialize(message, false);
        }
    }
}