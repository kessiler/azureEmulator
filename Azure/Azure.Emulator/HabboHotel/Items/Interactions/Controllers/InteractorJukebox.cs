using Azure.HabboHotel.GameClients.Interfaces;
using Azure.HabboHotel.Items.Interactions.Models;
using Azure.HabboHotel.Items.Interfaces;

namespace Azure.HabboHotel.Items.Interactions.Controllers
{
    internal class InteractorJukebox : FurniInteractorModel
    {
        public override void OnTrigger(GameClient session, RoomItem item, int request, bool hasRights)
        {
            if (!hasRights)
                return;

            if (item.ExtraData == "1")
            {
                item.GetRoom().GetRoomMusicController().Stop();
                item.ExtraData = "0";
            }
            else
            {
                item.GetRoom().GetRoomMusicController().Start();
                item.ExtraData = "1";
            }

            item.UpdateState();
        }

        public override void OnWiredTrigger(RoomItem item)
        {
            if (item.ExtraData == "1")
            {
                item.GetRoom().GetRoomMusicController().Stop();
                item.ExtraData = "0";
            }
            else
            {
                item.GetRoom().GetRoomMusicController().Start();
                item.ExtraData = "1";
            }

            item.UpdateState();
        }
    }
}