#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Globalization;
using System.Linq;
using Azure.Configuration;
using Azure.Database.Manager.Database.Session_Details.Interfaces;
using Azure.HabboHotel.Catalogs.Wrappers;
using Azure.HabboHotel.Items.Interactions;
using Azure.HabboHotel.Items.Interactions.Enums;
using Azure.HabboHotel.Items.Interfaces;

#endregion

namespace Azure.HabboHotel.Items
{
    /// <summary>
    ///     Class ItemManager.
    /// </summary>
    internal class ItemManager
    {
        /// <summary>
        ///     The items
        /// </summary>
        private readonly HybridDictionary _items;

        /// <summary>
        ///     The photo identifier
        /// </summary>
        internal uint PhotoId;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ItemManager" /> class.
        /// </summary>
        internal ItemManager()
        {
            _items = new HybridDictionary();
        }

        /// <summary>
        ///     Loads the items.
        /// </summary>
        /// <param name="dbClient">The database client.</param>
        /// <param name="itemLoaded">The item loaded.</param>
        internal void LoadItems(IQueryAdapter dbClient, out uint itemLoaded)
        {
            LoadItems(dbClient);
            itemLoaded = (uint)_items.Count;
        }

        public int CountItems()
        {
            return _items.Count;
        }

        /// <summary>
        ///     Loads the items.
        /// </summary>
        /// <param name="dbClient">The database client.</param>
        internal void LoadItems(IQueryAdapter dbClient)
        {
            _items.Clear();

            GiftWrapper.Clear();

            dbClient.SetQuery("SELECT * FROM catalog_furnis");
            var table = dbClient.GetTable();
            if (table == null) return;
            List<double> heights = null;

            foreach (DataRow dataRow in table.Rows)
            {
                try
                {
                    var id = Convert.ToUInt32(dataRow["id"]);
                    var type = Convert.ToChar(dataRow["type"]);
                    var name = Convert.ToString(dataRow["item_name"]);
                    var flatId = Convert.ToInt32(dataRow["flat_id"]);
                    var stackHeightStr = dataRow["stack_height"].ToString();
                    double stackHeight;
                    uint modes;
                    uint.TryParse(dataRow["interaction_modes_count"].ToString(), out modes);
                    var vendingIds = (string)dataRow["vending_ids"];
                    var sub = Azure.EnumToBool(dataRow["subscriber"].ToString());
                    var effect = (int)dataRow["effectid"];
                    var stackable = Convert.ToInt32(dataRow["can_stack"]) == 1;
                    var allowRecycle = Convert.ToInt32(dataRow["allow_recycle"]) == 1;
                    var allowTrade = Convert.ToInt32(dataRow["allow_trade"]) == 1;
                    var allowMarketplaceSell = Convert.ToInt32(dataRow["allow_marketplace_sell"]) == 1;
                    var allowGift = Convert.ToInt32(dataRow["allow_gift"]) == 1;
                    var allowInventoryStack = Convert.ToInt32(dataRow["allow_inventory_stack"]) == 1;
                    var typeFromString = InteractionTypes.GetTypeFromString((string)dataRow["interaction_type"]);

                    var sprite = 0;

                    ushort x = ushort.MinValue, y = ushort.MinValue;
                    var publicName = Convert.ToString(dataRow["item_name"]);
                    bool canWalk = false, canSit = false, stackMultiple = false;

                    if (name.StartsWith("external_image_wallitem_poster")) PhotoId = id;
                    // Special Types of Furnis
                    if (name == "landscape" || name == "floor" || name == "wallpaper")
                    {
                        sprite = FurniDataParser.WallItems[name].Id;
                        x = 1;
                        y = 1;
                    }
                    else if (type == 's' && FurniDataParser.FloorItems.ContainsKey(name))
                    {
                        sprite = FurniDataParser.FloorItems[name].Id;
                        publicName = FurniDataParser.FloorItems[name].Name;
                        x = FurniDataParser.FloorItems[name].X;
                        y = FurniDataParser.FloorItems[name].Y;
                        canWalk = FurniDataParser.FloorItems[name].CanWalk;
                        canSit = FurniDataParser.FloorItems[name].CanSit;
                    }
                    else if (type == 'i' && FurniDataParser.WallItems.ContainsKey(name))
                    {
                        sprite = FurniDataParser.WallItems[name].Id;
                        publicName = FurniDataParser.WallItems[name].Name;
                    }
                    else if (name.StartsWith("a0 pet", StringComparison.InvariantCulture))
                    {
                        x = 1;
                        y = 1;
                        publicName = name;
                    }
                    else if (type != 'e' && type != 'h' && type != 'r' && type != 'b') continue;

                    if (name.StartsWith("present_gen"))
                        GiftWrapper.AddOld(sprite);
                    else if (name.StartsWith("present_wrap*"))
                        GiftWrapper.Add(sprite);

                    // Stack Height Values
                    if (stackHeightStr.Contains(';'))
                    {
                        var heightsStr = stackHeightStr.Split(';');
                        heights =
                            heightsStr.Select(heightStr => double.Parse(heightStr, CultureInfo.InvariantCulture))
                                .ToList();
                        stackHeight = heights[0];
                        stackMultiple = true;
                    }
                    else
                    {
                        stackHeight = double.Parse(stackHeightStr, CultureInfo.InvariantCulture);
                    }

                    // If Can Walk
                    if (InteractionTypes.AreFamiliar(GlobalInteractions.Gate, typeFromString) ||
                        (typeFromString == Interaction.BanzaiPyramid) || (name.StartsWith("hole")))
                        canWalk = false;

                    // Add Item
                    var value = new Item(id, sprite, publicName, name, type, x, y, stackHeight, stackable, canWalk,
                        canSit, allowRecycle, allowTrade, allowMarketplaceSell, allowGift, allowInventoryStack,
                        typeFromString, modes, vendingIds, sub, effect, stackMultiple,
                        (heights == null ? null : heights.ToArray()), flatId);

                    _items.Add(id, value);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    Console.ReadKey();
                    Out.WriteLine(
                        string.Format("Could not load item #{0}, please verify the data is okay.",
                            Convert.ToUInt32(dataRow[0])), "Azure.Items", ConsoleColor.DarkRed);
                }
            }
        }

        internal Item GetItem(uint id)
        {
            return (Item)_items[id];
        }

        internal bool GetItem(string itemName, out Item item)
        {
            foreach (DictionaryEntry entry in _items)
            {
                item = (Item)entry.Value;
                if (item.Name == itemName)
                    return true;
            }

            item = null;

            return false;
        }

        internal Item GetItemByName(string name)
        {
            foreach (DictionaryEntry entry in _items)
            {
                var item = (Item)entry.Value;
                if (item.Name == name)
                    return item;
            }

            return null;
        }

        internal Item GetItemBySpriteId(int spriteId)
        {
            foreach (DictionaryEntry entry in _items)
            {
                var item = (Item)entry.Value;
                if (item.SpriteId == spriteId)
                    return item;
            }
            return null;
        }

        /// <summary>
        ///     Determines whether the specified identifier contains item.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns><c>true</c> if the specified identifier contains item; otherwise, <c>false</c>.</returns>
        internal bool ContainsItem(uint id)
        {
            return _items.Contains(id);
        }
    }
}