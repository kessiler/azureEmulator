#region

using System.Collections.Generic;

#endregion

namespace Azure.HabboHotel.Catalogs
{
    /// <summary>
    /// Class GiftWrappers.
    /// </summary>
    internal static class GiftWrappers
    {
        /// <summary>
        /// The gift wrappers list
        /// </summary>
        public static List<int> GiftWrappersList = new List<int>();

        /// <summary>
        /// The old gift wrappers
        /// </summary>
        public static List<int> OldGiftWrappers = new List<int>();

        /// <summary>
        /// Adds the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public static void Add(int id)
        {
            GiftWrappersList.Add(id);
        }

        /// <summary>
        /// Adds the old.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public static void AddOld(int id)
        {
            OldGiftWrappers.Add(id);
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public static void Clear()
        {
            GiftWrappersList.Clear();
            OldGiftWrappers.Clear();
        }
    }
}