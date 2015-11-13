using System.Linq;
using Azure.Game.GameClients.Interfaces;
using Azure.Game.Items.Interactions.Models;
using Azure.Game.Items.Interfaces;
using Azure.Game.Pathfinding;
using Azure.Game.Rooms.User.Path;

namespace Azure.Game.Items.Interactions.Controllers
{
    internal class InteractorVendor : FurniInteractorModel
    {
        public override void OnPlace(GameClient session, RoomItem item)
        {
            item.ExtraData = "0";
            item.UpdateNeeded = true;

            if (item.InteractingUser > 0u)
            {
                var roomUserByHabbo = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(item.InteractingUser);

                if (roomUserByHabbo != null)
                    roomUserByHabbo.CanWalk = true;
            }
        }

        public override void OnRemove(GameClient session, RoomItem item)
        {
            item.ExtraData = "0";

            if (item.InteractingUser <= 0u)
                return;

            var roomUserByHabbo = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(item.InteractingUser);

            if (roomUserByHabbo != null)
                roomUserByHabbo.CanWalk = true;
        }

        public override void OnTrigger(GameClient session, RoomItem item, int request, bool hasRights)
        {
            if (item.ExtraData == "1" || !item.GetBaseItem().VendingIds.Any() || item.InteractingUser != 0u ||
                session == null)
                return;

            var roomUserByHabbo = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);

            if (roomUserByHabbo == null)
                return;

            if (!Gamemap.TilesTouching(roomUserByHabbo.X, roomUserByHabbo.Y, item.X, item.Y))
            {
                roomUserByHabbo.MoveTo(item.SquareInFront);
                return;
            }

            item.InteractingUser = session.GetHabbo().Id;
            roomUserByHabbo.CanWalk = false;
            roomUserByHabbo.ClearMovement();

            roomUserByHabbo.SetRot(PathFinder.CalculateRotation(roomUserByHabbo.X, roomUserByHabbo.Y, item.X, item.Y));

            item.ReqUpdate(2, true);
            item.ExtraData = "1";
            item.UpdateState(false, true);
        }
    }
}