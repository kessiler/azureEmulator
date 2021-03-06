﻿using System;
using System.Diagnostics;
using System.Runtime;
using Azure.HabboHotel;
using Azure.Messages;
using Azure.Messages.Parsers;
using Azure.Security;
using Azure.Security.BlackWords;
using Azure.Util;

namespace Azure.Configuration
{
    /// <summary>
    /// Class ConsoleCommandHandling.
    /// </summary>
    internal class ConsoleCommandHandling
    {
        /// <summary>
        /// Gets the game.
        /// </summary>
        /// <returns>Game.</returns>
        internal static Game GetGame() => Azure.GetGame();

        /// <summary>
        /// Invokes the command.
        /// </summary>
        /// <param name="inputData">The input data.</param>
        internal static void InvokeCommand(string inputData)
        {
            if (string.IsNullOrEmpty(inputData) && Logging.DisabledState)
                return;

            Console.WriteLine();

            try
            {
                if (inputData == null)
                    return;

                var strArray = inputData.Split(' ');

                switch (strArray[0])
                {
                    case "shutdown":
                    case "close":
                        Logging.DisablePrimaryWriting(true);
                        Out.WriteLine("Shutdown Initalized", "Azure.Life", ConsoleColor.DarkYellow);
                        Azure.PerformShutDown(false);
                        Console.WriteLine();
                        break;

                    case "restart":
                        Logging.LogMessage($"Server Restarting at {DateTime.Now}");
                        Logging.DisablePrimaryWriting(true);
                        Out.WriteLine("Restart Initialized", "Azure.Life", ConsoleColor.DarkYellow);
                        Azure.PerformShutDown(true);
                        Console.WriteLine();
                        break;

                    case "flush":
                    case "reload":
                        if (strArray.Length >= 2) break;
                        Console.WriteLine("Please specify parameter. Type 'help' to know more about Console Commands");
                        Console.WriteLine();
                        break;

                    case "alert":
                        {
                            var str = inputData.Substring(6);
                            var message = new ServerMessage(LibraryParser.OutgoingRequest("BroadcastNotifMessageComposer"));
                            message.AppendString(str);
                            message.AppendString(string.Empty);
                            GetGame().GetClientManager().QueueBroadcaseMessage(message);
                            Console.WriteLine("[{0}] was sent!", str);
                            return;
                        }
                    case "clear":
                        Console.Clear();
                        break;

                    case "status":
                        TimeSpan uptime = DateTime.Now - Azure.ServerStarted;

                        Console.WriteLine("Server status:");
                        Console.WriteLine();
                        Console.WriteLine("Uptime:");
                        Console.WriteLine("\tDays:    {0}", uptime.Days);
                        Console.WriteLine("\tHours:   {0}", uptime.Hours);
                        Console.WriteLine("\tMinutes: {0}", uptime.Minutes);
                        Console.WriteLine();
                        Console.WriteLine("Stats:");
                        Console.WriteLine("\tAccepted Connections: {0}", Azure.GetConnectionManager().Manager.AcceptedConnections);
                        Console.WriteLine("\tActive Threads: {0}", Process.GetCurrentProcess().Threads.Count);
                        Console.WriteLine();
                        Console.WriteLine();
                        break;

                    case "gcinfo":
                        {
                            Console.WriteLine("Mode: " + GCSettings.LatencyMode);
                            Console.WriteLine("Is server GC: " + GCSettings.IsServerGC);

                            break;
                        }

                    case "memstat":
                        {
                            Console.WriteLine("GC status:");
                            Console.WriteLine("\tGeneration supported: " + GC.MaxGeneration);
                            Console.WriteLine("\tLatency mode: " + GCSettings.LatencyMode);
                            Console.WriteLine("\tIs server GC: " + GCSettings.IsServerGC);
                            Console.WriteLine();
                            break;
                        }

                    case "memory":
                        {
                            GC.Collect();
                            Console.WriteLine("Memory flushed");

                            break;
                        }

                    case "help":
                        Console.WriteLine("shutdown/close - for safe shutting down AzureEmulator");
                        Console.WriteLine("clear - Clear all text");
                        Console.WriteLine("memory - Call gargabe collector");
                        Console.WriteLine("alert (msg) - send alert to Every1!");
                        Console.WriteLine("flush/reload");
                        Console.WriteLine("   - catalog");
                        Console.WriteLine("   - modeldata");
                        Console.WriteLine("   - bans");
                        Console.WriteLine("   - packets (reload packets ids)");
                        Console.WriteLine("   - filter");
                        Console.WriteLine();
                        break;

                    default:
                        UnknownCommand(inputData);
                        break;
                }

                switch (strArray[1])
                {
                    case "database":
                        Azure.GetDatabaseManager().Destroy();
                        Console.WriteLine("Database destroyed");
                        Console.WriteLine();
                        break;

                    case "packets":
                        LibraryParser.ReloadDictionarys();
                        Console.WriteLine("> Packets Reloaded Suceffuly...");
                        Console.WriteLine();
                        break;

                    case "catalog":
                    case "shop":
                    case "catalogus":
                        FurniDataParser.SetCache();
                        using (var adapter = Azure.GetDatabaseManager().GetQueryReactor()) GetGame().GetCatalog().Initialize(adapter);
                        FurniDataParser.Clear();

                        GetGame()
                            .GetClientManager()
                            .QueueBroadcaseMessage(
                                new ServerMessage(LibraryParser.OutgoingRequest("PublishShopMessageComposer")));
                        Console.WriteLine("Catalogue was re-loaded.");
                        Console.WriteLine();
                        break;

                    case "modeldata":
                        using (var adapter2 = Azure.GetDatabaseManager().GetQueryReactor()) GetGame().GetRoomManager().LoadModels(adapter2);
                        Console.WriteLine("Room models were re-loaded.");
                        Console.WriteLine();
                        break;

                    case "bans":
                        using (var adapter3 = Azure.GetDatabaseManager().GetQueryReactor()) GetGame().GetBanManager().LoadBans(adapter3);
                        Console.WriteLine("Bans were re-loaded");
                        Console.WriteLine();
                        break;

                    case "filter":
                        Filter.Reload();
                        BlackWordsManager.Reload();
                        break;

                    default:
                        UnknownCommand(inputData);
                        Console.WriteLine();
                        break;
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        /// <summary>
        /// Unknowns the command.
        /// </summary>
        /// <param name="command">The command.</param>
        private static void UnknownCommand(string command)
        {
            Out.WriteLine("Manager", "Undefined Command: " + command);
        }
    }
}