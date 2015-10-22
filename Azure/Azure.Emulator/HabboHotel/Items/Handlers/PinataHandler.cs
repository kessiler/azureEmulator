using System;
using System.Collections.Generic;
using System.Data;
using Azure.Database.Manager.Database.Session_Details.Interfaces;
using Azure.HabboHotel.Items.Interactions.Enums;
using Azure.HabboHotel.Items.Interfaces;
using Azure.HabboHotel.Rooms;
using Azure.HabboHotel.Rooms.User;

namespace Azure.HabboHotel.Items.Handlers
{
    /// <summary>
    ///     Class PinataHandler.
    /// </summary>
    internal class PinataHandler
    {
        /// <summary>
        ///     The _table
        /// </summary>
        private DataTable _table;

        /// <summary>
        ///     The pinatas
        /// </summary>
        internal Dictionary<uint, PinataItem> Pinatas;

        /// <summary>
        ///     Initializes the specified database client.
        /// </summary>
        /// <param name="dbClient">The database client.</param>
        internal void Initialize(IQueryAdapter dbClient)
        {
            dbClient.SetQuery("SELECT * FROM items_pinatas");
            Pinatas = new Dictionary<uint, PinataItem>();
            _table = dbClient.GetTable();

            foreach (DataRow dataRow in _table.Rows)
            {
                var value = new PinataItem(dataRow);
                Pinatas.Add(uint.Parse(dataRow["item_baseid"].ToString()), value);
            }
        }

        /// <summary>
        ///     Delivers the random pinata item.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="room">The room.</param>
        /// <param name="item">The item.</param>
        internal void DeliverRandomPinataItem(RoomUser user, Room room, RoomItem item)
        {
            if (room == null || item == null || item.GetBaseItem().InteractionType != Interaction.Pinata ||
                !Pinatas.ContainsKey(item.GetBaseItem().ItemId))
                return;

            PinataItem pinataItem;
            Pinatas.TryGetValue(item.GetBaseItem().ItemId, out pinataItem);

            if (pinataItem == null || pinataItem.Rewards.Count < 1)
                return;

            item.RefreshItem();
            item.BaseItem = pinataItem.Rewards[new Random().Next((pinataItem.Rewards.Count - 1))];

            item.ExtraData = string.Empty;
            room.GetRoomItemHandler().RemoveFurniture(user.GetClient(), item.Id, false);

            using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.RunFastQuery(
                    $"UPDATE items_rooms SET base_item='{item.BaseItem}', extra_data='' WHERE id='{item.Id}'");
                queryReactor.RunQuery();
            }

            if (!room.GetRoomItemHandler().SetFloorItem(user.GetClient(), item, item.X, item.Y, 0, true, false, true))
                user.GetClient().GetHabbo().GetInventoryComponent().AddItem(item);
        }
    }
}