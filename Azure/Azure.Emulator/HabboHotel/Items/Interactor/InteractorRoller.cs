#region

using Azure.HabboHotel.GameClients;
using Azure.HabboHotel.Rooms;

#endregion

namespace Azure.HabboHotel.Items.Interactor
{
    internal class InteractorRoller : IFurniInteractor
    {

        public void OnPlace(GameClient session, RoomItem item) { }
        public void OnRemove(GameClient session, RoomItem item) { }

        public void OnTrigger(GameClient session, RoomItem item, int request, bool hasRights)
        {
            item.GetRoom().GetRoomItemHandler().GotRollers = true;
        }

        public void OnUserWalk(GameClient session, RoomItem item, Rooms.RoomUser user) { }
        public void OnWiredTrigger(RoomItem item) { }
    }
}
