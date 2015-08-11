#region

using Azure.Database.Manager.Database.Session_Details.Interfaces;
using Azure.HabboHotel.GameClients;
using Azure.HabboHotel.Rooms;

#endregion

namespace Azure.HabboHotel.Items.Interactor
{
    internal class InteractorHopper : IFurniInteractor
    {
        public void OnPlace(GameClient session, RoomItem item)
        {
            {
                item.GetRoom().GetRoomItemHandler().HopperCount++;
                using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
                {
                    queryReactor.SetQuery("INSERT INTO items_hopper (hopper_id, room_id) VALUES (@hopperid, @roomid);");
                    queryReactor.AddParameter("hopperid", item.Id);
                    queryReactor.AddParameter("roomid", item.RoomId);
                    queryReactor.RunQuery();
                }
                if (item.InteractingUser == 0u)
                {
                    return;
                }
                var roomUserByHabbo = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(item.InteractingUser);
                if (roomUserByHabbo != null)
                {
                    roomUserByHabbo.ClearMovement();
                    roomUserByHabbo.AllowOverride = false;
                    roomUserByHabbo.CanWalk = true;
                }
                item.InteractingUser = 0u;
            }
        }

        public void OnRemove(GameClient session, RoomItem item)
        {
            {
                item.GetRoom().GetRoomItemHandler().HopperCount--;
                using (IQueryAdapter queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
                {
                    queryReactor.SetQuery(string.Format("DELETE FROM items_hopper WHERE item_id=@hid OR room_id={0} LIMIT 1", item.GetRoom().RoomId));
                    queryReactor.AddParameter("hid", item.Id);
                    queryReactor.RunQuery();
                }
                if (item.InteractingUser == 0u)
                {
                    return;
                }
                RoomUser roomUserByHabbo = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(item.InteractingUser);
                if (roomUserByHabbo != null)
                {
                    roomUserByHabbo.UnlockWalking();
                }
                item.InteractingUser = 0u;
            }
        }

        public void OnTrigger(GameClient session, RoomItem item, int request, bool hasRights)
        {
            if (item == null || item.GetRoom() == null || session == null || session.GetHabbo() == null)
            {
                return;
            }
            RoomUser roomUserByHabbo = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (roomUserByHabbo == null)
            {
                return;
            }
            if (!(roomUserByHabbo.Coordinate == item.Coordinate) && !(roomUserByHabbo.Coordinate == item.SquareInFront))
            {
                if (roomUserByHabbo.CanWalk)
                {
                    roomUserByHabbo.MoveTo(item.SquareInFront);
                }
                return;
            }
            if (item.InteractingUser != 0u)
            {
                return;
            }
            roomUserByHabbo.TeleDelay = 2;
            item.InteractingUser = roomUserByHabbo.GetClient().GetHabbo().Id;
        }

        public void OnUserWalk(GameClient session, RoomItem item, RoomUser user)
        {
        }

        public void OnWiredTrigger(RoomItem item)
        {
        }
    }
}