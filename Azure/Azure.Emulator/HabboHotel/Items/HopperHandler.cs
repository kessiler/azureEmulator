#region

using System;

#endregion

namespace Azure.HabboHotel.Items
{
    /// <summary>
    /// Class HopperHandler.
    /// </summary>
    internal static class HopperHandler
    {
        /// <summary>
        /// Gets a hopper.
        /// </summary>
        /// <param name="curRoom">The current room.</param>
        /// <returns>System.UInt32.</returns>
        internal static uint GetAHopper(uint curRoom)
        {
            uint result;
            using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery(string.Format("SELECT room_id FROM items_hopper WHERE room_id <> '{0}' ORDER BY RAND() LIMIT 1", curRoom));
                var num = Convert.ToUInt32(queryReactor.GetInteger());
                result = num;
            }
            return result;
        }

        /// <summary>
        /// Gets the hopper identifier.
        /// </summary>
        /// <param name="nextRoom">The next room.</param>
        /// <returns>System.UInt32.</returns>
        internal static uint GetHopperId(uint nextRoom)
        {
            uint result;
            using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery("SELECT hopper_id FROM items_hopper WHERE room_id = @room LIMIT 1");
                queryReactor.AddParameter("room", nextRoom);
                var @string = queryReactor.GetString();
                result = @string == null ? 0u : Convert.ToUInt32(@string);
            }
            return result;
        }
    }
}