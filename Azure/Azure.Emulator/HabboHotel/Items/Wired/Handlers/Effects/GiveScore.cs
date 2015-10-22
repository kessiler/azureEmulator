﻿using System.Collections.Generic;
using Azure.HabboHotel.Items.Interactions.Enums;
using Azure.HabboHotel.Items.Interfaces;
using Azure.HabboHotel.Rooms.Items.Games.Teams.Enums;
using Azure.HabboHotel.Rooms.User;

namespace Azure.HabboHotel.Rooms.Wired.Handlers.Effects
{
    public class GiveScore : IWiredItem
    {
        //private List<InteractionType> mBanned;
        public GiveScore(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            OtherString = "10,1";
            OtherExtraString = "0";
            OtherExtraString2 = string.Empty;
            //mBanned = new List<InteractionType>();
        }

        public Interaction Type => Interaction.ActionGiveScore;

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
            if (stuff[0] == null) return false;
            if ((Interaction)stuff[1] == Interaction.TriggerScoreAchieved) return false;

            var roomUser = (RoomUser)stuff[0];

            if (roomUser == null)
                return false;
            if (roomUser.Team == Team.None)
                return false;

            int timesDone;
            int.TryParse(OtherExtraString, out timesDone);

            var scoreToAchieve = 10;
            var maxTimes = 1;

            if (!string.IsNullOrWhiteSpace(OtherString))
            {
                var integers = OtherString.Split(',');
                scoreToAchieve = int.Parse(integers[0]);
                maxTimes = int.Parse(integers[1]);
            }

            if (timesDone >= maxTimes)
                return false;

            Room.GetGameManager().AddPointToTeam(roomUser.Team, scoreToAchieve, roomUser);
            timesDone++;

            OtherExtraString = timesDone.ToString();
            return true;
        }
    }
}