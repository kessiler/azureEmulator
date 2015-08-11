#region

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using Azure.HabboHotel.Items;

#endregion

namespace Azure.HabboHotel.Rooms
{
    /// <summary>
    /// Class CoordItemSearch.
    /// </summary>
    internal class CoordItemSearch
    {
        /// <summary>
        /// The _items
        /// </summary>
        private readonly HybridDictionary _items;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoordItemSearch"/> class.
        /// </summary>
        /// <param name="itemArray">The item array.</param>
        public CoordItemSearch(HybridDictionary itemArray)
        {
            _items = itemArray;
        }

        /// <summary>
        /// Gets the room item for square.
        /// </summary>
        /// <param name="pX">The p x.</param>
        /// <param name="pY">The p y.</param>
        /// <param name="minZ">The minimum z.</param>
        /// <returns>List&lt;RoomItem&gt;.</returns>
        internal List<RoomItem> GetRoomItemForSquare(int pX, int pY, double minZ)
        {
            var list = new List<RoomItem>();
            var point = new Point(pX, pY);
            if (!_items.Contains(point))
            {
                return list;
            }
            var list2 = (List<RoomItem>)_items[point];
            list.AddRange(list2.Where(current => current.Z > minZ && current.X == pX && current.Y == pY));
            return list;
        }

        /// <summary>
        /// Gets the room item for square.
        /// </summary>
        /// <param name="pX">The p x.</param>
        /// <param name="pY">The p y.</param>
        /// <returns>List&lt;RoomItem&gt;.</returns>
        internal List<RoomItem> GetRoomItemForSquare(int pX, int pY)
        {
            var point = new Point(pX, pY);
            var list = new List<RoomItem>();
            if (!_items.Contains(point))
            {
                return list;
            }
            var list2 = (List<RoomItem>)_items[point];
            list.AddRange(list2.Where(current => current.Coordinate.X == point.X && current.Coordinate.Y == point.Y));
            return list;
        }

        /// <summary>
        /// Gets all room item for square.
        /// </summary>
        /// <param name="pX">The p x.</param>
        /// <param name="pY">The p y.</param>
        /// <returns>List&lt;RoomItem&gt;.</returns>
        internal List<RoomItem> GetAllRoomItemForSquare(int pX, int pY)
        {
            var point = new Point(pX, pY);
            var list = new List<RoomItem>();
            if (!_items.Contains(point))
            {
                return list;
            }
            var list2 = (List<RoomItem>)_items[point];
            foreach (RoomItem current in list2.Where(current => !list.Contains(current)))
            {
                list.Add(current);
            }
            return list;
        }
    }
}