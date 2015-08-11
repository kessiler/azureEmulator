#region

using Azure.HabboHotel.GameClients;
using Azure.HabboHotel.Rooms;
using Azure.HabboHotel.Rooms.Games;

#endregion

namespace Azure.HabboHotel.Items.Interactor
{
    internal class InteractorFreezeTile : IFurniInteractor
    {
        public void OnPlace(GameClient session, RoomItem item)
        {
        }

        public void OnRemove(GameClient session, RoomItem item)
        {
        }

        public void OnTrigger(GameClient session, RoomItem item, int request, bool hasRights)
        {
            if (session == null || session.GetHabbo() == null || item.InteractingUser > 0U)
            {
                return;
            }
            string pName = session.GetHabbo().UserName;
            RoomUser roomUserByHabbo = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(pName);
            roomUserByHabbo.GoalX = item.X;
            roomUserByHabbo.GoalY = item.Y;
            if (roomUserByHabbo.Team != Team.none)
            {
                roomUserByHabbo.ThrowBallAtGoal = true;
            }
        }

        public void OnWiredTrigger(RoomItem item)
        {
        }

        public void OnUserWalk(GameClient session, RoomItem item, RoomUser user)
        {
        }
    }
}