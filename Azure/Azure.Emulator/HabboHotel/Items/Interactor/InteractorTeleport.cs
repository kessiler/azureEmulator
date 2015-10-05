#region

using Azure.HabboHotel.GameClients;
using Azure.HabboHotel.Rooms;

#endregion

namespace Azure.HabboHotel.Items.Interactor
{
    internal class InteractorTeleport : IFurniInteractor
    {
        public void OnPlace(GameClient session, RoomItem item)
        {
            item.ExtraData = "0";
            if (item.InteractingUser != 0)
            {
                RoomUser user = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(item.InteractingUser);
                if (user != null)
                {
                    user.ClearMovement();
                    user.AllowOverride = false;
                    user.CanWalk = true;
                }
                item.InteractingUser = 0;
            }
            if (item.InteractingUser2 != 0)
            {
                RoomUser user = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(item.InteractingUser2);
                if (user != null)
                {
                    user.ClearMovement();
                    user.AllowOverride = false;
                    user.CanWalk = true;
                }
                item.InteractingUser2 = 0;
            }
        }

        public void OnRemove(GameClient session, RoomItem item)
        {
            item.ExtraData = "0";
            if (item.InteractingUser != 0)
            {
                RoomUser user = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(item.InteractingUser);
                if (user != null)
                {
                    user.UnlockWalking();
                }
                item.InteractingUser = 0;
            }
            if (item.InteractingUser2 != 0)
            {
                RoomUser user = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(item.InteractingUser2);
                if (user != null)
                {
                    user.UnlockWalking();
                }
                item.InteractingUser2 = 0;
            }
        }

        public void OnTrigger(GameClient session, RoomItem item, int request, bool hasRights)
        {
            if (item == null || item.GetRoom() == null || session == null || session.GetHabbo() == null)
                return;

            RoomUser user = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user != null)
            {
                if (user.Coordinate == item.Coordinate || user.Coordinate == item.SquareInFront)
                {
                    if (item.InteractingUser != 0)
                    {
                        return;
                    }
                    item.InteractingUser = user.GetClient().GetHabbo().Id;
                }
                else if (user.CanWalk)
                {
                    user.MoveTo(item.SquareInFront);
                }
            }
        }

        public void OnUserWalk(GameClient session, RoomItem item, RoomUser user)
        {
        }

        public void OnWiredTrigger(RoomItem item)
        {
        }
    }
}