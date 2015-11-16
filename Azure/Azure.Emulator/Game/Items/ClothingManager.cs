using System.Collections.Generic;
using System.Data;
using Azure.Database.Manager.Database.Session_Details.Interfaces;
using Azure.Game.Items.Interfaces;

namespace Azure.Game.Items
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
            dbClient.SetQuery("SELECT * FROM catalog_wearables");
            ClothingItems = new Dictionary<string, ClothingItem>();
            _table = dbClient.GetTable();

            foreach (DataRow dataRow in _table.Rows)
                ClothingItems.Add((string)dataRow["item_name"], new ClothingItem(dataRow));
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