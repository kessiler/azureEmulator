using System.Collections.Generic;
using System.Linq;
using Azure.HabboHotel.Items.Interactions.Enums;
using Azure.HabboHotel.Items.Interfaces;
using Azure.HabboHotel.Items.Wired.Interfaces;
using Azure.HabboHotel.Rooms;

namespace Azure.HabboHotel.Items.Wired.Handlers.Conditions
{
    internal class FurniHasFurni : IWiredItem
    {
        public FurniHasFurni(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            Items = new List<RoomItem>();
        }

        public Interaction Type => Interaction.ConditionFurniHasFurni;

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

        public bool OtherBool { get; set; }

        public int Delay
        {
            get { return 0; }
            set { }
        }

        public bool Execute(params object[] stuff)
        {
            if (!Items.Any())
                return true;

            foreach (var current in Items.Where(current => current != null && Room.GetRoomItemHandler().FloorItems.ContainsKey(current.Id)))
            {
                var toContinue = false;

                foreach (var current2 in current.AffectedTiles.Values.Where(current2 => Room.GetGameMap().SquareHasFurni(current2.X, current2.Y)))
                    toContinue = Room.GetGameMap().GetRoomItemForSquare(current2.X, current2.Y).Any(current3 => current3.Id != current.Id && current3.Z >= current2.Z);

                if (toContinue)
                    continue;

                if (Room.GetGameMap().GetRoomItemForSquare(current.X, current.Y).Any(current4 => current4.Id != current.Id && current4.Z >= current.Z))
                    continue;

                return false;
            }

            return true;
        }
    }
}