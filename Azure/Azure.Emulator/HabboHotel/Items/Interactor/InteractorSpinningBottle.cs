#region

using Azure.HabboHotel.GameClients;
using Azure.HabboHotel.Rooms;

#endregion

namespace Azure.HabboHotel.Items.Interactor
{
    internal class InteractorSpinningBottle : IFurniInteractor
    {
        public void OnPlace(GameClient session, RoomItem item)
        {
            item.ExtraData = "0";
            item.UpdateState(true, false);
        }

        public void OnRemove(GameClient session, RoomItem item)
        {
            item.ExtraData = "0";
        }

        public void OnTrigger(GameClient session, RoomItem item, int request, bool hasRights)
        {
            if (item.ExtraData == "-1")
            {
                return;
            }
            item.ExtraData = "-1";
            item.UpdateState(false, true);
            item.ReqUpdate(3, true);
        }

        public void OnUserWalk(GameClient session, RoomItem item, RoomUser user)
        {
        }

        public void OnWiredTrigger(RoomItem item)
        {
            if (item.ExtraData == "-1")
            {
                return;
            }
            item.ExtraData = "-1";
            item.UpdateState(false, true);
            item.ReqUpdate(3, true);
        }
    }
}