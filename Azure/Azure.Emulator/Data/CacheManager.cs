using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Azure.Game.Rooms.Data;
using Azure.Game.Users;

namespace Azure.Data
{
    /// <summary>
    /// Class CacheManager.
    /// </summary>
    public static class CacheManager
    {
        /// <summary>
        /// The _thread
        /// </summary>
        private static Thread _thread;
        /// <summary>
        /// The working
        /// </summary>
        public static bool Working;

        /// <summary>
        /// Starts the process.
        /// </summary>
        public static void StartProcess()
        {
            _thread = new Thread(Process) { Name = "Cache Thread" };
            _thread.Start();
            Working = true;
        }

        /// <summary>
        /// Stops the process.
        /// </summary>
        public static void StopProcess()
        {
            _thread.Abort();
            Working = false;
        }

        /// <summary>
        /// Processes this instance.
        /// </summary>
        private static void Process()
        {
            while (Working)
            {
                ClearUserCache();
                ClearRoomsCache();

                GC.Collect();
                GC.WaitForPendingFinalizers();

                Thread.Sleep(1000000); // WTF? <<< #TODO WTF!!
            }
        }

        /// <summary>
        /// Clears the user cache.
        /// </summary>
        private static void ClearUserCache()
        {
            var toRemove = new List<uint>();

            foreach (var user in Azure.UsersCached)
            {
                if (user.Value == null)
                {
                    toRemove.Add(user.Key);
                    return;
                }

                if (Azure.GetGame().GetClientManager().Clients.ContainsKey(user.Key))
                    continue;

                if ((DateTime.Now - user.Value.LastUsed).TotalMilliseconds < 1800000)
                    continue;

                toRemove.Add(user.Key);
            }

            foreach (var userId in toRemove)
            {
                Habbo nullHabbo;

                if (Azure.UsersCached.TryRemove(userId, out nullHabbo))
                    nullHabbo = null;
            }
        }

        /// <summary>
        /// Clears the rooms cache.
        /// </summary>
        private static void ClearRoomsCache()
        {
            if (Azure.GetGame() == null || Azure.GetGame().GetRoomManager() == null || Azure.GetGame().GetRoomManager().LoadedRoomData == null)
                return;

            var toRemove = (from roomData in Azure.GetGame().GetRoomManager().LoadedRoomData where roomData.Value != null && roomData.Value.UsersNow <= 0 where !((DateTime.Now - roomData.Value.LastUsed).TotalMilliseconds < 1800000) select roomData.Key).ToList();

            foreach (var roomId in toRemove)
            {
                RoomData nullRoom;

                if (Azure.GetGame().GetRoomManager().LoadedRoomData.TryRemove(roomId, out nullRoom))
                    nullRoom = null;
            }
        }
    }
}