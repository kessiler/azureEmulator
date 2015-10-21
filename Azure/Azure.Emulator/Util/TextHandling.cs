#region

using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Azure.Configuration;

#endregion

namespace Azure.Util
{
    /// <summary>
    /// Class TextHandling.
    /// </summary>
    internal class TextHandling
    {
        /// <summary>
        /// Splits the specified k.
        /// </summary>
        /// <param name="k">The k.</param>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        internal static void Split(double k, out int a, out int b)
        {
            b = (int)Math.Round(k % 1 * 100);
            a = (((int)Math.Round(k * 100) - b) / 100);
        }

        /// <summary>
        /// Combines the specified a.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns>System.Double.</returns>
        internal static double Combine(int a, int b)
        {
            return (a + ((float)b / 100));
        }

        /// <summary>
        /// Parses the specified a.
        /// </summary>
        /// <param name="a">a.</param>
        /// <returns>System.Int32.</returns>
        internal static int Parse(string a)
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
        internal static int GetFirstSiffer(double k)
        {
            return (int)Math.Round(k % 1 * 100);
        }

        /// <summary>
        /// Gets the string.
        /// </summary>
        /// <param name="k">The k.</param>
        /// <returns>System.String.</returns>
        internal static string GetString(double k)
        {
            return k.ToString(CultureInfo.InvariantCulture).Replace(',', '.');
        }

        /// <summary>
        /// Booleans to int.
        /// </summary>
        /// <param name="k">if set to <c>true</c> [k].</param>
        /// <returns>System.Int32.</returns>
        internal static int BooleanToInt(bool k)
        {
            return k ? 1 : 0;
        }

        /// <summary>
        /// Filters the HTML.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="allow">if set to <c>true</c> [allow].</param>
        /// <returns>System.String.</returns>
        internal static string FilterHtml(string str, bool allow = false)
        {
            if (allow && ExtraSettings.AdminCanUseHtml)
                return str;

            return Regex.Replace(str, @"</?(?(?=b|i)notag|[a-zA-Z0-9]+)(?:\s[a-zA-Z0-9\-]+=?(?:(["",']?).*?\1?)?)*\s*/?>", string.Empty);
        }
    }
}