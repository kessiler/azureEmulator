#region

using Azure.HabboHotel.GameClients.Interfaces;
using Azure.HabboHotel.Items.Interactions.Models;
using Azure.HabboHotel.Items.Interfaces;

#endregion

namespace Azure.HabboHotel.Items.Interactions.Controllers
{
    internal class InteractorSpinningBottle : FurniInteractorModel
    {
        public override void OnPlace(GameClient session, RoomItem item)
        {
            item.ExtraData = "0";
            item.UpdateState(true, false);
        }

        public override void OnRemove(GameClient session, RoomItem item)
        {
            item.ExtraData = "0";
        }

        public override void OnTrigger(GameClient session, RoomItem item, int request, bool hasRights)
        {
            if (item.ExtraData == "-1")
                return;

            item.ExtraData = "-1";
            item.UpdateState(false, true);
            item.ReqUpdate(3, true);
        }

        public override void OnWiredTrigger(RoomItem item)
        {
            if (item.ExtraData == "-1")
                return;

            item.ExtraData = "-1";
            item.UpdateState(false, true);
            item.ReqUpdate(3, true);
        }
    }
}