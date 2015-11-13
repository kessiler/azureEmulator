using System;
using Azure.Game.GameClients.Interfaces;
using Azure.Game.Items.Interactions.Models;
using Azure.Game.Items.Interfaces;
using Azure.Game.Rooms.User;
using Azure.Game.Rooms.User.Path;

namespace Azure.Game.Items.Interactions.Controllers
{
    internal class InteractorCrackableEgg : FurniInteractorModel
    {
        public override void OnTrigger(GameClient session, RoomItem item, int request, bool hasRights)
        {
            RoomUser roomUser = null;
            if (session != null)
                roomUser = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);

            if (roomUser == null)
                return;

            if (Gamemap.TilesTouching(item.X, item.Y, roomUser.X, roomUser.Y))
            {
                var cracks = 0;

                if (Azure.IsNum(item.ExtraData))
                    cracks = Convert.ToInt16(item.ExtraData);

                cracks++;
                item.ExtraData = Convert.ToString(cracks);
                item.UpdateState(false, true);
                return;
            }

            roomUser.MoveTo(item.SquareInFront);
        }
    }
}