#region

using System.Drawing;

#endregion

namespace Azure.Util
{
    internal class Formatter
    {
        internal static int PointToInt(Point p)
        {
            return CombineXyCoord(p.X, p.Y);
        }

        internal static int CombineXyCoord(int a, int b)
        {
            return (a << 16) | b;
        }
    }
}