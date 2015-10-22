using System.Collections.Generic;
using Azure.HabboHotel.Items.Interactions.Enums;
using Azure.HabboHotel.Items.Interfaces;
using Azure.HabboHotel.Rooms.User;

namespace Azure.HabboHotel.Rooms.Wired.Handlers.Conditions
{
    internal class IsGroupMember : IWiredItem
    {
        public IsGroupMember(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            Items = new List<RoomItem>();
        }

        public Interaction Type => Interaction.ConditionGroupMember;

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
            if (stuff == null || !(stuff[0] is RoomUser))
                return false;

            var roomUser = (RoomUser)stuff[0];
            if (roomUser == null)
                return false;

            return Room.RoomData.Group != null &&
                   Room.RoomData.Group.Members.ContainsKey(roomUser.GetClient().GetHabbo().Id);
        }
    }
}