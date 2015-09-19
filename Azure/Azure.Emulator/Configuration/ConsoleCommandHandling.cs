#region

using System;
using Azure.HabboHotel;
using Azure.Messages;
using Azure.Messages.Parsers;
using Azure.Security;
using Azure.Security.BlackWords;

#endregion

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
        internal static Game GetGame()
        {
            return Azure.GetGame();
        }

        //private static void ConnectCallback(IAsyncResult ar)
        //{
        //    try
        //    {
        //        // Retrieve the socket from the state object.
        //        System.Net.Sockets.Socket client = (System.Net.Sockets.Socket)ar.AsyncState;

        //        // Complete the connection.
        //        client.EndConnect(ar);

        //        Console.WriteLine("Socket connected to {0}",
        //            client.RemoteEndPoint.ToString());

        //        // Signal that the connection has been made.
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e.ToString());
        //    }
        //}

        /// <summary>
        /// Invokes the command.
        /// </summary>
        /// <param name="inputData">The input data.</param>
        internal static void InvokeCommand(string inputData)
        {
            if (string.IsNullOrEmpty(inputData) && Logging.DisabledState) return;
            Console.WriteLine();

            try
            {
                if (inputData == null) return;
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
                        Logging.LogMessage(string.Format("Server Restarting at {0}", DateTime.Now));
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

                    case "memory":
                        {
                            GC.Collect();
                            Console.WriteLine("Memory flushed");

                            break;
                        }
                    case "lag":
                        //if (Azure.DebugMode)
                        //{
                        //    new System.Threading.Tasks.Task(() =>
                        //    {
                        //        for (uint i = 0; i < 5000; i++)
                        //        {
                        //            try
                        //            {
                        //                Azure.GetGame().GetRoomManager().LoadRoom(i);
                        //            }
                        //            catch (Exception exception)
                        //            {
                        //                Console.WriteLine(exception.Message);
                        //            }
                        //        }
                        //    }).Start();
                        //    int countable = 0;
                        //    for (uint i = 0; i < 10000; i++)
                        //    {
                        //        countable++;
                        //        new System.Threading.Tasks.Task(() =>
                        //        {
                        //            System.Net.IPAddress ipAddress = System.Net.IPAddress.Parse("127.0.0.1");
                        //            System.Net.IPEndPoint remoteEP = new System.Net.IPEndPoint(ipAddress, 30000);
                        //            System.Net.Sockets.Socket client = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork,System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);

                        //            // Connect to the remote endpoint.
                        //            client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), client);
                        //        }).Start();
                        //        if (countable == 150) { System.Threading.Thread.Sleep(5000); countable = 0; }
                               
                        //    }
                        //    Console.WriteLine("Lag Test started");
                        //}
                        break;


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
            }
        }

        /// <summary>
        /// Unknowns the command.
        /// </summary>
        /// <param name="command">The command.</param>
        private static void UnknownCommand(string command)
        {
        }
    }
}