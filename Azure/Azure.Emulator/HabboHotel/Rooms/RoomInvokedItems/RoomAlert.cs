namespace Azure.HabboHotel.Rooms.RoomInvokedItems
{
    internal class RoomAlert
    {
        internal string Message;
        internal int minrank;

        public RoomAlert(string Message, int minrank)
        {
            this.Message = Message;
            this.minrank = minrank;
        }
    }
}