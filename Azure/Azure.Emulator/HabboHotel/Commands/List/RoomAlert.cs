#region

using System.Linq;
using Azure.HabboHotel.GameClients;

#endregion

namespace Azure.HabboHotel.Commands.List
{
    internal sealed class RoomAlert : Command
    {
        public RoomAlert()
        {
            MinRank = 5;
            Description = "Alerts the Room.";
            Usage = ":roomalert [MESSAGE]";
            MinParams = -1;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var alert = string.Join(" ", pms);
            foreach (
                var user in
                    session.GetHabbo()
                        .CurrentRoom.GetRoomUserManager()
                        .GetRoomUsers()
                        .Where(user => !user.IsBot && user.GetClient() != null))
                user.GetClient().SendNotif(alert);
            return true;
        }
    }
}