#region

using System.Collections.Generic;
using System.Linq;
using Azure.HabboHotel.Items;

#endregion

namespace Azure.HabboHotel.Rooms.Wired.Handlers.Triggers
{
    public class TemplateTrigger : IWiredItem
    {
        public TemplateTrigger(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            OtherString = string.Empty;
            Delay = 0;
            OtherBool = true;
            OtherExtraString = string.Empty;
            OtherExtraString2 = string.Empty;
        }

        public Interaction Type
        {
            get
            {
                return Interaction.TriggerRoomEnter;
            }
        }

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public List<RoomItem> Items
        {
            get
            {
                return new List<RoomItem>();
            }
            set
            {
            }
        }

        public int Delay { get; set; }

        public string OtherString { get; set; }

        public string OtherExtraString { get; set; }

        public string OtherExtraString2 { get; set; }

        public bool OtherBool { get; set; }

        public bool Execute(params object[] stuff)
        {
            var roomUser = (RoomUser)stuff[0];
            if (roomUser == null)
            {
                return false;
            }

            List<IWiredItem> conditions = Room.GetWiredHandler().GetConditions(this);
            List<IWiredItem> effects = Room.GetWiredHandler().GetEffects(this);
            if (conditions.Any())
            {
                foreach (IWiredItem current in conditions)
                {
                    if (!current.Execute(roomUser))
                    {
                        return false;
                    }
                    WiredHandler.OnEvent(current);
                }
            }
            if (effects.Any())
            {
                foreach (IWiredItem current2 in effects.Where(current2 => current2.Execute(roomUser, Type)))
                {
                    WiredHandler.OnEvent(current2);
                }
            }
            WiredHandler.OnEvent(this);
            return true;
        }
    }
}