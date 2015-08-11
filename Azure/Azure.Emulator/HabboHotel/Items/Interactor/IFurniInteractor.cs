#region

using Azure.HabboHotel.GameClients;
using Azure.HabboHotel.Rooms;

#endregion

namespace Azure.HabboHotel.Items.Interactor
{
    internal interface IFurniInteractor
    {
        void OnPlace(GameClient session, RoomItem item);

        void OnRemove(GameClient session, RoomItem item);

        void OnTrigger(GameClient session, RoomItem item, int request, bool hasRights);

        void OnUserWalk(GameClient session, RoomItem item, RoomUser user);

        void OnWiredTrigger(RoomItem item);
    }
}