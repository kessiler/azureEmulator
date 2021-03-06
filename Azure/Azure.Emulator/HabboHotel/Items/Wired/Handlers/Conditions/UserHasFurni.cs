﻿using System.Collections.Generic;
using Azure.HabboHotel.Items.Interactions.Enums;
using Azure.HabboHotel.Items.Interfaces;
using Azure.HabboHotel.Items.Wired.Interfaces;
using Azure.HabboHotel.Rooms;
using Azure.HabboHotel.Rooms.User;

namespace Azure.HabboHotel.Items.Wired.Handlers.Conditions
{
    internal class UserHasFurni : IWiredItem
    {
        public UserHasFurni(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            Items = new List<RoomItem>();
            OtherString = string.Empty;
        }

        public Interaction Type => Interaction.ConditionUserHasFurni;

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public List<RoomItem> Items { get; set; }

        public string OtherString { get; set; }

        public string OtherExtraString
        {
            get { return string.Empty; }
            set { }
        }

        public string OtherExtraString2
        {
            get { return string.Empty; }
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
            var roomUser = stuff?[0] as RoomUser;

            if ((roomUser?.IsBot ?? true) || roomUser.GetClient() == null || roomUser.GetClient().GetHabbo() == null || roomUser.GetClient().GetHabbo().GetInventoryComponent() == null || string.IsNullOrEmpty(OtherString))
                return false;

            var itemsIdsArray = OtherString.Split(';');

            foreach (var itemIdStr in itemsIdsArray)
            {
                uint itemId;

                if (!uint.TryParse(itemIdStr, out itemId))
                    continue;

                if (roomUser.GetClient().GetHabbo().GetInventoryComponent().HasBaseItem(itemId))
                    return true;
            }

            return false;
        }
    }
}