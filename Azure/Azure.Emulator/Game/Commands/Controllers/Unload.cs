using System.Collections.Generic;
using System.Linq;
using Azure.Game.Commands.Interfaces;
using Azure.Game.GameClients.Interfaces;
using Azure.Game.Rooms.User;
using Azure.Messages;
using Azure.Messages.Parsers;

namespace Azure.Game.Commands.Controllers
{
    /// <summary>
    ///     Class Unload. This class cannot be inherited.
    /// </summary>
    internal sealed class Unload : Command
    {
        /// <summary>
        ///     The _re enter
        /// </summary>
        private readonly bool _reEnter;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Unload" /> class.
        /// </summary>
        /// <param name="reEnter">if set to <c>true</c> [re enter].</param>
        public Unload(bool reEnter = false)
        {
            MinRank = -1;
            Description = "Unloads the current room!";
            Usage = reEnter ? ":reload" : ":unload";
            MinParams = 0;
            _reEnter = reEnter;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var roomId = session.GetHabbo().CurrentRoom.RoomId;
            var users = new List<RoomUser>(session.GetHabbo().CurrentRoom.GetRoomUserManager().UserList.Values);

            Azure.GetGame().GetRoomManager().UnloadRoom(session.GetHabbo().CurrentRoom, "Unload command");

            if (!_reEnter)
                return true;
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