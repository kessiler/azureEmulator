using Azure.Game.GameClients.Interfaces;
using Azure.Game.Items.Interactions.Models;
using Azure.Game.Items.Interfaces;

namespace Azure.Game.Items.Interactions.Controllers
{
    internal class InteractorRoller : FurniInteractorModel
    {
        public override void OnTrigger(GameClient session, RoomItem item, int request, bool hasRights)
        {
            item.GetRoom().GetRoomItemHandler().GotRollers = true;
        }
    }
}