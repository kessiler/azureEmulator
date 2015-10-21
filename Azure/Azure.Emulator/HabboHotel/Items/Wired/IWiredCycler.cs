#region

using System.Collections;
using System.Collections.Concurrent;
using Azure.HabboHotel.Rooms.User;

#endregion

namespace Azure.HabboHotel.Rooms.Wired
{
    public interface IWiredCycler
    {
        Queue ToWork { get; set; }

        ConcurrentQueue<RoomUser> ToWorkConcurrentQueue { get; set; }

        bool OnCycle();
    }
}