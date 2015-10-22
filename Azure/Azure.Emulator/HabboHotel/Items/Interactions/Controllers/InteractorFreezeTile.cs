using Azure.HabboHotel.GameClients.Interfaces;
using Azure.HabboHotel.Items.Interactions.Models;
using Azure.HabboHotel.Items.Interfaces;
using Azure.HabboHotel.Rooms.Items.Games.Teams.Enums;

namespace Azure.HabboHotel.Items.Interactions.Controllers
{
    internal class InteractorFreezeTile : FurniInteractorModel
    {
        public override void OnTrigger(GameClient session, RoomItem item, int request, bool hasRights)
        {
            if (session == null || session.GetHabbo() == null || item.InteractingUser > 0U)
                return;

            var pName = session.GetHabbo().UserName;
            var roomUserByHabbo = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(pName);
            roomUserByHabbo.GoalX = item.X;
            roomUserByHabbo.GoalY = item.Y;

            if (roomUserByHabbo.Team != Team.None)
                roomUserByHabbo.ThrowBallAtGoal = true;
        }
    }
}