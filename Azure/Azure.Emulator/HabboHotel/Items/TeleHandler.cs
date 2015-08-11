#region

using System;
using Azure.HabboHotel.Rooms;

#endregion

namespace Azure.HabboHotel.Items
{
    /// <summary>
    /// Class TeleHandler.
    /// </summary>
    internal static class TeleHandler
    {
        /// <summary>
        /// Gets the linked tele.
        /// </summary>
        /// <param name="teleId">The tele identifier.</param>
        /// <param name="pRoom">The p room.</param>
        /// <returns>System.UInt32.</returns>
        internal static uint GetLinkedTele(uint teleId, Room pRoom)
        {
            uint result = uint.MinValue;
            using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery(string.Format("SELECT tele_two_id FROM items_teleports WHERE tele_one_id = {0}", teleId));
                var row = queryReactor.GetRow();
                result = row == null ? 0u : Convert.ToUInt32(row[0]);
            }
            return result;
        }

        /// <summary>
        /// Gets the tele room identifier.
        /// </summary>
        /// <param name="teleId">The tele identifier.</param>
        /// <param name="pRoom">The p room.</param>
        /// <returns>System.UInt32.</returns>
        internal static uint GetTeleRoomId(uint teleId, Room pRoom)
        {
            if (pRoom.GetRoomItemHandler().GetItem(teleId) != null)
                return pRoom.RoomId;
            uint result = uint.MinValue;
            using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery(string.Format("SELECT room_id FROM items_rooms WHERE id = {0} LIMIT 1", teleId));
                var row = queryReactor.GetRow();
                result = row == null ? 0u : Convert.ToUInt32(row[0]);
            }
            return result;
        }

        /// <summary>
        /// Determines whether [is tele linked] [the specified tele identifier].
        /// </summary>
        /// <param name="teleId">The tele identifier.</param>
        /// <param name="pRoom">The p room.</param>
        /// <returns><c>true</c> if [is tele linked] [the specified tele identifier]; otherwise, <c>false</c>.</returns>
        internal static bool IsTeleLinked(uint teleId, Room pRoom)
        {
            var linkedTele = GetLinkedTele(teleId, pRoom);
            if (linkedTele == 0u)
                return false;
            var item = pRoom.GetRoomItemHandler().GetItem(linkedTele);
            return (item != null && (item.GetBaseItem().InteractionType == Interaction.Teleport || item.GetBaseItem().InteractionType == Interaction.QuickTeleport)) || GetTeleRoomId(linkedTele, pRoom) != 0u;
        }
    }
}