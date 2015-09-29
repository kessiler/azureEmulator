#region

using System.Collections.Generic;
using Azure.HabboHotel.Items;

#endregion

namespace Azure.HabboHotel.Rooms.Wired
{
    public interface WiredItem
    {
        Interaction Type { get; }

        RoomItem Item { get; set; }

        Room Room { get; set; }

        List<RoomItem> Items { get; set; }

        string OtherString { get; set; }

        bool OtherBool { get; set; }

        string OtherExtraString { get; set; }

        string OtherExtraString2 { get; set; }

        int Delay { get; set; }

        bool Execute(params object[] stuff);
    }
}