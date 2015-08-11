#region

using System;
using System.Collections.Generic;
using System.Data;

#endregion

namespace Azure.HabboHotel.Items
{
    /// <summary>
    /// Class ClothingItem.
    /// </summary>
    internal class ClothingItem
    {
        /// <summary>
        /// The item name
        /// </summary>
        internal string ItemName;

        /// <summary>
        /// The clothes
        /// </summary>
        internal List<int> Clothes;

        /// <summary>
        /// The identifier
        /// </summary>
        internal uint Id;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClothingItem"/> class.
        /// </summary>
        /// <param name="row">The row.</param>
        internal ClothingItem(DataRow row)
        {
            Clothes = new List<int>();
            ItemName = Convert.ToString(row["item_name"]);
            Id = Convert.ToUInt32(row["id"]);
            var text = Convert.ToString(row["clothings"]);
            if (!string.IsNullOrEmpty(text))
            {
                if (text.Contains(","))
                {
                    string[] array = text.Split(',');
                    foreach (string value in array)
                    {
                        if (string.IsNullOrWhiteSpace(value))
                        {
                            Clothes.Add(Convert.ToInt32(value.Replace(" ", string.Empty)));
                        }
                        else
                        {
                            Clothes.Add(Convert.ToInt32(value));
                        }
                    }
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        text = text.Replace(" ", string.Empty);
                    }

                    if (!string.IsNullOrEmpty(text))
                    {
                        Clothes.Add(Convert.ToInt32(text));
                    }
                }
            }
        }
    }
}