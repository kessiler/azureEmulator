using System;
using System.Threading;
using Azure.HabboHotel.GameClients.Interfaces;
using Azure.HabboHotel.Items.Interactions.Models;
using Azure.HabboHotel.Items.Interfaces;
using Azure.HabboHotel.PathFinding;
using Azure.HabboHotel.Rooms;
using Azure.HabboHotel.Rooms.User;

namespace Azure.HabboHotel.Items.Interactions.Controllers
{
    internal class InteractorFxBox : FurniInteractorModel
    {
        public override void OnTrigger(GameClient session, RoomItem item, int request, bool hasRights)
        {
            if (!hasRights)
                return;

            RoomUser user = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);

            if (user == null)
                return;

            Room room = session.GetHabbo().CurrentRoom;

            if (room == null)
                return;

            int effectId = Convert.ToInt32(item.GetBaseItem().Name.Replace("fxbox_fx", ""));

            try
            {
                while (PathFinder.GetDistance(user.X, user.Y, item.X, item.Y) > 1)
                {
                    if (user.RotBody == 0)
                        user.MoveTo(item.X, item.Y + 1);
                    else if (user.RotBody == 2)
                        user.MoveTo(item.X - 1, item.Y);
                    else if (user.RotBody == 4)
                        user.MoveTo(item.X, item.Y - 1);
                    else if(user.RotBody == 6)
                        user.MoveTo(item.X + 1, item.Y);
                    else
                        user.MoveTo(item.X, item.Y + 1); // Diagonal user...
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                if (PathFinder.GetDistance(user.X, user.Y, item.X, item.Y) == 1)
                {
                    session.GetHabbo().GetAvatarEffectsInventoryComponent().AddNewEffect(effectId, -1, 0);
                    session.GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(effectId);

                    Thread.Sleep(500); //Wait 0.5 second until remove furniture. (Delay)

                    room.GetRoomItemHandler().RemoveFurniture(session, item.Id, false);

                    using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
                        queryReactor.RunFastQuery("DELETE FROM items_rooms WHERE id = " + item.Id);
                }
            }
        }
    }
}