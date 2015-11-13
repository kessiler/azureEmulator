using Azure.Game.GameClients.Interfaces;
using Azure.Game.Items.Interactions.Models;
using Azure.Game.Items.Interfaces;
using Azure.Game.Rooms.Items.Games.Teams.Enums;

namespace Azure.Game.Items.Interactions.Controllers
{
    internal class InteractorBanzaiScoreCounter : FurniInteractorModel
    {
        public override void OnPlace(GameClient session, RoomItem item)
        {
            if (item.Team == Team.None)
                return;

            item.ExtraData = item.GetRoom().GetGameManager().Points[(int)item.Team].ToString();
            item.UpdateState(false, true);
        }

        public override void OnTrigger(GameClient session, RoomItem item, int request, bool hasRights)
        {
            if (!hasRights)
                return;

            item.GetRoom().GetGameManager().Points[(int)item.Team] = 0;
            item.ExtraData = "0";
            item.UpdateState();
        }
    }
}