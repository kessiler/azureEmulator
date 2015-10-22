using System.Collections.Generic;
using System.Data;
using System.Linq;
using Azure.Database.Manager.Database.Session_Details.Interfaces;
using Azure.HabboHotel.Catalogs.Composers;
using Azure.HabboHotel.Groups.Interfaces;
using Azure.HabboHotel.Rooms;
using Azure.HabboHotel.Rooms.Data;
using Azure.HabboHotel.Rooms.User;
using Azure.HabboHotel.Users;
using Azure.Messages.Parsers;

namespace Azure.Messages.Handlers
{
    /// <summary>
    /// Class GameClientMessageHandler.
    /// </summary>
    internal partial class GameClientMessageHandler
    {
        internal readonly ushort TotalPerPage = 20;

        /// <summary>
        /// Serializes the group purchase page.
        /// </summary>
        internal void SerializeGroupPurchasePage()
        {
            var list = new HashSet<RoomData>(Session.GetHabbo().UsersRooms.Where(x => x.Group == null));

            Response.Init(LibraryParser.OutgoingRequest("GroupPurchasePageMessageComposer"));
            Response.AppendInteger(10);
            Response.AppendInteger(list.Count);

            foreach (RoomData current2 in list)
                NewMethod(current2);

            Response.AppendInteger(5);
            Response.AppendInteger(10);
            Response.AppendInteger(3);
            Response.AppendInteger(4);
            Response.AppendInteger(25);
            Response.AppendInteger(17);
            Response.AppendInteger(5);
            Response.AppendInteger(25);
            Response.AppendInteger(17);
            Response.AppendInteger(3);
            Response.AppendInteger(29);
            Response.AppendInteger(11);
            Response.AppendInteger(4);
            Response.AppendInteger(0);
            Response.AppendInteger(0);
            Response.AppendInteger(0);
            SendResponse();
        }

        /// <summary>
        /// Serializes the group purchase parts.
        /// </summary>
        internal void SerializeGroupPurchaseParts()
        {
            Response.Init(LibraryParser.OutgoingRequest("GroupPurchasePartsMessageComposer"));
            Response.AppendInteger(Azure.GetGame().GetGroupManager().Bases.Count);
            foreach (GroupBases current in Azure.GetGame().GetGroupManager().Bases)
            {
                Response.AppendInteger(current.Id);
                Response.AppendString(current.Value1);
                Response.AppendString(current.Value2);
            }
            Response.AppendInteger(Azure.GetGame().GetGroupManager().Symbols.Count);
            foreach (GroupSymbols current2 in Azure.GetGame().GetGroupManager().Symbols)
            {
                Response.AppendInteger(current2.Id);
                Response.AppendString(current2.Value1);
                Response.AppendString(current2.Value2);
            }
            Response.AppendInteger(Azure.GetGame().GetGroupManager().BaseColours.Count);
            foreach (GroupBaseColours current3 in Azure.GetGame().GetGroupManager().BaseColours)
            {
                Response.AppendInteger(current3.Id);
                Response.AppendString(current3.Colour);
            }
            Response.AppendInteger(Azure.GetGame().GetGroupManager().SymbolColours.Count);
            foreach (GroupSymbolColours current4 in Azure.GetGame().GetGroupManager().SymbolColours.Values)
            {
                Response.AppendInteger(current4.Id);
                Response.AppendString(current4.Colour);
            }
            Response.AppendInteger(Azure.GetGame().GetGroupManager().BackGroundColours.Count);
            foreach (GroupBackGroundColours current5 in Azure.GetGame().GetGroupManager().BackGroundColours.Values)
            {
                Response.AppendInteger(current5.Id);
                Response.AppendString(current5.Colour);
            }
            SendResponse();
        }

        /// <summary>
        /// Purchases the group.
        /// </summary>
        internal void PurchaseGroup()
        {
            if (Session == null || Session.GetHabbo().Credits < 10)
                return;

            var gStates = new List<int>();
            var name = Request.GetString();
            var description = Request.GetString();
            var roomid = Request.GetUInteger();
            var color = Request.GetInteger();
            var num3 = Request.GetInteger();
            var unused = Request.GetInteger();
            var guildBase = Request.GetInteger();
            var guildBaseColor = Request.GetInteger();
            var num6 = Request.GetInteger();
            var roomData = Azure.GetGame().GetRoomManager().GenerateRoomData(roomid);

            if (roomData.Owner != Session.GetHabbo().UserName)
                return;

            for (var i = 0; i < (num6 * 3); i++)
            {
                var item = Request.GetInteger();
                gStates.Add(item);
            }

            var image = Azure.GetGame()
                .GetGroupManager()
                .GenerateGuildImage(guildBase, guildBaseColor, gStates);
            Guild group;
            Azure.GetGame()
                .GetGroupManager()
                .CreateGroup(name, description, roomid, image, Session,
                    (!Azure.GetGame().GetGroupManager().SymbolColours.Contains(color)) ? 1 : color,
                    (!Azure.GetGame().GetGroupManager().BackGroundColours.Contains(num3)) ? 1 : num3,
                    out group);

            Session.SendMessage(CatalogPageComposer.PurchaseOk(0u, "CREATE_GUILD", 10));
            Response.Init(LibraryParser.OutgoingRequest("GroupRoomMessageComposer"));
            Response.AppendInteger(roomid);
            Response.AppendInteger(group.Id);
            SendResponse();
            roomData.Group = group;
            roomData.GroupId = group.Id;
            roomData.SerializeRoomData(Response, Session, true);

            if (!Session.GetHabbo().InRoom || Session.GetHabbo().CurrentRoom.RoomId != roomData.Id)
            {
                Session.GetMessageHandler()
                    .PrepareRoomForUser(roomData.Id,
                        roomData.PassWord);
                Session.GetHabbo().CurrentRoomId = roomData.Id;
            }

            if (Session.GetHabbo().CurrentRoom != null &&
                !Session.GetHabbo().CurrentRoom.LoadedGroups.ContainsKey(group.Id))
                Session.GetHabbo().CurrentRoom.LoadedGroups.Add(group.Id, group.Badge);
            if (CurrentLoadingRoom != null && !CurrentLoadingRoom.LoadedGroups.ContainsKey(group.Id))
                CurrentLoadingRoom.LoadedGroups.Add(group.Id, group.Badge);

            if (CurrentLoadingRoom != null)
            {
                var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("RoomGroupMessageComposer"));
                serverMessage.AppendInteger(CurrentLoadingRoom.LoadedGroups.Count);

                foreach (var current in CurrentLoadingRoom.LoadedGroups)
                {
                    serverMessage.AppendInteger(current.Key);
                    serverMessage.AppendString(current.Value);
                }
                CurrentLoadingRoom.SendMessage(serverMessage);
            }

            if (CurrentLoadingRoom == null || Session.GetHabbo().FavouriteGroup != @group.Id)
                return;
            var serverMessage2 = new ServerMessage(LibraryParser.OutgoingRequest("ChangeFavouriteGroupMessageComposer"));
            serverMessage2.AppendInteger(
                CurrentLoadingRoom.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id).VirtualId);
            serverMessage2.AppendInteger(@group.Id);
            serverMessage2.AppendInteger(3);
            serverMessage2.AppendString(@group.Name);
            CurrentLoadingRoom.SendMessage(serverMessage2);
        }

        /// <summary>
        /// Serializes the group information.
        /// </summary>
        internal void SerializeGroupInfo()
        {
            uint groupId = Request.GetUInteger();
            bool newWindow = Request.GetBool();
            Guild group = Azure.GetGame().GetGroupManager().GetGroup(groupId);

            if (group == null)
                return;

            Azure.GetGame().GetGroupManager().SerializeGroupInfo(group, Response, Session, newWindow);
        }

        /// <summary>
        /// Serializes the group members.
        /// </summary>
        internal void SerializeGroupMembers()
        {
            uint groupId = Request.GetUInteger();
            int page = Request.GetInteger();
            string searchVal = Request.GetString();
            uint reqType = Request.GetUInteger();
            Guild group = Azure.GetGame().GetGroupManager().GetGroup(groupId);
            Response.Init(LibraryParser.OutgoingRequest("GroupMembersMessageComposer"));
            Azure.GetGame().GetGroupManager().SerializeGroupMembers(Response, group, reqType, Session, searchVal, page);
            SendResponse();
        }

        /// <summary>
        /// Makes the group admin.
        /// </summary>
        internal void MakeGroupAdmin()
        {
            uint num = Request.GetUInteger();
            uint num2 = Request.GetUInteger();
            Guild group = Azure.GetGame().GetGroupManager().GetGroup(num);

            if (Session.GetHabbo().Id != group.CreatorId || !group.Members.ContainsKey(num2) || group.Admins.ContainsKey(num2))
                return;

            group.Members[num2].Rank = 1;
            group.Admins.Add(num2, group.Members[num2]);
            Response.Init(LibraryParser.OutgoingRequest("GroupMembersMessageComposer"));
            Azure.GetGame().GetGroupManager().SerializeGroupMembers(Response, group, 1u, Session);
            SendResponse();
            Room room = Azure.GetGame().GetRoomManager().GetRoom(group.RoomId);

            if (room != null)
            {
                RoomUser roomUserByHabbo = room.GetRoomUserManager().GetRoomUserByHabbo(Azure.GetHabboById(num2).UserName);
                if (roomUserByHabbo != null)
                {
                    if (!roomUserByHabbo.Statusses.ContainsKey("flatctrl 1"))
                        roomUserByHabbo.AddStatus("flatctrl 1", "");
                    Response.Init(LibraryParser.OutgoingRequest("RoomRightsLevelMessageComposer"));
                    Response.AppendInteger(1);
                    roomUserByHabbo.GetClient().SendMessage(GetResponse());
                    roomUserByHabbo.UpdateNeeded = true;
                }
            }
            using (IQueryAdapter queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.RunFastQuery(string.Concat("UPDATE groups_members SET rank='1' WHERE group_id=", num, " AND user_id=", num2, " LIMIT 1;"));
            }
        }

        /// <summary>
        /// Removes the group admin.
        /// </summary>
        internal void RemoveGroupAdmin()
        {
            uint num = Request.GetUInteger();
            uint num2 = Request.GetUInteger();
            Guild group = Azure.GetGame().GetGroupManager().GetGroup(num);
            if (Session.GetHabbo().Id != group.CreatorId || !group.Members.ContainsKey(num2) || !group.Admins.ContainsKey(num2))
                return;
            group.Members[num2].Rank = 0;
            group.Admins.Remove(num2);
            Response.Init(LibraryParser.OutgoingRequest("GroupMembersMessageComposer"));
            Azure.GetGame().GetGroupManager().SerializeGroupMembers(Response, group, 0u, Session);
            SendResponse();
            Room room = Azure.GetGame().GetRoomManager().GetRoom(group.RoomId);
            if (room != null)
            {
                RoomUser roomUserByHabbo = room.GetRoomUserManager().GetRoomUserByHabbo(Azure.GetHabboById(num2).UserName);
                if (roomUserByHabbo != null)
                {
                    if (roomUserByHabbo.Statusses.ContainsKey("flatctrl 1"))
                        roomUserByHabbo.RemoveStatus("flatctrl 1");
                    Response.Init(LibraryParser.OutgoingRequest("RoomRightsLevelMessageComposer"));
                    Response.AppendInteger(0);
                    roomUserByHabbo.GetClient().SendMessage(GetResponse());
                    roomUserByHabbo.UpdateNeeded = true;
                }
            }
            using (IQueryAdapter queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.RunFastQuery(string.Concat("UPDATE groups_members SET rank='0' WHERE group_id=", num, " AND user_id=", num2, " LIMIT 1;"));
            }
        }

        /// <summary>
        /// Accepts the membership.
        /// </summary>
        internal void AcceptMembership()
        {
            uint groupId = Request.GetUInteger();
            uint userId = Request.GetUInteger();
            Guild group = Azure.GetGame().GetGroupManager().GetGroup(groupId);
            if (Session.GetHabbo().Id != group.CreatorId && !group.Admins.ContainsKey(Session.GetHabbo().Id) && !group.Requests.ContainsKey(userId))
                return;
            if (group.Members.ContainsKey(userId))
            {
                group.Requests.Remove(userId);
                using (IQueryAdapter queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
                {
                    queryReactor.RunFastQuery(string.Format("DELETE FROM groups_requests WHERE group_id = '{0}' AND user_id = '{1}' LIMIT 1", groupId, userId));
                }
                return;
            }

            GroupMember memberGroup = group.Requests[userId];
            memberGroup.DateJoin = Azure.GetUnixTimeStamp();
            group.Members.Add(userId, memberGroup);
            group.Requests.Remove(userId);
            group.Admins.Add(userId, group.Members[userId]);

            Azure.GetGame().GetGroupManager().SerializeGroupInfo(group, Response, Session);
            Response.Init(LibraryParser.OutgoingRequest("GroupMembersMessageComposer"));
            Azure.GetGame().GetGroupManager().SerializeGroupMembers(Response, group, 0u, Session);
            SendResponse();

            using (IQueryAdapter queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.RunFastQuery(string.Format("DELETE FROM groups_requests WHERE group_id = '{0}' AND user_id = '{1}' LIMIT 1", groupId, userId));
            }
            using (IQueryAdapter queryreactor2 = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryreactor2.RunFastQuery(string.Format("INSERT INTO groups_members (group_id, user_id, rank, date_join) VALUES ('{0}','{1}','0','{2}')", groupId, userId, Azure.GetUnixTimeStamp()));
            }
        }

        /// <summary>
        /// Declines the membership.
        /// </summary>
        internal void DeclineMembership()
        {
            var groupId = Request.GetUInteger();
            var userId = Request.GetUInteger();
            var group = Azure.GetGame().GetGroupManager().GetGroup(groupId);

            if (Session.GetHabbo().Id != group.CreatorId && !group.Admins.ContainsKey(Session.GetHabbo().Id) &&
                !group.Requests.ContainsKey(userId))
                return;

            group.Requests.Remove(userId);

            Response.Init(LibraryParser.OutgoingRequest("GroupMembersMessageComposer"));
            Azure.GetGame().GetGroupManager().SerializeGroupMembers(Response, group, 2u, Session);
            SendResponse();
            var room = Azure.GetGame().GetRoomManager().GetRoom(group.RoomId);
            if (room != null)
            {
                var roomUserByHabbo = room.GetRoomUserManager().GetRoomUserByHabbo(Azure.GetHabboById(userId).UserName);
                if (roomUserByHabbo != null)
                {
                    if (roomUserByHabbo.Statusses.ContainsKey("flatctrl 1")) roomUserByHabbo.RemoveStatus("flatctrl 1");
                    roomUserByHabbo.UpdateNeeded = true;
                }
            }
            Azure.GetGame().GetGroupManager().SerializeGroupInfo(group, Response, Session);
            using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.RunFastQuery("DELETE FROM groups_requests WHERE group_id=" + groupId + " AND user_id=" + userId);
            }
        }

        /// <summary>
        /// Joins the group.
        /// </summary>
        internal void JoinGroup()
        {
            uint groupId = Request.GetUInteger();
            Guild group = Azure.GetGame().GetGroupManager().GetGroup(groupId);
            Habbo user = Session.GetHabbo();

            if (!group.Members.ContainsKey(user.Id))
            {
                if (group.State == 0)
                {
                    using (IQueryAdapter queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
                    {
                        queryReactor.RunFastQuery(string.Concat("INSERT INTO groups_members (user_id, group_id, date_join) VALUES (", user.Id, ",", groupId, ",", Azure.GetUnixTimeStamp(), ")"));
                        queryReactor.RunFastQuery(string.Concat("UPDATE users_stats SET favourite_group=", groupId, " WHERE id= ", user.Id, " LIMIT 1"));
                    }
                    group.Members.Add(user.Id, new GroupMember(user.Id, user.UserName, user.Look, group.Id, 0, Azure.GetUnixTimeStamp()));
                    Session.GetHabbo().UserGroups.Add(group.Members[user.Id]);
                }
                else
                {
                    if (!group.Requests.ContainsKey(user.Id))
                    {
                        using (IQueryAdapter queryreactor2 = Azure.GetDatabaseManager().GetQueryReactor())
                        {
                            queryreactor2.RunFastQuery(string.Concat("INSERT INTO groups_requests (user_id, group_id) VALUES (", Session.GetHabbo().Id, ",", groupId, ")"));
                        }
                        GroupMember groupRequest = new GroupMember(user.Id, user.UserName, user.Look, group.Id, 0, Azure.GetUnixTimeStamp());
                        group.Requests.Add(user.Id, groupRequest);
                    }
                }

                Azure.GetGame().GetGroupManager().SerializeGroupInfo(group, Response, Session);
            }
        }

        /// <summary>
        /// Removes the member.
        /// </summary>
        internal void RemoveMember()
        {
            uint num = Request.GetUInteger();
            uint num2 = Request.GetUInteger();
            Guild group = Azure.GetGame().GetGroupManager().GetGroup(num);
            if (num2 == Session.GetHabbo().Id)
            {
                if (group.Members.ContainsKey(num2))
                    group.Members.Remove(num2);
                if (group.Admins.ContainsKey(num2))
                    group.Admins.Remove(num2);
                using (IQueryAdapter queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
                {
                    queryReactor.RunFastQuery(string.Concat("DELETE FROM groups_members WHERE user_id=", num2, " AND group_id=", num, " LIMIT 1"));
                }
                Azure.GetGame().GetGroupManager().SerializeGroupInfo(group, Response, Session);
                if (Session.GetHabbo().FavouriteGroup == num)
                {
                    Session.GetHabbo().FavouriteGroup = 0u;
                    using (IQueryAdapter queryreactor2 = Azure.GetDatabaseManager().GetQueryReactor())
                        queryreactor2.RunFastQuery(string.Format("UPDATE users_stats SET favourite_group=0 WHERE id={0} LIMIT 1", num2));
                    Response.Init(LibraryParser.OutgoingRequest("FavouriteGroupMessageComposer"));
                    Response.AppendInteger(Session.GetHabbo().Id);
                    Session.GetHabbo().CurrentRoom.SendMessage(Response);
                    Response.Init(LibraryParser.OutgoingRequest("ChangeFavouriteGroupMessageComposer"));
                    Response.AppendInteger(0);
                    Response.AppendInteger(-1);
                    Response.AppendInteger(-1);
                    Response.AppendString("");
                    Session.GetHabbo().CurrentRoom.SendMessage(Response);
                    if (group.AdminOnlyDeco == 0u)
                    {
                        RoomUser roomUserByHabbo = Azure.GetGame().GetRoomManager().GetRoom(group.RoomId).GetRoomUserManager().GetRoomUserByHabbo(Azure.GetHabboById(num2).UserName);
                        if (roomUserByHabbo == null)
                            return;
                        roomUserByHabbo.RemoveStatus("flatctrl 1");
                        Response.Init(LibraryParser.OutgoingRequest("RoomRightsLevelMessageComposer"));
                        Response.AppendInteger(0);
                        roomUserByHabbo.GetClient().SendMessage(GetResponse());
                    }
                }
                return;
            }
            if (Session.GetHabbo().Id != group.CreatorId || !group.Members.ContainsKey(num2))
                return;
            group.Members.Remove(num2);
            if (group.Admins.ContainsKey(num2))
                group.Admins.Remove(num2);
            Azure.GetGame().GetGroupManager().SerializeGroupInfo(group, Response, Session);
            Response.Init(LibraryParser.OutgoingRequest("GroupMembersMessageComposer"));
            Azure.GetGame().GetGroupManager().SerializeGroupMembers(Response, group, 0u, Session);
            SendResponse();
            using (IQueryAdapter queryreactor3 = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryreactor3.RunFastQuery(string.Concat("DELETE FROM groups_members WHERE group_id=", num, " AND user_id=", num2, " LIMIT 1;"));
            }
        }

        /// <summary>
        /// Makes the fav.
        /// </summary>
        internal void MakeFav()
        {
            uint groupId = Request.GetUInteger();
            Guild group = Azure.GetGame().GetGroupManager().GetGroup(groupId);
            if (group == null)
                return;
            if (!group.Members.ContainsKey(Session.GetHabbo().Id))
                return;
            Session.GetHabbo().FavouriteGroup = group.Id;
            Azure.GetGame().GetGroupManager().SerializeGroupInfo(group, Response, Session);
            using (IQueryAdapter queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.RunFastQuery(string.Concat("UPDATE users_stats SET favourite_group =", @group.Id, " WHERE id=", Session.GetHabbo().Id, " LIMIT 1;"));
            }
            Response.Init(LibraryParser.OutgoingRequest("FavouriteGroupMessageComposer"));
            Response.AppendInteger(Session.GetHabbo().Id);
            Session.SendMessage(Response);
            if (Session.GetHabbo().CurrentRoom != null)
            {
                if (!Session.GetHabbo().CurrentRoom.LoadedGroups.ContainsKey(group.Id))
                {
                    Session.GetHabbo().CurrentRoom.LoadedGroups.Add(group.Id, group.Badge);
                    Response.Init(LibraryParser.OutgoingRequest("RoomGroupMessageComposer"));
                    Response.AppendInteger(Session.GetHabbo().CurrentRoom.LoadedGroups.Count);
                    foreach (KeyValuePair<uint, string> current in Session.GetHabbo().CurrentRoom.LoadedGroups)
                    {
                        Response.AppendInteger(current.Key);
                        Response.AppendString(current.Value);
                    }
                    Session.GetHabbo().CurrentRoom.SendMessage(Response);
                }
            }
            Response.Init(LibraryParser.OutgoingRequest("ChangeFavouriteGroupMessageComposer"));
            Response.AppendInteger(0);
            Response.AppendInteger(group.Id);
            Response.AppendInteger(3);
            Response.AppendString(group.Name);
            Session.SendMessage(Response);
        }

        /// <summary>
        /// Removes the fav.
        /// </summary>
        internal void RemoveFav()
        {
            Request.GetUInteger();
            Session.GetHabbo().FavouriteGroup = 0u;
            using (IQueryAdapter queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
                queryReactor.RunFastQuery(string.Format("UPDATE users_stats SET favourite_group=0 WHERE id={0} LIMIT 1;", Session.GetHabbo().Id));
            Response.Init(LibraryParser.OutgoingRequest("FavouriteGroupMessageComposer"));
            Response.AppendInteger(Session.GetHabbo().Id);
            Session.SendMessage(Response);
            Response.Init(LibraryParser.OutgoingRequest("ChangeFavouriteGroupMessageComposer"));
            Response.AppendInteger(0);
            Response.AppendInteger(-1);
            Response.AppendInteger(-1);
            Response.AppendString("");
            Session.SendMessage(Response);
        }

        /// <summary>
        /// Publishes the forum thread.
        /// </summary>
        internal void PublishForumThread()
        {
            if ((Azure.GetUnixTimeStamp() - Session.GetHabbo().LastSqlQuery) < 20)
                return;
            uint groupId = Request.GetUInteger();
            uint threadId = Request.GetUInteger();
            string subject = Request.GetString();
            string content = Request.GetString();
            Guild group = Azure.GetGame().GetGroupManager().GetGroup(groupId);
            if (group == null || !group.HasForum)
                return;
            int timestamp = Azure.GetUnixTimeStamp();
            using (IQueryAdapter dbClient = Azure.GetDatabaseManager().GetQueryReactor())
            {
                if (threadId != 0)
                {
                    dbClient.SetQuery(string.Format("SELECT * FROM groups_forums_posts WHERE id = {0}", threadId));
                    DataRow row = dbClient.GetRow();
                    var post = new GroupForumPost(row);
                    if (post.Locked || post.Hidden)
                    {
                        Session.SendNotif(Azure.GetLanguage().GetVar("forums_cancel"));
                        return;
                    }
                }
                Session.GetHabbo().LastSqlQuery = Azure.GetUnixTimeStamp();
                dbClient.SetQuery("INSERT INTO groups_forums_posts (group_id, parent_id, timestamp, poster_id, poster_name, poster_look, subject, post_content) VALUES (@gid, @pard, @ts, @pid, @pnm, @plk, @subjc, @content)");
                dbClient.AddParameter("gid", groupId);
                dbClient.AddParameter("pard", threadId);
                dbClient.AddParameter("ts", timestamp);
                dbClient.AddParameter("pid", Session.GetHabbo().Id);
                dbClient.AddParameter("pnm", Session.GetHabbo().UserName);
                dbClient.AddParameter("plk", Session.GetHabbo().Look);
                dbClient.AddParameter("subjc", subject);
                dbClient.AddParameter("content", content);
                threadId = (uint)dbClient.GetInteger();
            }
            group.ForumScore += 0.25;
            group.ForumLastPosterName = Session.GetHabbo().UserName;
            group.ForumLastPosterId = Session.GetHabbo().Id;
            group.ForumLastPosterTimestamp = timestamp;
            group.ForumMessagesCount++;
            group.UpdateForum();
            if (threadId == 0)
            {
                var message = new ServerMessage(LibraryParser.OutgoingRequest("GroupForumNewThreadMessageComposer"));
                message.AppendInteger(groupId);
                message.AppendInteger(threadId);
                message.AppendInteger(Session.GetHabbo().Id);
                message.AppendString(subject);
                message.AppendString(content);
                message.AppendBool(false);
                message.AppendBool(false);
                message.AppendInteger((Azure.GetUnixTimeStamp() - timestamp));
                message.AppendInteger(1);
                message.AppendInteger(0);
                message.AppendInteger(0);
                message.AppendInteger(1);
                message.AppendString("");
                message.AppendInteger((Azure.GetUnixTimeStamp() - timestamp));
                message.AppendByte(1);
                message.AppendInteger(1);
                message.AppendString("");
                message.AppendInteger(42);//useless
                Session.SendMessage(message);
            }
            else
            {
                var message = new ServerMessage(LibraryParser.OutgoingRequest("GroupForumNewResponseMessageComposer"));
                message.AppendInteger(groupId);
                message.AppendInteger(threadId);
                message.AppendInteger(group.ForumMessagesCount);
                message.AppendInteger(0);
                message.AppendInteger(Session.GetHabbo().Id);
                message.AppendString(Session.GetHabbo().UserName);
                message.AppendString(Session.GetHabbo().Look);
                message.AppendInteger((Azure.GetUnixTimeStamp() - timestamp));
                message.AppendString(content);
                message.AppendByte(0);
                message.AppendInteger(0);
                message.AppendString("");
                message.AppendInteger(0);
                Session.SendMessage(message);
            }
        }

        /// <summary>
        /// Updates the state of the thread.
        /// </summary>
        internal void UpdateThreadState()
        {
            uint groupId = Request.GetUInteger();
            uint threadId = Request.GetUInteger();
            bool pin = Request.GetBool();
            bool Lock = Request.GetBool();
            using (IQueryAdapter dbClient = Azure.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(string.Format("SELECT * FROM groups_forums_posts WHERE group_id = '{0}' AND id = '{1}' LIMIT 1;", groupId, threadId));
                DataRow row = dbClient.GetRow();
                Guild @group = Azure.GetGame().GetGroupManager().GetGroup(groupId);
                if (row != null)
                {
                    if ((uint)row["poster_id"] == Session.GetHabbo().Id || @group.Admins.ContainsKey(Session.GetHabbo().Id))
                    {
                        dbClient.SetQuery(string.Format("UPDATE groups_forums_posts SET pinned = @pin , locked = @lock WHERE id = {0};", threadId));
                        dbClient.AddParameter("pin", (pin) ? "1" : "0");
                        dbClient.AddParameter("lock", (Lock) ? "1" : "0");
                        dbClient.RunQuery();
                    }
                }

                var thread = new GroupForumPost(row);
                if (thread.Pinned != pin)
                {
                    var notif = new ServerMessage(LibraryParser.OutgoingRequest("SuperNotificationMessageComposer"));
                    notif.AppendString((pin) ? "forums.thread.pinned" : "forums.thread.unpinned");
                    notif.AppendInteger(0);
                    Session.SendMessage(notif);
                }
                if (thread.Locked != Lock)
                {
                    var notif2 = new ServerMessage(LibraryParser.OutgoingRequest("SuperNotificationMessageComposer"));
                    notif2.AppendString((Lock) ? "forums.thread.locked" : "forums.thread.unlocked");
                    notif2.AppendInteger(0);
                    Session.SendMessage(notif2);
                }
                if (thread.ParentId != 0)
                    return;
                var message = new ServerMessage(LibraryParser.OutgoingRequest("GroupForumThreadUpdateMessageComposer"));
                message.AppendInteger(groupId);
                message.AppendInteger(thread.Id);
                message.AppendInteger(thread.PosterId);
                message.AppendString(thread.PosterName);
                message.AppendString(thread.Subject);
                message.AppendBool(pin);
                message.AppendBool(Lock);
                message.AppendInteger((Azure.GetUnixTimeStamp() - thread.Timestamp));
                message.AppendInteger(thread.MessageCount + 1);
                message.AppendInteger(0);
                message.AppendInteger(0);
                message.AppendInteger(1);
                message.AppendString("");
                message.AppendInteger((Azure.GetUnixTimeStamp() - thread.Timestamp));
                message.AppendByte((thread.Hidden) ? 10 : 1);
                message.AppendInteger(1);
                message.AppendString(thread.Hider);
                message.AppendInteger(0);
                Session.SendMessage(message);
            }
        }

        /// <summary>
        /// Alters the state of the forum thread.
        /// </summary>
        internal void AlterForumThreadState()
        {
            uint groupId = Request.GetUInteger();
            uint threadId = Request.GetUInteger();
            int stateToSet = Request.GetInteger();
            using (IQueryAdapter dbClient = Azure.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(string.Format("SELECT * FROM groups_forums_posts WHERE group_id = '{0}' AND id = '{1}' LIMIT 1;", groupId, threadId));
                DataRow row = dbClient.GetRow();
                Guild @group = Azure.GetGame().GetGroupManager().GetGroup(groupId);
                if (row != null)
                {
                    if ((uint)row["poster_id"] == Session.GetHabbo().Id || @group.Admins.ContainsKey(Session.GetHabbo().Id))
                    {
                        dbClient.SetQuery(string.Format("UPDATE groups_forums_posts SET hidden = @hid WHERE id = {0};", threadId));
                        dbClient.AddParameter("hid", (stateToSet == 20) ? "1" : "0");
                        dbClient.RunQuery();
                    }
                }
                var thread = new GroupForumPost(row);
                var notif = new ServerMessage(LibraryParser.OutgoingRequest("SuperNotificationMessageComposer"));
                notif.AppendString((stateToSet == 20) ? "forums.thread.hidden" : "forums.thread.restored");
                notif.AppendInteger(0);
                Session.SendMessage(notif);
                if (thread.ParentId != 0)
                    return;
                var message = new ServerMessage(LibraryParser.OutgoingRequest("GroupForumThreadUpdateMessageComposer"));
                message.AppendInteger(groupId);
                message.AppendInteger(thread.Id);
                message.AppendInteger(thread.PosterId);
                message.AppendString(thread.PosterName);
                message.AppendString(thread.Subject);
                message.AppendBool(thread.Pinned);
                message.AppendBool(thread.Locked);
                message.AppendInteger((Azure.GetUnixTimeStamp() - thread.Timestamp));
                message.AppendInteger(thread.MessageCount + 1);
                message.AppendInteger(0);
                message.AppendInteger(0);
                message.AppendInteger(0);
                message.AppendString("");
                message.AppendInteger((Azure.GetUnixTimeStamp() - thread.Timestamp));
                message.AppendByte(stateToSet);
                message.AppendInteger(0);
                message.AppendString(thread.Hider);
                message.AppendInteger(0);
                Session.SendMessage(message);
            }
        }

        /// <summary>
        /// Reads the forum thread.
        /// </summary>
        internal void ReadForumThread()
        {
            uint groupId = Request.GetUInteger();
            uint threadId = Request.GetUInteger();
            int startIndex = Request.GetInteger();
            int endIndex = Request.GetInteger();
            Guild @group = Azure.GetGame().GetGroupManager().GetGroup(groupId);
            if (@group == null || !@group.HasForum)
                return;
            using (IQueryAdapter dbClient = Azure.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(string.Format("SELECT * FROM groups_forums_posts WHERE group_id = '{0}' AND parent_id = '{1}' OR id = '{2}' ORDER BY timestamp ASC;", groupId, threadId, threadId));
                DataTable table = dbClient.GetTable();
                if (table == null)
                    return;
                int b = (table.Rows.Count <= 20) ? table.Rows.Count : 20;
                var posts = new List<GroupForumPost>();
                int i = 1;
                while (i <= b)
                {
                    DataRow row = table.Rows[i - 1];
                    if (row == null)
                    {
                        b--;
                        continue;
                    }
                    var thread = new GroupForumPost(row);
                    if (thread.ParentId == 0 && thread.Hidden)
                        return;
                    posts.Add(thread);
                    i++;
                }

                var message = new ServerMessage(LibraryParser.OutgoingRequest("GroupForumReadThreadMessageComposer"));
                message.AppendInteger(groupId);
                message.AppendInteger(threadId);
                message.AppendInteger(startIndex);
                message.AppendInteger(b);
                int indx = 0;
                foreach (GroupForumPost post in posts)
                {
                    message.AppendInteger(indx++ - 1);
                    message.AppendInteger(indx - 1);
                    message.AppendInteger(post.PosterId);
                    message.AppendString(post.PosterName);
                    message.AppendString(post.PosterLook);
                    message.AppendInteger((Azure.GetUnixTimeStamp() - post.Timestamp));
                    message.AppendString(post.PostContent);
                    message.AppendByte(0);
                    message.AppendInteger(0);
                    message.AppendString(post.Hider);
                    message.AppendInteger(0);
                }
                Session.SendMessage(message);
            }
        }

        /// <summary>
        /// Gets the group forum thread root.
        /// </summary>
        internal void GetGroupForumThreadRoot()
        {
            uint groupId = Request.GetUInteger();
            int startIndex = Request.GetInteger();
            using (IQueryAdapter dbClient = Azure.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(string.Format("SELECT count(id) FROM groups_forums_posts WHERE group_id = '{0}' AND parent_id = 0", groupId));
                int totalThreads = dbClient.GetInteger();
                dbClient.SetQuery(string.Format("SELECT * FROM groups_forums_posts WHERE group_id = '{0}' AND parent_id = 0 ORDER BY timestamp DESC, pinned DESC LIMIT @startIndex, @totalPerPage;", groupId));
                dbClient.AddParameter("startIndex", startIndex);
                dbClient.AddParameter("totalPerPage", TotalPerPage);
                DataTable table = dbClient.GetTable();
                int threadCount = (table.Rows.Count <= TotalPerPage) ? table.Rows.Count : TotalPerPage;
                var threads = new List<GroupForumPost>();
                foreach (DataRow row in table.Rows)
                {
                    var thread = new GroupForumPost(row);
                    threads.Add(thread);
                }
                var message = new ServerMessage(LibraryParser.OutgoingRequest("GroupForumThreadRootMessageComposer"));
                message.AppendInteger(groupId);
                message.AppendInteger(startIndex);
                message.AppendInteger(threadCount);
                foreach (GroupForumPost thread in threads)
                {
                    message.AppendInteger(thread.Id);
                    message.AppendInteger(thread.PosterId);
                    message.AppendString(thread.PosterName);
                    message.AppendString(thread.Subject);
                    message.AppendBool(thread.Pinned);
                    message.AppendBool(thread.Locked);
                    message.AppendInteger((Azure.GetUnixTimeStamp() - thread.Timestamp));
                    message.AppendInteger(thread.MessageCount + 1);
                    message.AppendInteger(0);
                    message.AppendInteger(0);
                    message.AppendInteger(0);
                    message.AppendString("");
                    message.AppendInteger((Azure.GetUnixTimeStamp() - thread.Timestamp));
                    message.AppendByte((thread.Hidden) ? 10 : 1);
                    message.AppendInteger(0);
                    message.AppendString(thread.Hider);
                    message.AppendInteger(0);
                }
                Session.SendMessage(message);
            }
        }

        /// <summary>
        /// Gets the group forum data.
        /// </summary>
        internal void GetGroupForumData()
        {
            uint groupId = Request.GetUInteger();
            Guild @group = Azure.GetGame().GetGroupManager().GetGroup(groupId);
            if (@group != null && @group.HasForum)
            {
                Session.SendMessage(@group.ForumDataMessage(Session.GetHabbo().Id));
            }
        }

        /// <summary>
        /// Gets the group forums.
        /// </summary>
        internal void GetGroupForums()
        {
            int selectType = Request.GetInteger();
            int startIndex = Request.GetInteger();
            var message = new ServerMessage(LibraryParser.OutgoingRequest("GroupForumListingsMessageComposer"));
            message.AppendInteger(selectType);
            var groupList = new List<Guild>();
            switch (selectType)
            {
                case 0:
                case 1:
                    using (IQueryAdapter dbClient = Azure.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.SetQuery("SELECT count(id) FROM groups_data WHERE has_forum = '1' AND forum_Messages_count > 0");
                        int qtdForums = dbClient.GetInteger();
                        dbClient.SetQuery("SELECT id FROM groups_data WHERE has_forum = '1' AND forum_Messages_count > 0 ORDER BY forum_Messages_count DESC LIMIT @startIndex, @totalPerPage;");
                        dbClient.AddParameter("startIndex", startIndex);
                        dbClient.AddParameter("totalPerPage", TotalPerPage);
                        DataTable table = dbClient.GetTable();
                        message.AppendInteger(qtdForums == 0 ? 1 : qtdForums);
                        message.AppendInteger(startIndex);
                        foreach (DataRow rowGroupData in table.Rows)
                        {
                            uint groupId = uint.Parse(rowGroupData["id"].ToString());
                            Guild guild = Azure.GetGame().GetGroupManager().GetGroup(groupId);
                            groupList.Add(guild);
                        }
                        message.AppendInteger(table.Rows.Count);
                        foreach (Guild @group in groupList)
                            @group.SerializeForumRoot(message);
                        Session.SendMessage(message);
                    }
                    break;

                case 2:
                    foreach (GroupMember groupUser in Session.GetHabbo().UserGroups)
                    {
                        Guild aGroup = Azure.GetGame().GetGroupManager().GetGroup(groupUser.GroupId);
                        if (aGroup != null && aGroup.HasForum)
                        {
                            groupList.Add(aGroup);
                        }
                    }
                    message.AppendInteger(groupList.Count == 0 ? 1 : groupList.Count);
                    groupList = groupList.OrderByDescending(x => x.ForumMessagesCount).Skip(startIndex).Take(20).ToList();
                    message.AppendInteger(startIndex);
                    message.AppendInteger(groupList.Count);
                    foreach (Guild @group in groupList)
                        @group.SerializeForumRoot(message);
                    Session.SendMessage(message);
                    break;

                default:
                    message.AppendInteger(1);
                    message.AppendInteger(startIndex);
                    message.AppendInteger(0);
                    Session.SendMessage(message);
                    break;
            }
        }

        /// <summary>
        /// Manages the group.
        /// </summary>
        internal void ManageGroup()
        {
            var groupId = Request.GetUInteger();
            var group = Azure.GetGame().GetGroupManager().GetGroup(groupId);
            if (group == null)
                return;
            if (!@group.Admins.ContainsKey(Session.GetHabbo().Id) && @group.CreatorId != Session.GetHabbo().Id &&
                Session.GetHabbo().Rank < 7)
                return;
            Response.Init(LibraryParser.OutgoingRequest("GroupDataEditMessageComposer"));
            Response.AppendInteger(0);
            Response.AppendBool(true);
            Response.AppendInteger(@group.Id);
            Response.AppendString(@group.Name);
            Response.AppendString(@group.Description);
            Response.AppendInteger(@group.RoomId);
            Response.AppendInteger(@group.Colour1);
            Response.AppendInteger(@group.Colour2);
            Response.AppendInteger(@group.State);
            Response.AppendInteger(@group.AdminOnlyDeco);
            Response.AppendBool(false);
            Response.AppendString("");
            var array = @group.Badge.Replace("b", "").Split('s');
            Response.AppendInteger(5);
            var num = (5 - array.Length);

            var num2 = 0;
            var array2 = array;
            foreach (var text in array2)
            {
                Response.AppendInteger((text.Length >= 6)
                    ? uint.Parse(text.Substring(0, 3))
                    : uint.Parse(text.Substring(0, 2)));
                Response.AppendInteger((text.Length >= 6)
                    ? uint.Parse(text.Substring(3, 2))
                    : uint.Parse(text.Substring(2, 2)));
                if (text.Length < 5)
                    Response.AppendInteger(0);
                else if (text.Length >= 6)
                    Response.AppendInteger(uint.Parse(text.Substring(5, 1)));
                else
                    Response.AppendInteger(uint.Parse(text.Substring(4, 1)));
            }
            while (num2 != num)
            {
                Response.AppendInteger(0);
                Response.AppendInteger(0);
                Response.AppendInteger(0);
                num2++;
            }
            Response.AppendString(@group.Badge);
            Response.AppendInteger(@group.Members.Count);
            SendResponse();
        }

        /// <summary>
        /// Updates the name of the group.
        /// </summary>
        internal void UpdateGroupName()
        {
            uint num = Request.GetUInteger();
            string text = Request.GetString();
            string text2 = Request.GetString();
            Guild group = Azure.GetGame().GetGroupManager().GetGroup(num);
            if (group == null)
                return;
            if (group.CreatorId != Session.GetHabbo().Id)
                return;
            using (IQueryAdapter queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery(string.Format("UPDATE groups_data SET `name`=@name, `desc`=@desc WHERE id={0} LIMIT 1", num));
                queryReactor.AddParameter("name", text);
                queryReactor.AddParameter("desc", text2);
                queryReactor.RunQuery();
            }
            group.Name = text;
            group.Description = text2;
            Azure.GetGame().GetGroupManager().SerializeGroupInfo(group, Response, Session, Session.GetHabbo().CurrentRoom);
        }

        /// <summary>
        /// Updates the group badge.
        /// </summary>
        internal void UpdateGroupBadge()
        {
            uint guildId = Request.GetUInteger();
            Guild guild = Azure.GetGame().GetGroupManager().GetGroup(guildId);
            if (guild != null)
            {
                Room room = Azure.GetGame().GetRoomManager().GetRoom(guild.RoomId);
                if (room != null)
                {
                    Request.GetInteger();
                    int Base = Request.GetInteger();
                    int baseColor = Request.GetInteger();
                    Request.GetInteger();
                    var guildStates = new List<int>();

                    for (int i = 0; i < 12; i++)
                    {
                        int item = Request.GetInteger();
                        guildStates.Add(item);
                    }
                    string badge = Azure.GetGame().GetGroupManager().GenerateGuildImage(Base, baseColor, guildStates);
                    guild.Badge = badge;
                    Response.Init(LibraryParser.OutgoingRequest("RoomGroupMessageComposer"));
                    Response.AppendInteger(room.LoadedGroups.Count);
                    foreach (KeyValuePair<uint, string> current2 in room.LoadedGroups)
                    {
                        Response.AppendInteger(current2.Key);
                        Response.AppendString(current2.Value);
                    }
                    room.SendMessage(Response);
                    Azure.GetGame().GetGroupManager().SerializeGroupInfo(guild, Response, Session, room);
                    using (IQueryAdapter queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
                    {
                        queryReactor.SetQuery(string.Format("UPDATE groups_data SET badge = @badgi WHERE id = {0}", guildId));
                        queryReactor.AddParameter("badgi", badge);
                        queryReactor.RunQuery();
                    }

                    if (Session.GetHabbo().CurrentRoom != null)
                    {
                        Session.GetHabbo().CurrentRoom.LoadedGroups[guildId] = guild.Badge;
                        Response.Init(LibraryParser.OutgoingRequest("RoomGroupMessageComposer"));
                        Response.AppendInteger(Session.GetHabbo().CurrentRoom.LoadedGroups.Count);
                        foreach (KeyValuePair<uint, string> current in Session.GetHabbo().CurrentRoom.LoadedGroups)
                        {
                            Response.AppendInteger(current.Key);
                            Response.AppendString(current.Value);
                        }
                        Session.GetHabbo().CurrentRoom.SendMessage(Response);
                        Azure.GetGame().GetGroupManager().SerializeGroupInfo(guild, Response, Session, Session.GetHabbo().CurrentRoom);
                    }
                }
            }
        }

        /// <summary>
        /// Updates the group colours.
        /// </summary>
        internal void UpdateGroupColours()
        {
            uint groupId = Request.GetUInteger();
            int num = Request.GetInteger();
            int num2 = Request.GetInteger();
            Guild group = Azure.GetGame().GetGroupManager().GetGroup(groupId);
            if (group == null)
                return;
            if (group.CreatorId != Session.GetHabbo().Id)
                return;
            using (IQueryAdapter queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.RunFastQuery(string.Concat("UPDATE groups_data SET colour1= ", num, ", colour2=", num2, " WHERE id=", @group.Id, " LIMIT 1"));
            }
            group.Colour1 = num;
            group.Colour2 = num2;
            Azure.GetGame().GetGroupManager().SerializeGroupInfo(group, Response, Session, Session.GetHabbo().CurrentRoom);
        }

        /// <summary>
        /// Updates the group settings.
        /// </summary>
        internal void UpdateGroupSettings()
        {
            uint groupId = Request.GetUInteger();
            uint num = Request.GetUInteger();
            uint num2 = Request.GetUInteger();
            Guild group = Azure.GetGame().GetGroupManager().GetGroup(groupId);
            if (group == null)
                return;
            if (group.CreatorId != Session.GetHabbo().Id)
                return;
            using (IQueryAdapter queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.RunFastQuery(string.Concat("UPDATE groups_data SET state='", num, "', admindeco='", num2, "' WHERE id=", @group.Id, " LIMIT 1"));
            }
            group.State = num;
            group.AdminOnlyDeco = num2;
            Room room = Azure.GetGame().GetRoomManager().GetRoom(group.RoomId);
            if (room != null)
            {
                foreach (RoomUser current in room.GetRoomUserManager().GetRoomUsers())
                {
                    if (room.RoomData.OwnerId != current.UserId && !group.Admins.ContainsKey(current.UserId) && group.Members.ContainsKey(current.UserId))
                    {
                        if (num2 == 1u)
                        {
                            current.RemoveStatus("flatctrl 1");
                            Response.Init(LibraryParser.OutgoingRequest("RoomRightsLevelMessageComposer"));
                            Response.AppendInteger(0);
                            current.GetClient().SendMessage(GetResponse());
                        }
                        else
                        {
                            if (num2 == 0u && !current.Statusses.ContainsKey("flatctrl 1"))
                            {
                                current.AddStatus("flatctrl 1", "");
                                Response.Init(LibraryParser.OutgoingRequest("RoomRightsLevelMessageComposer"));
                                Response.AppendInteger(1);
                                current.GetClient().SendMessage(GetResponse());
                            }
                        }
                        current.UpdateNeeded = true;
                    }
                }
            }
            Azure.GetGame().GetGroupManager().SerializeGroupInfo(group, Response, Session, Session.GetHabbo().CurrentRoom);
        }

        /// <summary>
        /// Requests the leave group.
        /// </summary>
        internal void RequestLeaveGroup()
        {
            uint groupId = Request.GetUInteger();
            uint userId = Request.GetUInteger();
            Guild guild = Azure.GetGame().GetGroupManager().GetGroup(groupId);
            if (guild == null || guild.CreatorId == userId)
                return;
            if (userId == Session.GetHabbo().Id || guild.Admins.ContainsKey(Session.GetHabbo().Id))
            {
                Response.Init(LibraryParser.OutgoingRequest("GroupAreYouSureMessageComposer"));
                Response.AppendInteger(userId);
                Response.AppendInteger(0);
                SendResponse();
            }
        }

        /// <summary>
        /// Confirms the leave group.
        /// </summary>
        internal void ConfirmLeaveGroup()
        {
            uint guild = Request.GetUInteger();
            uint userId = Request.GetUInteger();
            Guild byeGuild = Azure.GetGame().GetGroupManager().GetGroup(guild);
            if (byeGuild == null)
                return;
            if (byeGuild.CreatorId == userId)
            {
                Session.SendNotif(Azure.GetLanguage().GetVar("user_room_video_true"));
                return;
            }
            int type = 3;
            if (userId == Session.GetHabbo().Id || byeGuild.Admins.ContainsKey(Session.GetHabbo().Id))
            {
                GroupMember memberShip;
                if (byeGuild.Members.ContainsKey(userId))
                {
                    memberShip = byeGuild.Members[userId];
                    type = 3;
                    Session.GetHabbo().UserGroups.Remove(memberShip);
                    byeGuild.Members.Remove(userId);
                }
                else if (byeGuild.Admins.ContainsKey(userId))
                {
                    memberShip = byeGuild.Admins[userId];
                    type = 1;
                    Session.GetHabbo().UserGroups.Remove(memberShip);
                    byeGuild.Admins.Remove(userId);
                }
                else
                    return;
                using (IQueryAdapter queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
                {
                    queryReactor.RunFastQuery(string.Concat("DELETE FROM groups_members WHERE user_id=", userId, " AND group_id=", guild, " LIMIT 1"));
                }
                Habbo byeUser = Azure.GetHabboById(userId);
                if (byeUser != null)
                {
                    Response.Init(LibraryParser.OutgoingRequest("GroupConfirmLeaveMessageComposer"));
                    Response.AppendInteger(guild);
                    Response.AppendInteger(type);
                    Response.AppendInteger(byeUser.Id);
                    Response.AppendString(byeUser.UserName);
                    Response.AppendString(byeUser.Look);
                    Response.AppendString("");
                    SendResponse();
                }
                if (byeUser != null && byeUser.FavouriteGroup == guild)
                {
                    byeUser.FavouriteGroup = 0;
                    using (IQueryAdapter queryreactor2 = Azure.GetDatabaseManager().GetQueryReactor())
                        queryreactor2.RunFastQuery(string.Format("UPDATE users_stats SET favourite_group=0 WHERE id={0} LIMIT 1", userId));
                    Room room = Session.GetHabbo().CurrentRoom;

                    Response.Init(LibraryParser.OutgoingRequest("FavouriteGroupMessageComposer"));
                    Response.AppendInteger(byeUser.Id);
                    if (room != null)
                        room.SendMessage(Response);
                    else
                        SendResponse();
                }

                Response.Init(LibraryParser.OutgoingRequest("GroupRequestReloadMessageComposer"));
                Response.AppendInteger(guild);
                SendResponse();
            }
        }

        /// <summary>
        /// News the method.
        /// </summary>
        /// <param name="current2">The current2.</param>
        private void NewMethod(RoomData current2)
        {
            Response.AppendInteger(current2.Id);
            Response.AppendString(current2.Name);
            Response.AppendBool(false);
        }

        internal void UpdateForumSettings()
        {
            uint guild = Request.GetUInteger();
            int whoCanRead = Request.GetInteger();
            int whoCanPost = Request.GetInteger();
            int whoCanThread = Request.GetInteger();
            int whoCanMod = Request.GetInteger();
            Guild group = Azure.GetGame().GetGroupManager().GetGroup(guild);
            if (group == null) return;
            group.WhoCanRead = whoCanRead;
            group.WhoCanPost = whoCanPost;
            group.WhoCanThread = whoCanThread;
            group.WhoCanMod = whoCanMod;
            using (IQueryAdapter queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery("UPDATE groups_data SET who_can_read = @who_can_read, who_can_post = @who_can_post, who_can_thread = @who_can_thread, who_can_mod = @who_can_mod WHERE id = @group_id");
                queryReactor.AddParameter("group_id", group.Id);
                queryReactor.AddParameter("who_can_read", whoCanRead);
                queryReactor.AddParameter("who_can_post", whoCanPost);
                queryReactor.AddParameter("who_can_thread", whoCanThread);
                queryReactor.AddParameter("who_can_mod", whoCanMod);
                queryReactor.RunQuery();
            }
            Session.SendMessage(group.ForumDataMessage(Session.GetHabbo().Id));
        }

        internal void DeleteGroup()
        {
            uint groupId = Request.GetUInteger();
            var group = Azure.GetGame().GetGroupManager().GetGroup(groupId);
            var room = Azure.GetGame().GetRoomManager().GetRoom(group.RoomId);
            if (room == null || room.RoomData == null || room.RoomData.Group == null)
            {
                Session.SendNotif(Azure.GetLanguage().GetVar("command_group_has_no_room"));
            }
            else
            {
                foreach (var user in group.Members.Values)
                {
                    var clientByUserId = Azure.GetGame().GetClientManager().GetClientByUserId(user.Id);
                    if (clientByUserId == null) continue;
                    clientByUserId.GetHabbo().UserGroups.Remove(user);
                    if (clientByUserId.GetHabbo().FavouriteGroup == group.Id) clientByUserId.GetHabbo().FavouriteGroup = 0;
                }
                room.RoomData.Group = null;
                room.RoomData.GroupId = 0;
                Azure.GetGame().GetGroupManager().DeleteGroup(group.Id);
                var deleteGroup = new ServerMessage(LibraryParser.OutgoingRequest("GroupDeletedMessageComposer"));
                deleteGroup.AppendInteger(groupId);
                room.SendMessage(deleteGroup);
                var roomItemList = room.GetRoomItemHandler().RemoveAllFurniture(Session);
                room.GetRoomItemHandler().RemoveItemsByOwner(ref roomItemList, ref Session);
                var roomData = room.RoomData;
                var roomId = room.RoomData.Id;
                Azure.GetGame().GetRoomManager().UnloadRoom(room, "Delete room");
                Azure.GetGame().GetRoomManager().QueueVoteRemove(roomData);
                using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
                {
                    queryReactor.RunFastQuery(string.Format("DELETE FROM rooms_data WHERE id = {0}", roomId));
                    queryReactor.RunFastQuery(string.Format("DELETE FROM users_favorites WHERE room_id = {0}", roomId));
                    queryReactor.RunFastQuery(string.Format("DELETE FROM items_rooms WHERE room_id = {0}", roomId));
                    queryReactor.RunFastQuery(string.Format("DELETE FROM rooms_rights WHERE room_id = {0}", roomId));
                    queryReactor.RunFastQuery(string.Format("UPDATE users SET home_room = '0' WHERE home_room = {0}",
                        roomId));
                }
                var roomData2 = (
                    from p in Session.GetHabbo().UsersRooms
                    where p.Id == roomId
                    select p).SingleOrDefault();
                if (roomData2 != null)
                    Session.GetHabbo().UsersRooms.Remove(roomData2);
            }
        }
    }
}