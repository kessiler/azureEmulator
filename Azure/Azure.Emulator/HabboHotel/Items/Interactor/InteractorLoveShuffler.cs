#region

using Azure.HabboHotel.GameClients;
using Azure.HabboHotel.Rooms;

#endregion

namespace Azure.HabboHotel.Items.Interactor
{
    internal class InteractorLoveShuffler : IFurniInteractor
    {
        public void OnPlace(GameClient session, RoomItem item)
        {
            item.ExtraData = "-1";
            item.UpdateNeeded = true;
        }

        public void OnRemove(GameClient session, RoomItem item)
        {
            item.ExtraData = "-1";
        }

        public void OnTrigger(GameClient session, RoomItem item, int request, bool hasRights)
        {
            if (!hasRights)
            {
                return;
            }
            if (item.ExtraData == "0")
            {
                return;
            }
            item.ExtraData = "0";
            item.UpdateState(false, true);
            item.ReqUpdate(10, true);
        }

        public void OnUserWalk(GameClient session, RoomItem item, RoomUser user)
        {
        }

        public void OnWiredTrigger(RoomItem item)
        {
            if (item.ExtraData == "0")
            {
                return;
            }
            item.ExtraData = "0";
            item.UpdateState(false, true);
            item.ReqUpdate(10, true);
        }
    }
}