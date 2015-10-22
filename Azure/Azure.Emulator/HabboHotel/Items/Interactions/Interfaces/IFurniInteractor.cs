using Azure.HabboHotel.GameClients.Interfaces;
using Azure.HabboHotel.Items.Interfaces;
using Azure.HabboHotel.Rooms.User;

namespace Azure.HabboHotel.Items.Interactions.Interfaces
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