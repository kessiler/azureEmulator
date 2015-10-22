using Azure.HabboHotel.GameClients.Interfaces;
using Azure.HabboHotel.Items.Interactions.Models;
using Azure.HabboHotel.Items.Interfaces;

namespace Azure.HabboHotel.Items.Interactions.Controllers
{
    internal class InteractorRoller : FurniInteractorModel
    {
        public override void OnTrigger(GameClient session, RoomItem item, int request, bool hasRights)
        {
            item.GetRoom().GetRoomItemHandler().GotRollers = true;
        }
    }
}