﻿#region

using System.Collections.Generic;
using Azure.HabboHotel.Items;

#endregion

namespace Azure.HabboHotel.Rooms.Wired.Handlers.Effects
{
    public class BotFollowAvatar : IWiredItem
    {
        //private List<InteractionType> mBanned;
        public BotFollowAvatar(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            OtherString = string.Empty;
            OtherExtraString = string.Empty;
            OtherExtraString2 = string.Empty;
            //this.mBanned = new List<InteractionType>();
        }

        public Interaction Type
        {
            get
            {
                return Interaction.ActionBotFollowAvatar;
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

        public int Delay
        {
            get
            {
                return 0;
            }
            set
            {
            }
        }

        public string OtherString { get; set; }

        public string OtherExtraString { get; set; }

        public string OtherExtraString2 { get; set; }

        public bool OtherBool { get; set; }

        public bool Execute(params object[] stuff)
        {
            //RoomUser roomUser = (RoomUser)stuff[0];
            //InteractionType item = (InteractionType)stuff[1];
            return true;
        }
    }
}