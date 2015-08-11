#region

using Azure.Util;

#endregion

namespace Azure.HabboHotel.Items
{
    /// <summary>
    /// Class WallCoordinate.
    /// </summary>
    internal class WallCoordinate
    {
        /// <summary>
        /// The _width x
        /// </summary>
        private readonly int widthX;

        /// <summary>
        /// The _width y
        /// </summary>
        private readonly int widthY;

        /// <summary>
        /// The _length x
        /// </summary>
        private readonly int lengthX;

        /// <summary>
        /// The _length y
        /// </summary>
        private readonly int lengthY;

        /// <summary>
        /// The _side
        /// </summary>
        private readonly char side;

        /// <summary>
        /// Initializes a new instance of the <see cref="WallCoordinate"/> class.
        /// </summary>
        /// <param name="wallPosition">The wall position.</param>
        public WallCoordinate(string wallPosition)
        {
            var posD = wallPosition.Split(' ');
            side = posD[2] == "l" ? 'l' : 'r';
            var widD = posD[0].Substring(3).Split(',');
            widthX = TextHandling.Parse(widD[0]);
            widthY = TextHandling.Parse(widD[1]);
            var lenD = posD[1].Substring(2).Split(',');
            lengthX = TextHandling.Parse(lenD[0]);
            lengthY = TextHandling.Parse(lenD[1]);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WallCoordinate"/> class.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="n">The n.</param>
        public WallCoordinate(double x, double y, sbyte n)
        {
            TextHandling.Split(x, out widthX, out widthY);
            TextHandling.Split(y, out lengthX, out lengthY);
            side = n == 7 ? 'r' : 'l';
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return ":w=" + widthX + "," + widthY + " " + "l=" + lengthX + "," + lengthY + " " + side;
        }

        /// <summary>
        /// Generates the database shit.
        /// </summary>
        /// <returns>System.String.</returns>
        internal string GenerateDbShit()
        {
            return "x: " + TextHandling.Combine(widthX, widthY) + " y: " + TextHandling.Combine(lengthX, lengthY);
        }

        /// <summary>
        /// Gets the x value.
        /// </summary>
        /// <returns>System.Double.</returns>
        internal double GetXValue()
        {
            return TextHandling.Combine(widthX, widthY);
        }

        /// <summary>
        /// Gets the y value.
        /// </summary>
        /// <returns>System.Double.</returns>
        internal double GetYValue()
        {
            return TextHandling.Combine(lengthX, lengthY);
        }

        /// <summary>
        /// ns this instance.
        /// </summary>
        /// <returns>System.Int32.</returns>
        internal int N()
        {
            return side == 'l' ? 8 : 7;
        }
    }
}