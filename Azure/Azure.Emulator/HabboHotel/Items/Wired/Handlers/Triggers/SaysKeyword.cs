using System;
using System.Collections.Generic;
using System.Linq;
using Azure.HabboHotel.Items.Interactions.Enums;
using Azure.HabboHotel.Items.Interfaces;
using Azure.HabboHotel.Items.Wired;
using Azure.HabboHotel.Rooms.User;

namespace Azure.HabboHotel.Rooms.Wired.Handlers.Triggers
{
    public class SaysKeyword : IWiredItem
    {
        public SaysKeyword(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            OtherString = string.Empty;
            OtherBool = false;
        }

        public Interaction Type => Interaction.TriggerOnUserSay;

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

        public bool OtherBool { get; set; }

        public bool Execute(params object[] stuff)
        {
            var roomUser = (RoomUser)stuff[0];
            var text = (string)stuff[1];
            if (string.IsNullOrEmpty(OtherString)) return false;
            if (!string.Equals(text, OtherString, StringComparison.CurrentCultureIgnoreCase)) return false;
            var conditions = Room.GetWiredHandler().GetConditions(this);
            var effects = Room.GetWiredHandler().GetEffects(this);
            if (conditions.Any())
            {
                foreach (var current in conditions)
                {
                    WiredHandler.OnEvent(current);
                    if (!current.Execute(roomUser)) return true;
                }
            }

            roomUser.GetClient().SendWhisper(text);
            if (effects.Any())
                foreach (var current2 in effects.Where(current2 => current2.Execute(roomUser, Type)))
                    WiredHandler.OnEvent(current2);

            WiredHandler.OnEvent(this);
            return true;
        }
    }
}