#region

using System;

#endregion

namespace Azure.Messages
{
    /// <summary>
    /// Class HabboEncoding.
    /// </summary>
    internal class HabboEncoding
    {
        /// <summary>
        /// Decodes the int32.
        /// </summary>
        /// <param name="v">The v.</param>
        /// <returns>System.Int32.</returns>
        internal static int DecodeInt32(byte[] v)
        {
            if ((v[0] | v[1] | v[2] | v[3]) < 0)
                return -1;
            return ((v[0] << 24) + (v[1] << 16) + (v[2] << 8) + (v[3]));
        }

        /// <summary>
        /// Decodes the int16.
        /// </summary>
        /// <param name="v">The v.</param>
        /// <returns>Int16.</returns>
        internal static Int16 DecodeInt16(byte[] v)
        {
            if ((v[0] | v[1]) < 0)
                return -1;
            var result = ((v[0] << 8) + (v[1]));
            return (Int16)result;
        }

        /// <summary>
        /// Gets the character filter.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>System.String.</returns>
        public static string GetCharFilter(string data)
        {
            for (var i = 0; i <= 13; i++)
                data = data.Replace(Convert.ToChar(i) + "", "[" + i + "]");
            return data;
        }
    }
}