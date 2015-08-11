namespace Azure.HabboHotel.Rooms.RoomInvokedItems
{
    internal struct RoomKick
    {
        internal string Alert;
        internal int MinRank;

        public RoomKick(string alert, int minRank)
        {
            Alert = alert;
            MinRank = minRank;
        }
    }
}