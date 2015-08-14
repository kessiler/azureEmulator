#region

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Text;
using Azure.HabboHotel.GameClients;
using Azure.HabboHotel.Groups.Structs;
using Azure.HabboHotel.Rooms;
using Azure.Messages;
using Azure.Messages.Parsers;

#endregion

namespace Azure.HabboHotel.Groups
{
    /// <summary>
    /// Class GroupManager.
    /// </summary>
   internal class GroupManager
    {
        /// <summary>
        /// The bases
        /// </summary>
        internal HashSet<GroupBases> Bases;
        /// <summary>
        /// The symbols
        /// </summary>
        internal HashSet<GroupSymbols> Symbols;
        /// <summary>
        /// The base colours
        /// </summary>
        internal HashSet<GroupBaseColours> BaseColours;
        /// <summary>
        /// The symbol colours
        /// </summary>
        internal HybridDictionary SymbolColours;
        /// <summary>
        /// The back ground colours
        /// </summary>
        internal HybridDictionary BackGroundColours;
        /// <summary>
        /// The groups
        /// </summary>
        internal HybridDictionary Groups;

        /// <summary>
        /// Initializes the groups.
        /// </summary>
        internal void InitGroups()
        {
            Bases = new HashSet<GroupBases>();
            Symbols = new HashSet<GroupSymbols>();
            BaseColours = new HashSet<GroupBaseColours>();
            SymbolColours = new HybridDictionary();
            BackGroundColours = new HybridDictionary();
            Groups = new HybridDictionary();
            ClearInfo();
            using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery("SELECT * FROM groups_badges_parts ORDER BY id");
                var table = queryReactor.GetTable();
                if (table == null)
                    return;
                foreach (DataRow row in table.Rows)
                    switch (row["type"].ToString().ToLower())
                    {
                        case "base":
                            Bases.Add(new GroupBases(int.Parse(row["id"].ToString()), row["code"].ToString(),
                                row["code2"].ToString()));
                            break;

                        case "symbol":
                            Symbols.Add(new GroupSymbols(int.Parse(row["id"].ToString()), row["code"].ToString(),
                                row["code2"].ToString()));
                            break;

                        case "base_color":
                            BaseColours.Add(new GroupBaseColours(int.Parse(row["id"].ToString()), row["code"].ToString()));
                            break;

                        case "symbol_color":
                            SymbolColours.Add(int.Parse(row["id"].ToString()),
                                new GroupSymbolColours(int.Parse(row["id"].ToString()), row["code"].ToString()));
                            break;

                        case "other_color":
                            BackGroundColours.Add(int.Parse(row["id"].ToString()),
                                new GroupBackGroundColours(int.Parse(row["id"].ToString()), row["code"].ToString()));
                            break;
                    }
            }
        }

        /// <summary>
        /// Clears the information.
        /// </summary>
        internal void ClearInfo()
        {
            Bases.Clear();
            Symbols.Clear();
            BaseColours.Clear();
            SymbolColours.Clear();
            BackGroundColours.Clear();
        }

        /// <summary>
        /// Creates the group.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="desc">The desc.</param>
        /// <param name="roomId">The room identifier.</param>
        /// <param name="badge">The badge.</param>
        /// <param name="session">The session.</param>
        /// <param name="colour1">The colour1.</param>
        /// <param name="colour2">The colour2.</param>
        /// <param name="group">The group.</param>
        internal void CreateGroup(string name, string desc, uint roomId, string badge, GameClient session, int colour1,
            int colour2, out Guild group)
        {
            using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery(string.Format("INSERT INTO groups_data (`name`, `desc`,`badge`,`owner_id`,`created`,`room_id`,`colour1`,`colour2`) VALUES(@name,@desc,@badge,'{0}',UNIX_TIMESTAMP(),'{1}','{2}','{3}')", session.GetHabbo().Id, roomId, colour1, colour2));
                queryReactor.AddParameter("name", name);
                queryReactor.AddParameter("desc", desc);
                queryReactor.AddParameter("badge", badge);

                var id = (uint)queryReactor.InsertQuery();
                queryReactor.RunFastQuery(string.Format("UPDATE rooms_data SET group_id='{0}' WHERE id='{1}' LIMIT 1", id, roomId));
                var dictionary = new Dictionary<uint, GroupUser> { { session.GetHabbo().Id, new GroupUser(session.GetHabbo().Id, id, 2, Azure.GetUnixTimeStamp()) } };
                group = new Guild(id, name, desc, roomId, badge, Azure.GetUnixTimeStamp(), session.GetHabbo().Id, colour1, colour2, dictionary, new List<uint>(), new Dictionary<uint, GroupUser>(), 0u, 1u, false, name, desc, 0, 0.0, 0, string.Empty, 0, 0, 1, 1, 2);
                Groups.Add(id, group);
                queryReactor.RunFastQuery(string.Format("INSERT INTO groups_members (group_id, user_id, rank, date_join) VALUES ('{0}','{1}','2','{2}')", id, session.GetHabbo().Id, Azure.GetUnixTimeStamp()));
                var room = Azure.GetGame().GetRoomManager().GetRoom(roomId);
                if (room != null)
                {
                    room.RoomData.Group = group;
                    room.RoomData.GroupId = id;
                }
                var user = new GroupUser(session.GetHabbo().Id, id, 2, Azure.GetUnixTimeStamp());
                session.GetHabbo().UserGroups.Add(user);
                group.Admins.Add(session.GetHabbo().Id, user);
                queryReactor.RunFastQuery(string.Format("UPDATE users_stats SET favourite_group='{0}' WHERE id='{1}' LIMIT 1", id, session.GetHabbo().Id));
                queryReactor.RunFastQuery(string.Format("DELETE FROM rooms_rights WHERE room_id='{0}'", roomId));
            }
        }

        /// <summary>
        /// Gets the group.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <returns>Guild.</returns>
        internal Guild GetGroup(uint groupId)
        {
            if (Groups == null)
                return null;
            if (Groups.Contains(groupId))
                return (Guild)Groups[groupId];
            using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery(string.Format("SELECT * FROM groups_data WHERE id='{0}' LIMIT 1", groupId));
                var row = queryReactor.GetRow();
                if (row == null)
                    return null;
                queryReactor.SetQuery(string.Format("SELECT user_id, rank, date_join FROM groups_members WHERE group_id='{0}'",
                    groupId));
                var table = queryReactor.GetTable();
                queryReactor.SetQuery(string.Format("SELECT user_id FROM groups_requests WHERE group_id='{0}'", groupId));
                var table2 = queryReactor.GetTable();
                var dictionary = new Dictionary<uint, GroupUser>();
                var dictionary2 = new Dictionary<uint, GroupUser>();
                foreach (DataRow dataRow in table.Rows)
                {
                    dictionary.Add((uint)dataRow[0],
                        new GroupUser((uint)dataRow[0], groupId, int.Parse(dataRow[1].ToString()), (int)dataRow[2]));
                    // all staffs are group admin... 
                    if ((int.Parse(dataRow[1].ToString()) >= 1) || (Azure.GetHabboById(uint.Parse(dataRow[0].ToString())).Rank >= 5))
                        dictionary2.Add((uint)dataRow[0],
                            new GroupUser((uint)dataRow[0], groupId, int.Parse(dataRow[1].ToString()), (int)dataRow[2]));
                }
                var list = (from DataRow dataRow2 in table2.Rows select (uint)dataRow2[0]).ToList();
                var group = new Guild((uint)row[0], row[1].ToString(), row[2].ToString(), (uint)row[6],
                    row[3].ToString(), (int)row[5], (uint)row[4], (int)row[8], (int)row[9], dictionary, list,
                    dictionary2, Convert.ToUInt16(row[7]), Convert.ToUInt16(row[10]), row["has_forum"].ToString() == "1",
                    row["forum_name"].ToString(), row["forum_description"].ToString(),
                    uint.Parse(row["forum_messages_count"].ToString()), double.Parse(row["forum_score"].ToString()),
                    uint.Parse(row["forum_lastposter_id"].ToString()), row["forum_lastposter_name"].ToString(),
                    int.Parse(row["forum_lastposter_timestamp"].ToString()),
                    (int)row["who_can_read"], (int)row["who_can_post"], (int)row["who_can_thread"], (int)row["who_can_mod"]);
                Groups.Add((uint)row[0], group);
                return group;
            }
        }

        /// <summary>
        /// Gets the user groups.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>HashSet&lt;GroupUser&gt;.</returns>
        internal HashSet<GroupUser> GetUserGroups(uint userId)
        {
            var list = new HashSet<GroupUser>();
            using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery(string.Format("SELECT group_id, rank, date_join FROM groups_members WHERE user_id={0}", userId));
                var table = queryReactor.GetTable();
                foreach (DataRow dataRow in table.Rows)
                    list.Add(new GroupUser(userId, (uint)dataRow[0], Convert.ToInt16(dataRow[1]), (int)dataRow[2]));
            }
            return list;
        }

        /// <summary>
        /// Serializes the group members.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <param name="group">The group.</param>
        /// <param name="reqType">Type of the req.</param>
        /// <param name="session">The session.</param>
        /// <param name="searchVal">The search value.</param>
        /// <param name="page">The page.</param>
        /// <returns>ServerMessage.</returns>
        internal ServerMessage SerializeGroupMembers(ServerMessage response, Guild group, uint reqType,
            GameClient session, string searchVal = "", int page = 0)
        {
            if (group == null || session == null)
                return null;
            if (page < 1)
                page = 0;
            var list = Split(GetGroupUsersByString(group, searchVal, reqType));
            response.AppendInteger(group.Id);
            response.AppendString(group.Name);
            response.AppendInteger(group.RoomId);
            response.AppendString(group.Badge);
            switch (reqType)
            {
                case 0u:
                    response.AppendInteger(group.Members.Count);
                    response.AppendInteger(list[page].Count);
                    using (var enumerator = list[page].GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            var current = enumerator.Current;
                            var habboForId = Azure.GetHabboById(current.Id);
                            if (habboForId == null)
                            {
                                response.AppendInteger(0);
                                response.AppendInteger(0);
                                response.AppendString(string.Empty);
                                response.AppendString(string.Empty);
                                response.AppendString(string.Empty);
                            }
                            else
                            {
                                response.AppendInteger((current.Rank == 2) ? 0 : ((current.Rank == 1) ? 1 : 2));
                                response.AppendInteger(habboForId.Id);
                                response.AppendString(habboForId.UserName);
                                response.AppendString(habboForId.Look);
                                response.AppendString(Azure.GetGroupDateJoinString(current.DateJoin));
                            }
                        }
                        goto AppendRest;
                    }
                case 1u:
                    break;

                case 2u:
                    {
                        var list2 = Split(GetGroupRequestsByString(group, searchVal, reqType));
                        response.AppendInteger(group.Requests.Count);
                        if (group.Requests.Count > 0)
                        {
                            response.AppendInteger(list2[page].Count);
                            using (var enumerator2 = list2[page].GetEnumerator())
                            {
                                while (enumerator2.MoveNext())
                                {
                                    var current2 = enumerator2.Current;
                                    var habboForId2 = Azure.GetHabboById(current2);
                                    if (habboForId2 == null)
                                    {
                                        response.AppendInteger(0);
                                        response.AppendInteger(0);
                                        response.AppendString(string.Empty);
                                        response.AppendString(string.Empty);
                                        response.AppendString(string.Empty);
                                    }
                                    else
                                    {
                                        response.AppendInteger(3);
                                        response.AppendInteger(habboForId2.Id);
                                        response.AppendString(habboForId2.UserName);
                                        response.AppendString(habboForId2.Look);
                                        response.AppendString("");
                                    }
                                }
                                goto AppendRest;
                            }
                        }
                        response.AppendInteger(0);
                        goto AppendRest;
                    }
                default:
                    goto AppendRest;
            }
            response.AppendInteger(group.Admins.Count);
            if (group.Admins.Count > 0)
            {
                response.AppendInteger(list[page].Count);
                using (var enumerator3 = list[page].GetEnumerator())
                {
                    while (enumerator3.MoveNext())
                    {
                        var current3 = enumerator3.Current;
                        var habboForId3 = Azure.GetHabboById(current3.Id);
                        if (habboForId3 == null)
                        {
                            response.AppendInteger(0);
                            response.AppendInteger(0);
                            response.AppendString(string.Empty);
                            response.AppendString(string.Empty);
                            response.AppendString(string.Empty);
                        }
                        else
                        {
                            response.AppendInteger((current3.Rank == 2) ? 0 : ((current3.Rank == 1) ? 1 : 2));
                            response.AppendInteger(habboForId3.Id);
                            response.AppendString(habboForId3.UserName);
                            response.AppendString(habboForId3.Look);
                            response.AppendString(Azure.GetGroupDateJoinString(current3.DateJoin));
                        }
                    }
                    goto AppendRest;
                }
            }
            response.AppendInteger(0);
        AppendRest:
            response.AppendBool(session.GetHabbo().Id == group.CreatorId);
            response.AppendInteger(14);
            response.AppendInteger(page);
            response.AppendInteger(reqType);
            response.AppendString(searchVal);
            return response;
        }

        /// <summary>
        /// Gets the group users by string.
        /// </summary>
        /// <param name="Group">The group.</param>
        /// <param name="searchVal">The search value.</param>
        /// <param name="req">The req.</param>
        /// <returns>List&lt;GroupUser&gt;.</returns>
        internal List<GroupUser> GetGroupUsersByString(Guild Group, string searchVal, uint req)
        {
            var list = new List<GroupUser>();
            if (string.IsNullOrWhiteSpace(searchVal))
            {
                if (req == 0u)
                    using (var enumerator = Group.Members.Values.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            var current = enumerator.Current;
                            list.Add(current);
                        }
                        return list;
                    }
                using (var enumerator2 = Group.Admins.Values.GetEnumerator())
                {
                    while (enumerator2.MoveNext())
                    {
                        var current2 = enumerator2.Current;
                        list.Add(current2);
                    }
                    return list;
                }
            }
            using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery("SELECT id FROM users WHERE username LIKE @name");
                queryReactor.AddParameter("name", "%" + searchVal + "%");
                var table = queryReactor.GetTable();
                if (table == null)
                {
                    if (req == 0u)
                        using (var enumerator3 = Group.Members.Values.GetEnumerator())
                        {
                            while (enumerator3.MoveNext())
                            {
                                var current3 = enumerator3.Current;
                                list.Add(current3);
                            }
                            return list;
                        }
                    using (var enumerator4 = Group.Admins.Values.GetEnumerator())
                    {
                        while (enumerator4.MoveNext())
                        {
                            var current4 = enumerator4.Current;
                            list.Add(current4);
                        }
                        return list;
                    }
                }
                list.AddRange(from DataRow dataRow in table.Rows where Group.Members.ContainsKey((uint)dataRow[0]) select Group.Members[(uint)dataRow[0]]);
            }
            return list;
        }

        /// <summary>
        /// Gets the group requests by string.
        /// </summary>
        /// <param name="Group">The group.</param>
        /// <param name="searchVal">The search value.</param>
        /// <param name="req">The req.</param>
        /// <returns>List&lt;System.UInt32&gt;.</returns>
        internal List<uint> GetGroupRequestsByString(Guild Group, string searchVal, uint req)
        {
            if (Group == null)
                return null;
            if (string.IsNullOrWhiteSpace(searchVal))
                return Group.Requests;
            var list = new List<uint>();
            using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery("SELECT id FROM users WHERE username LIKE @name");
                queryReactor.AddParameter("name", "%" + searchVal + "%");
                var table = queryReactor.GetTable();
                if (table == null)
                    return list;
                list.AddRange(from DataRow dataRow in table.Rows where Group.Requests.Contains((uint)dataRow[0]) select (uint)dataRow[0]);
            }
            return list;
        }

        /// <summary>
        /// Serializes the group information.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="response">The response.</param>
        /// <param name="session">The session.</param>
        /// <param name="newWindow">if set to <c>true</c> [new window].</param>
        internal void SerializeGroupInfo(Guild group, ServerMessage response, GameClient session, bool newWindow = false)
        {
            if (group == null || session == null)
                return;
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            var dateTime2 = dateTime.AddSeconds(group.CreateTime);
            response.Init(LibraryParser.OutgoingRequest("GroupDataMessageComposer"));
            response.AppendInteger(group.Id);
            response.AppendBool(true);
            response.AppendInteger(group.State);
            response.AppendString(group.Name);
            response.AppendString(group.Description);
            response.AppendString(group.Badge);
            response.AppendInteger(group.RoomId);
            response.AppendString((Azure.GetGame().GetRoomManager().GenerateRoomData(group.RoomId) == null) ? "No room found.." : Azure.GetGame().GetRoomManager().GenerateRoomData(@group.RoomId).Name);
            response.AppendInteger((group.CreatorId == session.GetHabbo().Id) ? 3 : (group.Requests.Contains(session.GetHabbo().Id) ? 2 : (group.Members.ContainsKey(session.GetHabbo().Id) ? 1 : 0)));
            response.AppendInteger(group.Members.Count);
            response.AppendBool(session.GetHabbo().FavouriteGroup == group.Id);
            response.AppendString(string.Format("{0}-{1}-{2}", dateTime2.Day.ToString("00"), dateTime2.Month.ToString("00"), dateTime2.Year));
            response.AppendBool(group.CreatorId == session.GetHabbo().Id);
            response.AppendBool(group.Admins.ContainsKey(session.GetHabbo().Id));
            response.AppendString((Azure.GetHabboById(group.CreatorId) == null) ? string.Empty : Azure.GetHabboById(group.CreatorId).UserName);
            response.AppendBool(newWindow);
            response.AppendBool(group.AdminOnlyDeco == 0u);
            response.AppendInteger(group.Requests.Count);
            response.AppendBool(group.HasForum);
            session.SendMessage(response);
        }

        /// <summary>
        /// Serializes the group information.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="response">The response.</param>
        /// <param name="session">The session.</param>
        /// <param name="room">The room.</param>
        /// <param name="newWindow">if set to <c>true</c> [new window].</param>
        internal void SerializeGroupInfo(Guild group, ServerMessage response, GameClient session, Room room,
            bool newWindow = false)
        {
            if (room == null || group == null)
                return;
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            var dateTime2 = dateTime.AddSeconds(group.CreateTime);
            response.Init(LibraryParser.OutgoingRequest("GroupDataMessageComposer"));
            response.AppendInteger(group.Id);
            response.AppendBool(true);
            response.AppendInteger(group.State);
            response.AppendString(group.Name);
            response.AppendString(group.Description);
            response.AppendString(group.Badge);
            response.AppendInteger(group.RoomId);
            response.AppendString((Azure.GetGame().GetRoomManager().GenerateRoomData(group.RoomId) == null) ? "No room found.." : Azure.GetGame().GetRoomManager().GenerateRoomData(group.RoomId).Name);
            response.AppendInteger((group.CreatorId == session.GetHabbo().Id) ? 3 : (group.Requests.Contains(session.GetHabbo().Id) ? 2 : (group.Members.ContainsKey(session.GetHabbo().Id) ? 1 : 0)));
            response.AppendInteger(group.Members.Count);
            response.AppendBool(session.GetHabbo().FavouriteGroup == group.Id);
            response.AppendString(string.Format("{0}-{1}-{2}", dateTime2.Day.ToString("00"), dateTime2.Month.ToString("00"), dateTime2.Year));
            response.AppendBool(group.CreatorId == session.GetHabbo().Id);
            response.AppendBool(group.Admins.ContainsKey(session.GetHabbo().Id));
            response.AppendString((Azure.GetHabboById(group.CreatorId) == null) ? string.Empty : Azure.GetHabboById(group.CreatorId).UserName);
            response.AppendBool(newWindow);
            response.AppendBool(group.AdminOnlyDeco == 0u);
            response.AppendInteger(group.Requests.Count);
            response.AppendBool(group.HasForum);
            room.SendMessage(response);
        }

        /// <summary>
        /// Generates the guild image.
        /// </summary>
        /// <param name="guildBase">The guild base.</param>
        /// <param name="guildBaseColor">Color of the guild base.</param>
        /// <param name="states">The states.</param>
        /// <returns>System.String.</returns>
        internal string GenerateGuildImage(int guildBase, int guildBaseColor, List<int> states)
        {
            var image = new StringBuilder(String.Format("b{0:00}{1:00}", guildBase, guildBaseColor));
            for (var i = 0; i < 3 * 4; i += 3)
                image.Append(i >= states.Count ? "s" : String.Format("s{0:00}{1:00}{2}", states[i], states[i + 1], states[i + 2]));
            return image.ToString();
        }

        /// <summary>
        /// Gets the group colour.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="colour1">if set to <c>true</c> [colour1].</param>
        /// <returns>System.String.</returns>
        internal string GetGroupColour(int index, bool colour1)
        {
            if (colour1)
            {
                if (SymbolColours.Contains(index))
                {
                    return ((GroupSymbolColours)SymbolColours[index]).Colour;
                }
                if (BackGroundColours.Contains(index))
                {
                    return ((GroupBackGroundColours)BackGroundColours[index]).Colour;
                }
                return "4f8a00";
            }
            if (BackGroundColours.Contains(index))
            {
                return ((GroupBackGroundColours)BackGroundColours[index]).Colour;
            }
            return "4f8a00";
        }


        /// <summary>
        /// Deletes the group.
        /// </summary>
        /// <param name="id">The identifier.</param>
        internal void DeleteGroup(uint id)
        {
            using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery(string.Format("DELETE FROM groups_members WHERE group_id = {0};DELETE FROM groups_requests WHERE group_id = {0};DELETE FROM groups_data WHERE id = {0};UPDATE rooms_data SET group_id = 0 WHERE group_id = {0};", id));
                queryReactor.RunQuery();

                Groups.Remove(id);
            }
        }

        /// <summary>
        /// Gets the message count for thread.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>System.Int32.</returns>
        internal int GetMessageCountForThread(uint id)
        {
            using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery(string.Format("SELECT COUNT(*) FROM groups_forums_posts WHERE parent_id='{0}'", id));
                return int.Parse(queryReactor.GetString());
            }
        }

        /// <summary>
        /// Splits the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>List&lt;List&lt;GroupUser&gt;&gt;.</returns>
        private static List<List<GroupUser>> Split(IEnumerable<GroupUser> source)
        {
            return (from x in source.Select((x, i) => new { Index = i, Value = x })
                    group x by x.Index / 14
                        into x
                        select (from v in x
                                select v.Value).ToList<GroupUser>()).ToList<List<GroupUser>>();
        }

        /// <summary>
        /// Splits the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>List&lt;List&lt;System.UInt32&gt;&gt;.</returns>
        private static List<List<uint>> Split(IEnumerable<uint> source)
        {
            return (
                from x in source.Select((x, i) => new
                {
                    Index = i,
                    Value = x
                })
                group x by x.Index / 14
                    into x
                    select (
                        from v in x
                        select v.Value).ToList<uint>()).ToList<List<uint>>();
        }
    }
}