using System.Collections.Generic;
using System.Linq;
using Azure.Game.Commands.Interfaces;
using Azure.Game.GameClients.Interfaces;
using Azure.Game.Rooms.User;
using Azure.Messages;
using Azure.Messages.Parsers;

namespace Azure.Game.Commands.Controllers
{
    internal sealed class MakePublic : Command
    {
        public MakePublic()
        {
            MinRank = 7;
            Description = "Haz una sala publica.";
            Usage = ":makepublic";
            MinParams = 0;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var room = session.GetHabbo().CurrentRoom;

            using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
                queryReactor.RunFastQuery($"UPDATE rooms_data SET roomtype = 'public' WHERE id = {room.RoomId}");

            var roomId = session.GetHabbo().CurrentRoom.RoomId;
            var users = new List<RoomUser>(session.GetHabbo().CurrentRoom.GetRoomUserManager().UserList.Values);

            Azure.GetGame().GetRoomManager().UnloadRoom(session.GetHabbo().CurrentRoom, "Unload command");

            Azure.GetGame().GetRoomManager().LoadRoom(roomId);

            var roomFwd = new ServerMessage(LibraryParser.OutgoingRequest("RoomForwardMessageComposer"));
            roomFwd.AppendInteger(roomId);

            var data = roomFwd.GetReversedBytes();

            foreach (var user in users.Where(user => user != null && user.GetClient() != null))
                user.GetClient().SendMessage(data);

            return true;
        }
    }
}