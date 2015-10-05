#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using Azure.Database.Manager.Database.Session_Details.Interfaces;
using Azure.HabboHotel.Catalogs;
using Azure.HabboHotel.Groups;
using Azure.HabboHotel.Groups.Structs;
using Azure.HabboHotel.Rooms;
using Azure.HabboHotel.Users;
using Azure.Messages.Parsers;

#endregion

namespace Azure.Messages.Handlers
{
    /// <summary>
    /// Class GameClientMessageHandler.
    /// </summary>
    internal partial class GameClientMessageHandler
    {
        internal readonly ushort TOTAL_PER_PAGE = 20;

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

            Session.SendMessage(CatalogPacket.PurchaseOk(0u, "CREATE_GUILD", 10));
            Response.Init(LibraryParser.OutgoingRequest("GroupRoomMessageComposer"));
            Response.AppendInteger(roomid);
            Response.AppendInteger(group.Id);
            SendResponse();
            roomData.Group = group;
            roomData.GroupId = group.Id;
            roomData.SerializeRoomData(Response, Session, true, false);

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
            Azure.GetGame().GetGroupManager().SerializeGroupMembers(Response, group, 1u, Session, "", 0);
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
            Azure.GetGame().GetGroupManager().SerializeGroupMembers(Response, group, 0u, Session, "", 0);
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
            uint GroupId = Request.GetUInteger();
            uint UserId = Request.GetUInteger();
            Guild group = Azure.GetGame().GetGroupManager().GetGroup(GroupId);
            if (Session.GetHabbo().Id != group.CreatorId && !group.Admins.ContainsKey(Session.GetHabbo().Id) && !group.Requests.ContainsKey(UserId))
                return;
            if (group.Members.ContainsKey(UserId))
            {
                group.Requests.Remove(UserId);
                using (IQueryAdapter queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
                {
                    queryReactor.RunFastQuery(string.Format("DELETE FROM groups_requests WHERE group_id = '{0}' AND user_id = '{1}' LIMIT 1", GroupId, UserId));
                }
                return;
            }

            GroupMember memberGroup = group.Requests[UserId];
            memberGroup.DateJoin = Azure.GetUnixTimeStamp();
            group.Members.Add(UserId, memberGroup);
            group.Requests.Remove(UserId);
            group.Admins.Add(UserId, group.Members[UserId]);

            Azure.GetGame().GetGroupManager().SerializeGroupInfo(group, Response, Session, false);
            Response.Init(LibraryParser.OutgoingRequest("GroupMembersMessageComposer"));
            Azure.GetGame().GetGroupManager().SerializeGroupMembers(Response, group, 0u, Session, "", 0);
            SendResponse();

            using (IQueryAdapter queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.RunFastQuery(string.Format("DELETE FROM groups_requests WHERE group_id = '{0}' AND user_id = '{1}' LIMIT 1", GroupId, UserId));
            }
            using (IQueryAdapter queryreactor2 = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryreactor2.RunFastQuery(string.Format("INSERT INTO groups_members (group_id, user_id, rank, date_join) VALUES ('{0}','{1}','0','{2}')", GroupId, UserId, Azure.GetUnixTimeStamp()));
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
                !group.Requests.ContainsKey(userId)) return;

            group.Requests.Remove(userId);

            Response.Init(LibraryParser.OutgoingRequest("GroupMembersMessageComposer"));
            Azure.GetGame().GetGroupManager().SerializeGroupMembers(Response, group, 2u, Session, "", 0);
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
            Azure.GetGame().GetGroupManager().SerializeGroupInfo(group, Response, Session, false);
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
            uint GroupId = Request.GetUInteger();
            Guild group = Azure.GetGame().GetGroupManager().GetGroup(GroupId);
            Habbo user = Session.GetHabbo();

            if (!group.Members.ContainsKey(user.Id))
            {

                if (group.State == 0)
                {
                    using (IQueryAdapter queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
                    {
                        queryReactor.RunFastQuery(string.Concat("INSERT INTO groups_members (user_id, group_id, date_join) VALUES (", user.Id, ",", GroupId, ",", Azure.GetUnixTimeStamp(), ")"));
                        queryReactor.RunFastQuery(string.Concat("UPDATE users_stats SET favourite_group=", GroupId, " WHERE id= ", user.Id, " LIMIT 1"));
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
                            queryreactor2.RunFastQuery(string.Concat("INSERT INTO groups_requests (user_id, group_id) VALUES (", Session.GetHabbo().Id, ",", GroupId, ")"));
                        }
                        GroupMember groupRequest = new GroupMember(user.Id, user.UserName, user.Look, group.Id, 0, Azure.GetUnixTimeStamp());
                        group.Requests.Add(user.Id, groupRequest);
                    }
                }

                Azure.GetGame().GetGroupManager().SerializeGroupInfo(group, Response, Session, false);
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
                Azure.GetGame().GetGroupManager().SerializeGroupInfo(group, Response, Session, false);
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
            Azure.GetGame().GetGroupManager().SerializeGroupInfo(group, Response, Session, false);
            Response.Init(LibraryParser.OutgoingRequest("GroupMembersMessageComposer"));
            Azure.GetGame().GetGroupManager().SerializeGroupMembers(Response, group, 0u, Session, "", 0);
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
            Azure.GetGame().GetGroupManager().SerializeGroupInfo(group, Response, Session, false);
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
                    DataRow Row = dbClient.GetRow();
                    var Post = new GroupForumPost(Row);
                    if (Post.Locked || Post.Hidden)
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
                var Message = new ServerMessage(LibraryParser.OutgoingRequest("GroupForumNewThreadMessageComposer"));
                Message.AppendInteger(groupId);
                Message.AppendInteger(threadId);
                Message.AppendInteger(Session.GetHabbo().Id);
                Message.AppendString(subject);
                Message.AppendString(content);
                Message.AppendBool(false);
                Message.AppendBool(false);
                Message.AppendInteger((Azure.GetUnixTimeStamp() - timestamp));
                Message.AppendInteger(1);
                Message.AppendInteger(0);
                Message.AppendInteger(0);
                Message.AppendInteger(1);
                Message.AppendString("");
                Message.AppendInteger((Azure.GetUnixTimeStamp() - timestamp));
                Message.AppendByte(1);
                Message.AppendInteger(1);
                Message.AppendString("");
                Message.AppendInteger(42);//useless
                Session.SendMessage(Message);
            }
            else
            {
                var Message = new ServerMessage(LibraryParser.OutgoingRequest("GroupForumNewResponseMessageComposer"));
                Message.AppendInteger(groupId);
                Message.AppendInteger(threadId);
                Message.AppendInteger(group.ForumMessagesCount);
                Message.AppendInteger(0);
                Message.AppendInteger(Session.GetHabbo().Id);
                Message.AppendString(Session.GetHabbo().UserName);
                Message.AppendString(Session.GetHabbo().Look);
                Message.AppendInteger((Azure.GetUnixTimeStamp() - timestamp));
                Message.AppendString(content);
                Message.AppendByte(0);
                Message.AppendInteger(0);
                Message.AppendString("");
                Message.AppendInteger(0);
                Session.SendMessage(Message);
            }
        }

        /// <summary>
        /// Updates the state of the thread.
        /// </summary>
        internal void UpdateThreadState()
        {
            uint GroupId = Request.GetUInteger();
            uint ThreadId = Request.GetUInteger();
            bool Pin = Request.GetBool();
            bool Lock = Request.GetBool();
            using (IQueryAdapter dbClient = Azure.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(string.Format("SELECT * FROM groups_forums_posts WHERE group_id = '{0}' AND id = '{1}' LIMIT 1;", GroupId, ThreadId));
                DataRow Row = dbClient.GetRow();
                Guild Group = Azure.GetGame().GetGroupManager().GetGroup(GroupId);
                if (Row != null)
                {
                    if ((uint)Row["poster_id"] == Session.GetHabbo().Id || Group.Admins.ContainsKey(Session.GetHabbo().Id))
                    {
                        dbClient.SetQuery(string.Format("UPDATE groups_forums_posts SET pinned = @pin , locked = @lock WHERE id = {0};", ThreadId));
                        dbClient.AddParameter("pin", (Pin) ? "1" : "0");
                        dbClient.AddParameter("lock", (Lock) ? "1" : "0");
                        dbClient.RunQuery();
                    }
                }

                var Thread = new GroupForumPost(Row);
                if (Thread.Pinned != Pin)
                {
                    var Notif = new ServerMessage(LibraryParser.OutgoingRequest("SuperNotificationMessageComposer"));
                    Notif.AppendString((Pin) ? "forums.thread.pinned" : "forums.thread.unpinned");
                    Notif.AppendInteger(0);
                    Session.SendMessage(Notif);
                }
                if (Thread.Locked != Lock)
                {
                    var Notif2 = new ServerMessage(LibraryParser.OutgoingRequest("SuperNotificationMessageComposer"));
                    Notif2.AppendString((Lock) ? "forums.thread.locked" : "forums.thread.unlocked");
                    Notif2.AppendInteger(0);
                    Session.SendMessage(Notif2);
                }
                if (Thread.ParentId != 0)
                    return;
                var Message = new ServerMessage(LibraryParser.OutgoingRequest("GroupForumThreadUpdateMessageComposer"));
                Message.AppendInteger(GroupId);
                Message.AppendInteger(Thread.Id);
                Message.AppendInteger(Thread.PosterId);
                Message.AppendString(Thread.PosterName);
                Message.AppendString(Thread.Subject);
                Message.AppendBool(Pin);
                Message.AppendBool(Lock);
                Message.AppendInteger((Azure.GetUnixTimeStamp() - Thread.Timestamp));
                Message.AppendInteger(Thread.MessageCount + 1);
                Message.AppendInteger(0);
                Message.AppendInteger(0);
                Message.AppendInteger(1);
                Message.AppendString("");
                Message.AppendInteger((Azure.GetUnixTimeStamp() - Thread.Timestamp));
                Message.AppendByte((Thread.Hidden) ? 10 : 1);
                Message.AppendInteger(1);
                Message.AppendString(Thread.Hider);
                Message.AppendInteger(0);
                Session.SendMessage(Message);
            }
        }

        /// <summary>
        /// Alters the state of the forum thread.
        /// </summary>
        internal void AlterForumThreadState()
        {
            uint GroupId = Request.GetUInteger();
            uint ThreadId = Request.GetUInteger();
            int StateToSet = Request.GetInteger();
            using (IQueryAdapter dbClient = Azure.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(string.Format("SELECT * FROM groups_forums_posts WHERE group_id = '{0}' AND id = '{1}' LIMIT 1;", GroupId, ThreadId));
                DataRow Row = dbClient.GetRow();
                Guild Group = Azure.GetGame().GetGroupManager().GetGroup(GroupId);
                if (Row != null)
                {
                    if ((uint)Row["poster_id"] == Session.GetHabbo().Id || Group.Admins.ContainsKey(Session.GetHabbo().Id))
                    {
                        dbClient.SetQuery(string.Format("UPDATE groups_forums_posts SET hidden = @hid WHERE id = {0};", ThreadId));
                        dbClient.AddParameter("hid", (StateToSet == 20) ? "1" : "0");
                        dbClient.RunQuery();
                    }
                }
                var Thread = new GroupForumPost(Row);
                var Notif = new ServerMessage(LibraryParser.OutgoingRequest("SuperNotificationMessageComposer"));
                Notif.AppendString((StateToSet == 20) ? "forums.thread.hidden" : "forums.thread.restored");
                Notif.AppendInteger(0);
                Session.SendMessage(Notif);
                if (Thread.ParentId != 0)
                    return;
                var Message = new ServerMessage(LibraryParser.OutgoingRequest("GroupForumThreadUpdateMessageComposer"));
                Message.AppendInteger(GroupId);
                Message.AppendInteger(Thread.Id);
                Message.AppendInteger(Thread.PosterId);
                Message.AppendString(Thread.PosterName);
                Message.AppendString(Thread.Subject);
                Message.AppendBool(Thread.Pinned);
                Message.AppendBool(Thread.Locked);
                Message.AppendInteger((Azure.GetUnixTimeStamp() - Thread.Timestamp));
                Message.AppendInteger(Thread.MessageCount + 1);
                Message.AppendInteger(0);
                Message.AppendInteger(0);
                Message.AppendInteger(0);
                Message.AppendString("");
                Message.AppendInteger((Azure.GetUnixTimeStamp() - Thread.Timestamp));
                Message.AppendByte(StateToSet);
                Message.AppendInteger(0);
                Message.AppendString(Thread.Hider);
                Message.AppendInteger(0);
                Session.SendMessage(Message);
            }
        }

        /// <summary>
        /// Reads the forum thread.
        /// </summary>
        internal void ReadForumThread()
        {
            uint GroupId = Request.GetUInteger();
            uint ThreadId = Request.GetUInteger();
            int StartIndex = Request.GetInteger();
            int EndIndex = Request.GetInteger();
            Guild Group = Azure.GetGame().GetGroupManager().GetGroup(GroupId);
            if (Group == null || !Group.HasForum)
                return;
            using (IQueryAdapter dbClient = Azure.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(string.Format("SELECT * FROM groups_forums_posts WHERE group_id = '{0}' AND parent_id = '{1}' OR id = '{2}' ORDER BY timestamp ASC;", GroupId, ThreadId, ThreadId));
                DataTable Table = dbClient.GetTable();
                if (Table == null)
                    return;
                int b = (Table.Rows.Count <= 20) ? Table.Rows.Count : 20;
                var posts = new List<GroupForumPost>();
                int i = 1;
                while (i <= b)
                {
                    DataRow Row = Table.Rows[i - 1];
                    if (Row == null)
                    {
                        b--;
                        continue;
                    }
                    var thread = new GroupForumPost(Row);
                    if (thread.ParentId == 0 && thread.Hidden)
                        return;
                    posts.Add(thread);
                    i++;
                }

                var Message = new ServerMessage(LibraryParser.OutgoingRequest("GroupForumReadThreadMessageComposer"));
                Message.AppendInteger(GroupId);
                Message.AppendInteger(ThreadId);
                Message.AppendInteger(StartIndex);
                Message.AppendInteger(b);
                int indx = 0;
                foreach (GroupForumPost Post in posts)
                {
                    Message.AppendInteger(indx++ - 1);
                    Message.AppendInteger(indx - 1);
                    Message.AppendInteger(Post.PosterId);
                    Message.AppendString(Post.PosterName);
                    Message.AppendString(Post.PosterLook);
                    Message.AppendInteger((Azure.GetUnixTimeStamp() - Post.Timestamp));
                    Message.AppendString(Post.PostContent);
                    Message.AppendByte(0);
                    Message.AppendInteger(0);
                    Message.AppendString(Post.Hider);
                    Message.AppendInteger(0);
                }
                Session.SendMessage(Message);
            }
        }

        /// <summary>
        /// Gets the group forum thread root.
        /// </summary>
        internal void GetGroupForumThreadRoot()
        {
            uint GroupId = Request.GetUInteger();
            int StartIndex = Request.GetInteger();
            using (IQueryAdapter dbClient = Azure.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(string.Format("SELECT count(id) FROM groups_forums_posts WHERE group_id = '{0}' AND parent_id = 0", GroupId));
                int totalThreads = dbClient.GetInteger();
                dbClient.SetQuery(string.Format("SELECT * FROM groups_forums_posts WHERE group_id = '{0}' AND parent_id = 0 ORDER BY timestamp DESC, pinned DESC LIMIT @startIndex, @totalPerPage;", GroupId));
                dbClient.AddParameter("startIndex", StartIndex);
                dbClient.AddParameter("totalPerPage", TOTAL_PER_PAGE);
                DataTable Table = dbClient.GetTable();
                int threadCount = (Table.Rows.Count <= TOTAL_PER_PAGE) ? Table.Rows.Count : TOTAL_PER_PAGE;
                var Threads = new List<GroupForumPost>();
                foreach(DataRow row in Table.Rows) {
                    var thread = new GroupForumPost(row);
                    Threads.Add(thread);
                }
                var Message = new ServerMessage(LibraryParser.OutgoingRequest("GroupForumThreadRootMessageComposer"));
                Message.AppendInteger(GroupId);
                Message.AppendInteger(StartIndex);
                Message.AppendInteger(threadCount);
                foreach (GroupForumPost Thread in Threads)
                {
                    Message.AppendInteger(Thread.Id);
                    Message.AppendInteger(Thread.PosterId);
                    Message.AppendString(Thread.PosterName);
                    Message.AppendString(Thread.Subject);
                    Message.AppendBool(Thread.Pinned);
                    Message.AppendBool(Thread.Locked);
                    Message.AppendInteger((Azure.GetUnixTimeStamp() - Thread.Timestamp));
                    Message.AppendInteger(Thread.MessageCount + 1);
                    Message.AppendInteger(0);
                    Message.AppendInteger(0);
                    Message.AppendInteger(0);
                    Message.AppendString("");
                    Message.AppendInteger((Azure.GetUnixTimeStamp() - Thread.Timestamp));
                    Message.AppendByte((Thread.Hidden) ? 10 : 1);
                    Message.AppendInteger(0);
                    Message.AppendString(Thread.Hider);
                    Message.AppendInteger(0);
                }
                Session.SendMessage(Message);
            }
        }

        /// <summary>
        /// Gets the group forum data.
        /// </summary>
        internal void GetGroupForumData()
        {
            uint GroupId = Request.GetUInteger();
            Guild Group = Azure.GetGame().GetGroupManager().GetGroup(GroupId);
            if (Group != null && Group.HasForum)
            {
                Session.SendMessage(Group.ForumDataMessage(Session.GetHabbo().Id));
            }
        }

        /// <summary>
        /// Gets the group forums.
        /// </summary>
        internal void GetGroupForums()
        {
            int SelectType = Request.GetInteger();
            int StartIndex = Request.GetInteger();
            var Message = new ServerMessage(LibraryParser.OutgoingRequest("GroupForumListingsMessageComposer"));
            Message.AppendInteger(SelectType);
            var GroupList = new List<Guild>();
            switch (SelectType)
            {
                case 0:
                case 1:
                    using (IQueryAdapter dbClient = Azure.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.SetQuery("SELECT count(id) FROM groups_data WHERE has_forum = '1' AND forum_Messages_count > 0");
                        int qtdForums = dbClient.GetInteger();
                        dbClient.SetQuery("SELECT id FROM groups_data WHERE has_forum = '1' AND forum_Messages_count > 0 ORDER BY forum_Messages_count DESC LIMIT @startIndex, @totalPerPage;");
                        dbClient.AddParameter("startIndex", StartIndex);
                        dbClient.AddParameter("totalPerPage", TOTAL_PER_PAGE);
                        DataTable Table = dbClient.GetTable();
                        Message.AppendInteger(qtdForums == 0 ? 1 : qtdForums);
                        Message.AppendInteger(StartIndex);
                        foreach(DataRow rowGroupData in Table.Rows) {
                            uint GroupId = uint.Parse(rowGroupData["id"].ToString());
                            Guild Guild = Azure.GetGame().GetGroupManager().GetGroup(GroupId);
                            GroupList.Add(Guild);
                        }
                        Message.AppendInteger(Table.Rows.Count);
                        foreach (Guild Group in GroupList) 
                            Group.SerializeForumRoot(Message);
                        Session.SendMessage(Message);
                    }
                    break;

                case 2:
                    foreach (GroupMember groupUser in Session.GetHabbo().UserGroups)
                    {
                        Guild AGroup = Azure.GetGame().GetGroupManager().GetGroup(groupUser.GroupId);
                        if (AGroup != null && AGroup.HasForum)
                        {
                            GroupList.Add(AGroup);
                        }
                    }
                    Message.AppendInteger(GroupList.Count == 0 ? 1 : GroupList.Count);
                    GroupList = GroupList.OrderByDescending(x => x.ForumMessagesCount).Skip(StartIndex).Take(20).ToList();
                    Message.AppendInteger(StartIndex);
                    Message.AppendInteger(GroupList.Count);
                    foreach (Guild Group in GroupList)
                        Group.SerializeForumRoot(Message);
                    Session.SendMessage(Message);
                    break;
                default:
                    Message.AppendInteger(1);
                    Message.AppendInteger(StartIndex);
                    Message.AppendInteger(0);
                    Session.SendMessage(Message);
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
            Azure.GetGame().GetGroupManager().SerializeGroupInfo(group, Response, Session, Session.GetHabbo().CurrentRoom, false);
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
                    Azure.GetGame().GetGroupManager().SerializeGroupInfo(guild, Response, Session, room, false);
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
                        Azure.GetGame().GetGroupManager().SerializeGroupInfo(guild, Response, Session, Session.GetHabbo().CurrentRoom, false);
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
            Azure.GetGame().GetGroupManager().SerializeGroupInfo(group, Response, Session, Session.GetHabbo().CurrentRoom, false);
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
            Azure.GetGame().GetGroupManager().SerializeGroupInfo(group, Response, Session, Session.GetHabbo().CurrentRoom, false);
        }

        /// <summary>
        /// Requests the leave group.
        /// </summary>
        internal void RequestLeaveGroup()
        {
            uint GroupId = Request.GetUInteger();
            uint UserId = Request.GetUInteger();
            Guild Guild = Azure.GetGame().GetGroupManager().GetGroup(GroupId);
            if (Guild == null || Guild.CreatorId == UserId)
                return;
            if (UserId == Session.GetHabbo().Id || Guild.Admins.ContainsKey(Session.GetHabbo().Id))
            {
                Response.Init(LibraryParser.OutgoingRequest("GroupAreYouSureMessageComposer"));
                Response.AppendInteger(UserId);
                Response.AppendInteger(0);
                SendResponse();
            }
        }

        /// <summary>
        /// Confirms the leave group.
        /// </summary>
        internal void ConfirmLeaveGroup()
        {
            uint Guild = Request.GetUInteger();
            uint UserId = Request.GetUInteger();
            Guild byeGuild = Azure.GetGame().GetGroupManager().GetGroup(Guild);
            if (byeGuild == null)
                return;
            if (byeGuild.CreatorId == UserId)
            {
                Session.SendNotif(Azure.GetLanguage().GetVar("user_room_video_true"));
                return;
            }
            int type = 3;
            if (UserId == Session.GetHabbo().Id || byeGuild.Admins.ContainsKey(Session.GetHabbo().Id))
            {
                GroupMember memberShip;
                if (byeGuild.Members.ContainsKey(UserId))
                {
                    memberShip = byeGuild.Members[UserId];
                    type = 3;
                    Session.GetHabbo().UserGroups.Remove(memberShip);
                    byeGuild.Members.Remove(UserId);
                }
                else if (byeGuild.Admins.ContainsKey(UserId))
                {
                    memberShip = byeGuild.Admins[UserId];
                    type = 1;
                    Session.GetHabbo().UserGroups.Remove(memberShip);
                    byeGuild.Admins.Remove(UserId);
                }
                else
                    return;
                using (IQueryAdapter queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
                {
                    queryReactor.RunFastQuery(string.Concat("DELETE FROM groups_members WHERE user_id=", UserId, " AND group_id=", Guild, " LIMIT 1"));
                }
                Habbo byeUser = Azure.GetHabboById(UserId);
                if (byeUser != null)
                {
                    Response.Init(LibraryParser.OutgoingRequest("GroupConfirmLeaveMessageComposer"));
                    Response.AppendInteger(Guild);
                    Response.AppendInteger(type);
                    Response.AppendInteger(byeUser.Id);
                    Response.AppendString(byeUser.UserName);
                    Response.AppendString(byeUser.Look);
                    Response.AppendString("");
                    SendResponse();
                }
                if (byeUser != null && byeUser.FavouriteGroup == Guild)
                {
                    byeUser.FavouriteGroup = 0;
                    using (IQueryAdapter queryreactor2 = Azure.GetDatabaseManager().GetQueryReactor())
                        queryreactor2.RunFastQuery(string.Format("UPDATE users_stats SET favourite_group=0 WHERE id={0} LIMIT 1", UserId));
                    Room Room = Session.GetHabbo().CurrentRoom;

                    Response.Init(LibraryParser.OutgoingRequest("FavouriteGroupMessageComposer"));
                    Response.AppendInteger(byeUser.Id);
                    if (Room != null)
                        Room.SendMessage(Response);
                    else
                        SendResponse();
                }

                Response.Init(LibraryParser.OutgoingRequest("GroupRequestReloadMessageComposer"));
                Response.AppendInteger(Guild);
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
                    select p).SingleOrDefault<RoomData>();
                if (roomData2 != null)
                    Session.GetHabbo().UsersRooms.Remove(roomData2);
            }
        }
    }
}