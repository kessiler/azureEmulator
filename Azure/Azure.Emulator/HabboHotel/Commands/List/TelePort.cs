#region

using Azure.HabboHotel.GameClients;

#endregion

namespace Azure.HabboHotel.Commands.List
{
    /// <summary>
    /// Class TelePort. This class cannot be inherited.
    /// </summary>
    internal sealed class TelePort : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TelePort"/> class.
        /// </summary>
        public TelePort()
        {
            MinRank = 7;
            Description = "Teleport around the room, like a kingorooo.";
            Usage = ":teleport";
            MinParams = 0;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var room = session.GetHabbo().CurrentRoom;
            var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user == null) return true;
            if (!user.RidingHorse)
            {
                user.TeleportEnabled = !user.TeleportEnabled;
                room.GetGameMap().GenerateMaps(true);
                return true;
            }
            session.SendWhisper(Azure.GetLanguage().GetVar("command_error_teleport_enable"));
            return true;
        }
    }
}