using System.Collections.Generic;
using System.Data;
using Azure.Database.Manager.Database.Session_Details.Interfaces;
using Azure.HabboHotel.Items.Interfaces;

namespace Azure.HabboHotel.Items
{
    /// <summary>
    ///     Class ClothingManager.
    /// </summary>
    internal class ClothingManager
    {
        /// <summary>
        ///     The _table
        /// </summary>
        private DataTable _table;

        /// <summary>
        ///     The clothing items
        /// </summary>
        internal Dictionary<string, ClothingItem> ClothingItems;

        /// <summary>
        ///     Initializes the specified database client.
        /// </summary>
        /// <param name="dbClient">The database client.</param>
        internal void Initialize(IQueryAdapter dbClient)
        {
            dbClient.SetQuery("SELECT * FROM catalog_clothing");
            ClothingItems = new Dictionary<string, ClothingItem>();
            _table = dbClient.GetTable();

            foreach (DataRow dataRow in _table.Rows)
            {
                var value = new ClothingItem(dataRow);

                ClothingItems.Add((string)dataRow["item_name"], value);
            }
        }

        /// <summary>
        ///     Gets the clothes in furni.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>ClothingItem.</returns>
        internal ClothingItem GetClothesInFurni(string name)
        {
            ClothingItem clothe;
            ClothingItems.TryGetValue(name, out clothe);

            return clothe;
        }
    }
}