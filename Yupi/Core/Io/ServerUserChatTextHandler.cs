using System.Globalization;
using System.Text.RegularExpressions;

namespace Yupi.Core.Io
{
    /// <summary>
    /// Class ServerUserChatTextHandler.
    /// </summary>
    public class ServerUserChatTextHandler
    {
        /// <summary>
        /// Splits the specified k.
        /// </summary>
        /// <param name="k">The k.</param>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        public static void Split(double k, out int a, out int b)
        {
            b = (int)System.Math.Round(k % 1 * 100);
            a = (((int)System.Math.Round(k * 100) - b) / 100);
        }

        /// <summary>
        /// Combines the specified a.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns>System.Double.</returns>
        public static double Combine(int a, int b) => (a + ((float)b / 100));

        /// <summary>
        /// Parses the specified a.
        /// </summary>
        /// <param name="a">a.</param>
        /// <returns>System.Int32.</returns>
        public static int Parse(string a)
        {
            int w = 0, i = 0, length = a.Length;

            if (length == 0)
                return 0;

            do
            {
                int k = a[i++];

                if (k < 48 || k > 59)
                    return 0;

                w = 10 * w + k - 48;

            } while (i < length);

            return w;
        }

        /// <summary>
        /// Gets the first siffer.
        /// </summary>
        /// <param name="k">The k.</param>
        /// <returns>System.Int32.</returns>
        public static int GetFirstSiffer(double k) => (int)System.Math.Round(k % 1 * 100);

        /// <summary>
        /// Gets the string.
        /// </summary>
        /// <param name="k">The k.</param>
        /// <returns>System.String.</returns>
        public static string GetString(double k) => k.ToString(CultureInfo.InvariantCulture).Replace(',', '.');

        /// <summary>
        /// Booleans to int.
        /// </summary>
        /// <param name="k">if set to <c>true</c> [k].</param>
        /// <returns>System.Int32.</returns>
        public static int BooleanToInt(bool k) => k ? 1 : 0;

        /// <summary>
        /// Filters the HTML.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="allow">if set to <c>true</c> [allow].</param>
        /// <returns>System.String.</returns>
        public static string FilterHtml(string str, bool allow = false) => allow ? str : Regex.Replace(str, @"</?(?(?=b|i)notag|[a-zA-Z0-9]+)(?:\s[a-zA-Z0-9\-]+=?(?:(["",']?).*?\1?)?)*\s*/?>", string.Empty);
    }
}