#region

using Azure.HabboHotel.GameClients.Interfaces;
using Azure.HabboHotel.Items.Interactions.Models;
using Azure.HabboHotel.Items.Interfaces;
using Azure.HabboHotel.Rooms.Items.Games.Teams.Enums;

#endregion

namespace Azure.HabboHotel.Items.Interactions.Controllers
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