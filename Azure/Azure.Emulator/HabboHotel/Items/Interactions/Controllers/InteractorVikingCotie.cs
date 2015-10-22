using System.Timers;
using Azure.HabboHotel.GameClients.Interfaces;
using Azure.HabboHotel.Items.Interactions.Models;
using Azure.HabboHotel.Items.Interfaces;

namespace Azure.HabboHotel.Items.Interactions.Controllers
{
    internal class InteractorVikingCotie : FurniInteractorModel
    {
        private RoomItem _mItem;

        public override void OnTrigger(GameClient session, RoomItem item, int request, bool hasRights)
        {
            var user = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);

            if (user == null)
                return;

            if (user.CurrentEffect != 172 && user.CurrentEffect != 5 && user.CurrentEffect != 173)
                return;

            if (item.ExtraData != "5")
            {
                if (item.VikingCotieBurning)
                    return;

                item.ExtraData = "1";
                item.UpdateState();

                item.VikingCotieBurning = true;

                var clientByUsername =
                    Azure.GetGame().GetClientManager().GetClientByUserName(item.GetRoom().RoomData.Owner);

                if (clientByUsername != null)
                {
                    if (clientByUsername.GetHabbo().UserName != item.GetRoom().RoomData.Owner)
                        clientByUsername.SendNotif(string.Format(Azure.GetLanguage().GetVar("viking_burn_started"),
                            user.GetUserName()));
                }

                _mItem = item;

                var timer = new Timer(5000);
                timer.Elapsed += OnElapse;
                timer.Enabled = true;
            }
            else
                session.SendNotif(Azure.GetLanguage().GetVar("user_viking_error"));
        }

        private void OnElapse(object sender, ElapsedEventArgs e)
        {
            if (_mItem == null)
                return;

            switch (_mItem.ExtraData)
            {
                case "1":
                    _mItem.ExtraData = "2";
                    _mItem.UpdateState();
                    return;

                case "2":
                    _mItem.ExtraData = "3";
                    _mItem.UpdateState();
                    return;

                case "3":
                    _mItem.ExtraData = "4";
                    _mItem.UpdateState();
                    return;

                case "4":
                    ((Timer)sender).Stop();
                    _mItem.ExtraData = "5";
                    _mItem.UpdateState();
                    return;
            }
        }
    }
}