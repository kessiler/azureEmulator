#region

using System;
using Azure.Database.Manager.Database.Session_Details.Interfaces;
using Azure.Messages;

#endregion

namespace Azure.HabboHotel.Items
{
    /// <summary>
    /// Class CrackableEggHandler.
    /// </summary>
    internal class CrackableEggHandler
    {
        /// <summary>
        /// Initializes the specified database client.
        /// </summary>
        /// <param name="dbClient">The database client.</param>
        internal void Initialize(IQueryAdapter dbClient)
        {
        }

        internal int MaxCracks(string itemName)
        {
            switch (itemName)
            {
                case "easter13_egg_0":
                    return 1000;

                case "easter13_egg_1":
                    return 5000;

                case "easter13_egg_2":
                    return 10000;

                case "easter13_egg_3":
                    return 20000;

                default:
                    return 1;
            }
        }

        internal ServerMessage GetServerMessage(ServerMessage message, RoomItem item)
        {
            var cracks = 0;
            var cracks_max = MaxCracks(item.GetBaseItem().Name);
            if (Azure.IsNum(item.ExtraData))
                cracks = Convert.ToInt16(item.ExtraData);
            var state = "0";
            if (cracks >= cracks_max)
                state = "14";
            else if (cracks >= cracks_max * 6 / 7)
                state = "12";
            else if (cracks >= cracks_max * 5 / 7)
                state = "10";
            else if (cracks >= cracks_max * 4 / 7)
                state = "8";
            else if (cracks >= cracks_max * 3 / 7)
                state = "6";
            else if (cracks >= cracks_max * 2 / 7)
                state = "4";
            else if (cracks >= cracks_max * 1 / 7)
                state = "2";
            message.AppendInteger(7);
            message.AppendString(state); //state (0-7)
            message.AppendInteger(cracks); //actual
            message.AppendInteger(cracks_max); //max
            return message;
        }
    }
}