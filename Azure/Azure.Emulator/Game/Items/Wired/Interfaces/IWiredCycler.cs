using System.Collections;
using System.Collections.Concurrent;
using Azure.Game.Rooms.User;

namespace Azure.Game.Items.Wired.Interfaces
{
    public interface IWiredCycler
    {
        Queue ToWork { get; set; }

        ConcurrentQueue<RoomUser> ToWorkConcurrentQueue { get; set; }

        bool OnCycle();
    }
}