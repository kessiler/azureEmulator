using Azure.HabboHotel.GameClients.Interfaces;
using Azure.HabboHotel.Items.Interactions.Enums;
using Azure.HabboHotel.Items.Interactions.Models;
using Azure.HabboHotel.Items.Interfaces;
using Azure.HabboHotel.Rooms.User;
using Azure.HabboHotel.Rooms.User.Path;

namespace Azure.HabboHotel.Items.Interactions.Controllers
{
    internal class InteractorSwitch : FurniInteractorModel
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
                var num = item.GetBaseItem().Modes - 1;
                int num2, num3;
                int.TryParse(item.ExtraData, out num2);

                if (num2 <= 0)
                    num3 = 1;
                else
                {
                    if (num2 >= num)
                        num3 = 0;
                    else
                        num3 = num2 + 1;
                }

                item.ExtraData = num3.ToString();
                item.UpdateState();
                item.GetRoom().GetWiredHandler().ExecuteWired(Interaction.TriggerStateChanged, roomUser, item);

                return;
            }

            roomUser.MoveTo(item.SquareInFront);
        }
    }
}