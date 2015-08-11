#region

using Azure.HabboHotel.GameClients;
using Azure.HabboHotel.Rooms;

#endregion

namespace Azure.HabboHotel.Items.Interactor
{
    internal class InteractorJukebox : IFurniInteractor
    {
        public void OnPlace(GameClient session, RoomItem item)
        {
        }

        public void OnRemove(GameClient session, RoomItem item)
        {
        }

        public void OnTrigger(GameClient session, RoomItem item, int request, bool hasRights)
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

        public void OnUserWalk(GameClient session, RoomItem item, RoomUser user)
        {
        }

        public void OnWiredTrigger(RoomItem item)
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