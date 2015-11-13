using System.Collections.Generic;
using System.Linq;
using Azure.Game.Items.Interactions.Enums;
using Azure.Game.Items.Interfaces;
using Azure.Game.Items.Wired.Interfaces;
using Azure.Game.Rooms;
using Azure.Game.Rooms.User;

namespace Azure.Game.Items.Wired.Handlers.Triggers
{
    public class UserEntersRoom : IWiredItem
    {
        public UserEntersRoom(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            OtherString = string.Empty;
        }

        public Interaction Type => Interaction.TriggerRoomEnter;

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public List<RoomItem> Items
        {
            get { return new List<RoomItem>(); }
            set { }
        }

        public int Delay
        {
            get { return 0; }
            set { }
        }

        public string OtherString { get; set; }

        public string OtherExtraString
        {
            get { return ""; }
            set { }
        }

        public string OtherExtraString2
        {
            get { return ""; }
            set { }
        }

        public bool OtherBool
        {
            get { return true; }
            set { }
        }

        public bool Execute(params object[] stuff)
        {
            var roomUser = (RoomUser)stuff[0];

            if (!string.IsNullOrEmpty(OtherString) && roomUser.GetUserName() != OtherString && !roomUser.GetClient().GetHabbo().IsTeleporting)
                return false;

            var conditions = Room.GetWiredHandler().GetConditions(this);
            var effects = Room.GetWiredHandler().GetEffects(this);

            if (conditions.Any())
            {
                foreach (var current in conditions)
                {
                    if (!current.Execute(roomUser))
                        return false;

                    WiredHandler.OnEvent(current);
                }
            }

            if (effects.Any())
            {
                foreach (var current2 in effects.Where(current2 => current2.Execute(roomUser, Type)))
                    WiredHandler.OnEvent(current2);
            }

            WiredHandler.OnEvent(this);
            return true;
        }
    }
}