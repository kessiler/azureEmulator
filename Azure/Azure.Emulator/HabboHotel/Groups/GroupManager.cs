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
using Azure.HabboHotel.Users;

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
                Habbo user = session.GetHabbo();
                GroupMember memberGroup = new GroupMember(user.Id, user.UserName, user.Look, id, 2, Azure.GetUnixTimeStamp());
                Dictionary<uint, GroupMember> dictionary = new Dictionary<uint, GroupMember> { { session.GetHabbo().Id, memberGroup } };
                Dictionary<uint, GroupMember> emptyDictionary = new Dictionary<uint, GroupMember>();
                group = new Guild(id, name, desc, roomId, badge, Azure.GetUnixTimeStamp(), user.Id, colour1, colour2, dictionary, emptyDictionary, emptyDictionary, 0, 1, false, name, desc, 0, 0.0, 0, string.Empty, 0, 0, 1, 1, 2);
                Groups.Add(id, group);
                queryReactor.RunFastQuery(string.Format("INSERT INTO groups_members (group_id, user_id, rank, date_join) VALUES ('{0}','{1}','2','{2}')", id, session.GetHabbo().Id, Azure.GetUnixTimeStamp()));
                var room = Azure.GetGame().GetRoomManager().GetRoom(roomId);
                if (room != null)
                {
                    room.RoomData.Group = group;
                    room.RoomData.GroupId = id;
                }
                user.UserGroups.Add(memberGroup);
                group.Admins.Add(user.Id, memberGroup);
                queryReactor.RunFastQuery(string.Format("UPDATE users_stats SET favourite_group='{0}' WHERE id='{1}' LIMIT 1", id, user.Id));
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
                queryReactor.SetQuery(string.Format("SELECT g.user_id, u.username, u.look, g.rank, g.date_join FROM groups_members g " +
                                                    "INNER JOIN users u ON (g.user_id = u.id) WHERE g.group_id='{0}'",
                                      groupId));
                DataTable groupMembersTable = queryReactor.GetTable();
                queryReactor.SetQuery(string.Format("SELECT g.user_id, u.username, u.look FROM groups_requests g " +
                                                    "INNER JOIN users u ON (g.user_id = u.id) WHERE group_id='{0}'", groupId));
                DataTable groupRequestsTable = queryReactor.GetTable();
                Dictionary<uint, GroupMember> members = new Dictionary<uint, GroupMember>();
                Dictionary<uint, GroupMember> admins = new Dictionary<uint, GroupMember>();
                Dictionary<uint, GroupMember> requests = new Dictionary<uint, GroupMember>();
                uint userId;
                int rank;
                foreach (DataRow dataRow in groupMembersTable.Rows)
                {
                    userId = (uint)dataRow["user_id"];
                    rank = int.Parse(dataRow["rank"].ToString());
                    GroupMember membGroup = new GroupMember(userId, dataRow["username"].ToString(), dataRow["look"].ToString(),
                        groupId, rank, (int)dataRow["date_join"]);
                    members.Add(userId, membGroup);
                    if (rank >= 1)
                        admins.Add(userId, membGroup);
                }
                foreach (DataRow dataRow in groupRequestsTable.Rows)
                {
                    userId = (uint)dataRow["user_id"];
                    GroupMember membGroup = new GroupMember(userId, dataRow["username"].ToString(), dataRow["look"].ToString(),
                        groupId, 0, Azure.GetUnixTimeStamp());
                    if(!requests.ContainsKey(userId))
                        requests.Add(userId, membGroup);
                }
                Guild group = new Guild((uint)row[0], row[1].ToString(), row[2].ToString(), (uint)row[6],
                    row[3].ToString(), (int)row[5], (uint)row[4], (int)row[8], (int)row[9], members, requests,
                    admins, Convert.ToUInt16(row[7]), Convert.ToUInt16(row[10]), row["has_forum"].ToString() == "1",
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
        internal HashSet<GroupMember> GetUserGroups(uint userId)
        {
            var list = new HashSet<GroupMember>();
            using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery(string.Format("SELECT u.username, u.look, g.group_id, g.rank, g.date_join FROM groups_members g INNER JOIN users u ON (g.user_id = u.id) WHERE g.user_id={0}", userId));
                var table = queryReactor.GetTable();
                foreach (DataRow dataRow in table.Rows)
                    list.Add(new GroupMember(userId, dataRow["username"].ToString(), dataRow["look"].ToString(),
                        (uint)dataRow["group_id"], Convert.ToInt16(dataRow["rank"]), (int)dataRow["date_join"]));
            }
            return list;
        }

        internal void addGroupMemberIntoResponse(ServerMessage response, GroupMember member)
        {
            response.AppendInteger(member.Rank == 2 ? 0 : member.Rank == 1 ? 1 : 2);
            response.AppendInteger(member.Id);
            response.AppendString(member.Name);
            response.AppendString(member.Look);
            response.AppendString(Azure.GetGroupDateJoinString(member.DateJoin));
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
            response.AppendInteger(group.Id);
            response.AppendString(group.Name);
            response.AppendInteger(group.RoomId);
            response.AppendString(group.Badge);
            List<List<GroupMember>> list = Split(GetGroupUsersByString(group, searchVal, reqType));
            switch (reqType)
            {
                case 0:
                    response.AppendInteger(list.Count);
                    if (group.Members.Count > 0 && list.Count > 0 && list[page] != null)
                    {
                        response.AppendInteger(list[page].Count);
                        using (var enumerator = list[page].GetEnumerator())
                        {
                            GroupMember current;
                            while (enumerator.MoveNext())
                            {
                                current = enumerator.Current;
                                addGroupMemberIntoResponse(response, current);
                            }
                        }
                    }
                    else
                    {
                        response.AppendInteger(0);
                    }
                    break;
                case 1:
                    response.AppendInteger(group.Admins.Count);
                    if (group.Admins.Count > 0 && list.Count > 0 && list[page] != null)
                    {
                        response.AppendInteger(list[page].Count);
                        using (var enumerator = list[page].GetEnumerator())
                        {
                            GroupMember current;
                            while (enumerator.MoveNext())
                            {
                                current = enumerator.Current;
                                addGroupMemberIntoResponse(response, current);
                            }
                        }
                    }
                    else
                    {
                        response.AppendInteger(0);
                    }
                    break;

                case 2:
                    response.AppendInteger(group.Requests.Count);
                    if (group.Requests.Count > 0 && list.Count > 0 && list[page] != null)
                    {
                        response.AppendInteger(list[page].Count);
                        using (var enumerator = list[page].GetEnumerator())
                        {
                            GroupMember current;
                            while (enumerator.MoveNext())
                            {
                                current = enumerator.Current;
                                response.AppendInteger(3);
                                response.AppendInteger(current.Id);
                                response.AppendString(current.Name);
                                response.AppendString(current.Look);
                                response.AppendString("");
                            }
                        }
                    }
                    else
                    {
                        response.AppendInteger(0);
                    }
                    break;
            }
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
        internal List<GroupMember> GetGroupUsersByString(Guild Group, string searchVal, uint req)
        {
            List<GroupMember> list = new List<GroupMember>();
            switch (req)
            {
                case 0:
                    using (var enumerator = Group.Members.Values.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            var current = enumerator.Current;
                            list.Add(current);
                        }
                    }
                    break;
                case 1:
                    using (var enumerator2 = Group.Admins.Values.GetEnumerator())
                    {
                        while (enumerator2.MoveNext())
                        {
                            var current2 = enumerator2.Current;
                            list.Add(current2);
                        }
                    }
                    break;
                case 2:
                    list = GetGroupRequestsByString(Group, searchVal);
                    break;
            }
            if (!string.IsNullOrWhiteSpace(searchVal))
            {
                list = list.Where(member => member.Name.ToLower().Contains(searchVal.ToLower())).ToList();
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
        internal List<GroupMember> GetGroupRequestsByString(Guild Group, string searchVal)
        {
            List<GroupMember> groupRequests;
            if (string.IsNullOrWhiteSpace(searchVal))
            {
                groupRequests = Group.Requests.Values.ToList();
            }
            else
            {
                groupRequests = Group.Requests.Values.Where(request => request.Name.ToLower().Contains(searchVal.ToLower())).ToList();
            }
            return groupRequests;
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
            response.AppendInteger((group.CreatorId == session.GetHabbo().Id) ? 3 : (group.Requests.ContainsKey(session.GetHabbo().Id) ? 2 : (group.Members.ContainsKey(session.GetHabbo().Id) ? 1 : 0)));
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
            response.AppendInteger((group.CreatorId == session.GetHabbo().Id) ? 3 : (group.Requests.ContainsKey(session.GetHabbo().Id) ? 2 : (group.Members.ContainsKey(session.GetHabbo().Id) ? 1 : 0)));
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
        private static List<List<GroupMember>> Split(IEnumerable<GroupMember> source)
        {
            return (from x in source.Select((x, i) => new { Index = i, Value = x })
                    group x by x.Index / 14
                        into x
                        select (from v in x
                                select v.Value).ToList<GroupMember>()).ToList<List<GroupMember>>();
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