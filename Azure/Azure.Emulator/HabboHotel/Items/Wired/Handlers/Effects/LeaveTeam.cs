﻿using System.Collections.Generic;
using Azure.HabboHotel.Items.Interactions.Enums;
using Azure.HabboHotel.Items.Interfaces;
using Azure.HabboHotel.Items.Wired.Interfaces;
using Azure.HabboHotel.Rooms;
using Azure.HabboHotel.Rooms.Items.Games.Teams.Enums;
using Azure.HabboHotel.Rooms.User;

namespace Azure.HabboHotel.Items.Wired.Handlers.Effects
{
    public class LeaveTeam : IWiredItem
    {
        public LeaveTeam(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            OtherString = string.Empty;
            OtherExtraString = string.Empty;
            OtherExtraString2 = string.Empty;
        }

        public Interaction Type => Interaction.ActionLeaveTeam;

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

        public string OtherExtraString { get; set; }

        public string OtherExtraString2 { get; set; }

        public bool OtherBool { get; set; }

        public bool Execute(params object[] stuff)
        {
            if (stuff[0] == null)
                return false;

            var roomUser = (RoomUser)stuff[0];
            var t = roomUser.GetClient().GetHabbo().CurrentRoom.GetTeamManagerForFreeze();

            if (roomUser.Team != Team.None)
            {
                t.OnUserLeave(roomUser);
                roomUser.Team = Team.None;
            }

            return true;
        }
    }
}