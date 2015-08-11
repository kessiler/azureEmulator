#region

using Azure.HabboHotel.GameClients;
using Azure.HabboHotel.Rooms;

#endregion

namespace Azure.HabboHotel.Items.Interactor
{
    internal class InteractorOneWayGate : IFurniInteractor
    {
        public void OnPlace(GameClient session, RoomItem item)
        {
            item.ExtraData = "0";

            if (item.InteractingUser != 0)
            {
                var User = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(item.InteractingUser);

                if (User != null)
                {
                    User.ClearMovement();
                    User.UnlockWalking();
                }

                item.InteractingUser = 0;
            }
        }

        public void OnRemove(GameClient session, RoomItem item)
        {
            item.ExtraData = "0";

            if (item.InteractingUser != 0)
            {
                var User = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(item.InteractingUser);

                if (User != null)
                {
                    User.ClearMovement();
                    User.UnlockWalking();
                }

                item.InteractingUser = 0;
            }
        }

        public void OnTrigger(GameClient session, RoomItem item, int request, bool hasRights)
        {
            if (session == null)
                return;
            var User = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);

            if (User == null)
            {
                return;
            }

            if (User.Coordinate != item.SquareInFront && User.CanWalk)
            {
                User.MoveTo(item.SquareInFront);
                return;
            }

            if (!item.GetRoom().GetGameMap().CanWalk(item.SquareBehind.X, item.SquareBehind.Y, User.AllowOverride))
            {
                return;
            }

            if (item.InteractingUser == 0)
            {
                item.InteractingUser = User.HabboId;

                User.CanWalk = false;

                if (User.IsWalking && (User.GoalX != item.SquareInFront.X || User.GoalY != item.SquareInFront.Y))
                {
                    User.ClearMovement();
                }

                User.AllowOverride = true;
                User.MoveTo(item.Coordinate);

                item.ReqUpdate(4, true);
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