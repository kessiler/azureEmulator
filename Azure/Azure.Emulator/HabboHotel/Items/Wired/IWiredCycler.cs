#region

using System.Collections;
using System.Collections.Concurrent;

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