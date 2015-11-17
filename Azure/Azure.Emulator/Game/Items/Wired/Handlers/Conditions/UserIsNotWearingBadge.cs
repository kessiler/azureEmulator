﻿using System;
using System.Collections.Generic;
using System.Linq;
using Azure.Game.Items.Interactions.Enums;
using Azure.Game.Items.Interfaces;
using Azure.Game.Items.Wired.Interfaces;
using Azure.Game.Rooms;
using Azure.Game.Rooms.User;
using Azure.Game.Users.Badges;
using Azure.Game.Users.Badges.Models;

namespace Azure.Game.Items.Wired.Handlers.Conditions
{
    internal class UserIsNotWearingBadge : IWiredItem
    {
        public UserIsNotWearingBadge(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            Items = new List<RoomItem>();
            OtherString = string.Empty;
        }

        public Interaction Type => Interaction.ConditionUserNotWearingBadge;

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public List<RoomItem> Items { get; set; }

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

        public int Delay
        {
            get { return 0; }
            set { }
        }

        public bool Execute(params object[] stuff)
        {
            var roomUser = stuff?[0] as RoomUser;

            if ((roomUser?.IsBot ?? true) || roomUser.GetClient() == null || roomUser.GetClient().GetHabbo() == null || roomUser.GetClient().GetHabbo().GetBadgeComponent() == null || string.IsNullOrWhiteSpace(OtherString))
                return false;

            return roomUser.GetClient().GetHabbo().GetBadgeComponent().BadgeList.Values.Cast<Badge>().All(badge => badge.Slot <= 0 || !string.Equals(badge.Code, OtherString, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}