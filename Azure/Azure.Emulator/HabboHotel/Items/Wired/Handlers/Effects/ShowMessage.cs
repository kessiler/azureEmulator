using System.Collections.Generic;
using Azure.HabboHotel.Items.Interactions.Enums;
using Azure.HabboHotel.Items.Interfaces;
using Azure.HabboHotel.Rooms.User;

namespace Azure.HabboHotel.Rooms.Wired.Handlers.Effects
{
    public class ShowMessage : IWiredItem
    {
        private readonly List<Interaction> _mBanned;

        public ShowMessage(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            OtherString = string.Empty;
            _mBanned = new List<Interaction>
            {
                Interaction.TriggerRepeater,
                Interaction.TriggerLongRepeater
            };
        }

        public Interaction Type => Interaction.ActionShowMessage;

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
            if (stuff[0] == null) return false;
            var roomUser = (RoomUser)stuff[0];
            var item = (Interaction)stuff[1];
            if (_mBanned.Contains(item)) return false;
            if (roomUser != null && roomUser.GetClient() != null && !string.IsNullOrEmpty(OtherString))
                roomUser.GetClient().SendWhisper(OtherString, true);
            return true;
        }
    }
}