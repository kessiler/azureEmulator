#region

using System;
using System.Collections.Generic;
using System.Net;
using System.Xml;

#endregion

namespace Azure.Configuration
{
    /// <summary>
    /// Struct FurniData
    /// </summary>
    public struct FurniData
    {
        /// <summary>
        /// The identifier
        /// </summary>
        public int Id;

        /// <summary>
        /// The name
        /// </summary>
        public string Name;

        /// <summary>
        /// The x
        /// </summary>
        public ushort X, Y;

        /// <summary>
        /// The can sit
        /// </summary>
        public bool CanSit, CanWalk;

        /// <summary>
        /// Initializes a new instance of the <see cref="FurniData"/> struct.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="canSit">if set to <c>true</c> [can sit].</param>
        /// <param name="canWalk">if set to <c>true</c> [can walk].</param>
        public FurniData(int id, string name, ushort x = 0, ushort y = 0, bool canSit = false, bool canWalk = false)
        {
            Id = id;
            Name = name;
            X = x;
            Y = y;
            CanSit = canSit;
            CanWalk = canWalk;
        }
    }

    /// <summary>
    /// Class FurniDataParser.
    /// </summary>
    internal static class FurniDataParser
    {
        /// <summary>
        /// The floor items
        /// </summary>
        public static Dictionary<string, FurniData> FloorItems;

        /// <summary>
        /// The wall items
        /// </summary>
        public static Dictionary<string, FurniData> WallItems;

        /// <summary>
        /// Sets the cache.
        /// </summary>
        public static void SetCache()
        {
            var xmlParser = new XmlDocument();
            var wC = new WebClient();

             try
            {
                xmlParser.LoadXml(wC.DownloadString(ExtraSettings.FurniDataUrl));
                FloorItems = new Dictionary<string, FurniData>();
                foreach (XmlNode node in xmlParser.DocumentElement.SelectNodes("/furnidata/roomitemtypes/furnitype"))
                {
                    try
                    {
                    
                        FloorItems.Add(node.Attributes["classname"].Value,
                            new FurniData(int.Parse(node.Attributes["id"].Value), node.SelectSingleNode("name").InnerText,
                                ushort.Parse(node.SelectSingleNode("xdim").InnerText),
                                ushort.Parse(node.SelectSingleNode("ydim").InnerText),
                                node.SelectSingleNode("cansiton").InnerText == "1",
                                node.SelectSingleNode("canstandon").InnerText == "1"));
                    }
                    catch(Exception e)
                    {
                        var k = node.Attributes["classname"].Value;
                        if(!string.IsNullOrEmpty(k))
                        Console.WriteLine("Errror parsing furnidata by {0} with exception: {1}", k, e.StackTrace);

                    }
                }
                WallItems = new Dictionary<string, FurniData>();
                foreach (XmlNode node in xmlParser.DocumentElement.SelectNodes("/furnidata/wallitemtypes/furnitype"))
                    WallItems.Add(node.Attributes["classname"].Value, new FurniData(int.Parse(node.Attributes["id"].Value), node.SelectSingleNode("name").InnerText));
            }
            catch (WebException e)
            {
                Out.WriteLine(
                    string.Format("Error downloading furnidata.xml: {0}", Environment.NewLine + e), "Azure.FurniData",
                    ConsoleColor.Red);
                Out.WriteLine("Type a key to close", "", ConsoleColor.Black);
                Console.ReadKey();
                Environment.Exit(e.HResult);
            }
            catch (XmlException e)
            {
                Out.WriteLine(string.Format("Error parsing furnidata.xml: {0}", Environment.NewLine + e), "Azure.FurniData",
                    ConsoleColor.Red);
                Out.WriteLine("Type a key to close", "", ConsoleColor.Black);
                Console.ReadKey();
                Environment.Exit(e.HResult);
            }
            catch (NullReferenceException e)
            {
                Out.WriteLine(string.Format("Error parsing value null of furnidata.xml: {0}", Environment.NewLine + e), "Azure.FurniData", ConsoleColor.Red);
                Out.WriteLine("Type a key to close", "", ConsoleColor.Black);
                Console.ReadKey();
                Environment.Exit(e.HResult);
            }
            wC.Dispose();
            xmlParser = null;
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public static void Clear()
        {
            FloorItems.Clear();
            WallItems.Clear();
            FloorItems = null;
            WallItems = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}