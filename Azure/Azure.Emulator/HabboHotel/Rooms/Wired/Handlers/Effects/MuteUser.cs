#region

using System;
using System.Collections.Generic;
using Azure.HabboHotel.Items;

#endregion

namespace Azure.HabboHotel.Rooms.Wired.Handlers.Effects
{
    public class MuteUser : IWiredItem
    {
        //private List<InteractionType> _mBanned;
        public MuteUser(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            OtherString = string.Empty;
            OtherExtraString = string.Empty;
            OtherExtraString2 = string.Empty;
            Delay = 0;
            //_mBanned = new List<InteractionType>();
        }

        public Interaction Type
        {
            get
            {
                return Interaction.ActionMuteUser;
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

        public int Delay { get; set; }

        public string OtherString { get; set; }

        public string OtherExtraString { get; set; }

        public string OtherExtraString2 { get; set; }

        public bool OtherBool { get; set; }

        public bool Execute(params object[] stuff)
        {
            if (stuff[0] == null) return false;
            var roomUser = (RoomUser)stuff[0];

            if (roomUser == null || roomUser.IsBot || roomUser.GetClient() == null ||
                roomUser.GetClient().GetHabbo() == null)
            {
                return false;
            }

            if (roomUser.GetClient().GetHabbo().Rank > 3)
            {
                return false;
            }

            if (Delay == 0)
            {
                return false;
            }

            int minutes = Delay / 500;
            uint userId = roomUser.GetClient().GetHabbo().Id;

            if (Room.MutedUsers.ContainsKey(userId))
            {
                Room.MutedUsers.Remove(userId);
            }
            Room.MutedUsers.Add(userId, Convert.ToUInt32((Azure.GetUnixTimeStamp() + (minutes * 60))));
            if (!String.IsNullOrEmpty(OtherString))
            {
                roomUser.GetClient().SendWhisper(OtherString);
            }
            return true;
        }
    }
}