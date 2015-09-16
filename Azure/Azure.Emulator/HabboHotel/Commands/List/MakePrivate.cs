using Azure.Database.Manager.Database.Session_Details.Interfaces;
using Azure.HabboHotel.GameClients;
using Azure.HabboHotel.Rooms;
using Azure.HabboHotel.Users;
using Azure.Messages;
using Azure.Messages.Parsers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using Azure.HabboHotel.Rooms.RoomInvokedItems;

namespace Azure.HabboHotel.Commands.List
{
    internal sealed class MakePrivate : Command
    {
        public MakePrivate()
        {
            MinRank = 7;
            Description = "Haz una sala privada.";
            Usage = ":makeprivate";
            MinParams = 0;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var room = session.GetHabbo().CurrentRoom;
            using (IQueryAdapter queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
                queryReactor.RunFastQuery(string.Format("UPDATE rooms_data SET roomtype = 'private' WHERE id = {0}", room.RoomId));
            var roomId = session.GetHabbo().CurrentRoom.RoomId;
            var users = new List<RoomUser>(session.GetHabbo().CurrentRoom.GetRoomUserManager().UserList.Values);

            Azure.GetGame().GetRoomManager().UnloadRoom(session.GetHabbo().CurrentRoom, "Unload command");

            Azure.GetGame().GetRoomManager().LoadRoom(roomId);

            var roomFwd = new ServerMessage(LibraryParser.OutgoingRequest("RoomForwardMessageComposer"));
            roomFwd.AppendInteger(roomId);

            var data = roomFwd.GetReversedBytes();

            foreach (var user in users.Where(user => user != null && user.GetClient() != null)) user.GetClient().SendMessage(data);

            return true;
        }
    }
}