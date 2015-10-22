using System.Collections.Generic;
using System.Linq;
using Azure.HabboHotel.Items.Interactions.Enums;
using Azure.HabboHotel.Items.Interfaces;
using Azure.HabboHotel.Rooms.User;

namespace Azure.HabboHotel.Rooms.Wired.Handlers.Conditions
{
    internal class TriggererOnFurni : IWiredItem
    {
        public TriggererOnFurni(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            Items = new List<RoomItem>();
        }

        public Interaction Type => Interaction.ConditionTriggerOnFurni;

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public List<RoomItem> Items { get; set; }

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

        public int Delay
        {
            get { return 0; }
            set { }
        }

        public bool Execute(params object[] stuff)
        {
            if (!Items.Any())
                return true;
            if (stuff == null || !(stuff[0] is RoomUser))
                return false;

            var roomUser = (RoomUser)stuff[0];
            if (roomUser == null)
                return false;

            foreach (
                var current in
                    Items.Where(
                        current => current != null && Room.GetRoomItemHandler().FloorItems.ContainsKey(current.Id)))
            {
                if (current.AffectedTiles.Values.Any(current2 => roomUser.X == current2.X && roomUser.Y == current2.Y))
                    return true;
                if (roomUser.X == current.X && roomUser.Y == current.Y)
                    return true;
            }
            return false;
        }
    }
}