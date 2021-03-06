using System.Collections.Generic;
using System.Linq;
using Azure.HabboHotel.Items.Interactions.Enums;
using Azure.HabboHotel.Items.Interfaces;
using Azure.HabboHotel.Items.Wired.Interfaces;
using Azure.HabboHotel.Rooms;

namespace Azure.HabboHotel.Items.Wired.Handlers.Triggers
{
    public class BotReachedAvatar : IWiredItem
    {
        public BotReachedAvatar(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
        }

        public Interaction Type => Interaction.TriggerBotReachedAvatar;

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

        public string OtherString
        {
            get { return ""; }
            set { }
        }

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
            var conditions = Room.GetWiredHandler().GetConditions(this);
            var effects = Room.GetWiredHandler().GetEffects(this);

            if (conditions.Any())
            {
                foreach (var current in conditions)
                {
                    if (!current.Execute(null))
                        return false;

                    WiredHandler.OnEvent(current);
                }
            }

            if (effects.Any())
            {
                foreach (var current2 in effects)
                {
                    foreach (var current3 in Room.GetRoomUserManager().UserList.Values)
                        current2.Execute(current3, Type);

                    WiredHandler.OnEvent(current2);
                }
            }

            WiredHandler.OnEvent(this);
            return true;
        }
    }
}