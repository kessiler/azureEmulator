using Azure.Game.GameClients.Interfaces;
using Azure.Game.Items.Interactions.Interfaces;
using Azure.Game.Items.Interfaces;
using Azure.Game.Rooms.User;

namespace Azure.Game.Items.Interactions.Models
{
    internal class FurniInteractorModel : IFurniInteractor
    {
        public virtual void OnPlace(GameClient session, RoomItem item)
        {
        }

        public virtual void OnRemove(GameClient session, RoomItem item)
        {
        }

        public virtual void OnTrigger(GameClient session, RoomItem item, int request, bool hasRights)
        {
        }

        public virtual void OnUserWalk(GameClient session, RoomItem item, RoomUser user)
        {
        }

        public virtual void OnWiredTrigger(RoomItem item)
        {
        }
    }
}