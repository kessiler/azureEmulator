﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Timers;
using System.Windows.Forms;
using Azure.Configuration;
using Azure.Database;
using Azure.Encryption.Encryption;
using Azure.HabboHotel;
using Azure.HabboHotel.GameClients.Interfaces;
using Azure.HabboHotel.Groups.Interfaces;
using Azure.HabboHotel.Misc;
using Azure.HabboHotel.Pets;
using Azure.HabboHotel.Users;
using Azure.HabboHotel.Users.Messenger;
using Azure.HabboHotel.Users.UserDataManagement;
using Azure.Manager;
using Azure.Messages;
using Azure.Messages.Factorys;
using Azure.Messages.Parsers;
using Azure.Util;
using MySql.Data.MySqlClient;
using Timer = System.Timers.Timer;

namespace Azure
{
    /// <summary>
    /// Class Azure.
    /// </summary>
    public static class Azure
    {
        /// <summary>
        /// Azure Environment: Main Thread of Azure Emulator, SetUp's the Emulator
        /// Contains Initialize: Responsible of the Emulator Loadings
        /// </summary>

        internal static string DatabaseConnectionType = "MySQL", ServerLanguage = "english";

        /// <summary>
        /// The build of the server
        /// </summary>
        internal static readonly string Build = "100", Version = "2.0";

        /// <summary>
        /// The live currency type
        /// </summary>
        internal static int LiveCurrencyType = 105, ConsoleTimer = 2000;

        /// <summary>
        /// The is live
        /// </summary>
        internal static bool IsLive, SeparatedTasksInGameClientManager, SeparatedTasksInMainLoops, DebugMode, ConsoleTimerOn;

        /// <summary>
        /// The staff alert minimum rank
        /// </summary>
        internal static uint StaffAlertMinRank = 4, FriendRequestLimit = 1000;

        /// <summary>
        /// Bobba Filter Muted Users by Filter
        /// </summary>
        internal static Dictionary<uint, uint> MutedUsersByFilter;

        /// <summary>
        /// The manager
        /// </summary>
        internal static DatabaseManager Manager;

        /// <summary>
        /// The configuration data
        /// </summary>
        internal static ConfigData ConfigData;

        /// <summary>
        /// The server started
        /// </summary>
        internal static DateTime ServerStarted;

        /// <summary>
        /// The offline messages
        /// </summary>
        internal static Dictionary<uint, List<OfflineMessage>> OfflineMessages;

        /// <summary>
        /// The timer
        /// </summary>
        internal static Timer Timer;

        /// <summary>
        /// The culture information
        /// </summary>
        internal static CultureInfo CultureInfo;

        /// <summary>
        /// The _plugins
        /// </summary>
        public static Dictionary<string, IPlugin> Plugins;

        /// <summary>
        /// The users cached
        /// </summary>
        public static readonly ConcurrentDictionary<uint, Habbo> UsersCached = new ConcurrentDictionary<uint, Habbo>();

        /// <summary>
        /// The _connection manager
        /// </summary>
        private static ConnectionHandling _connectionManager;

        /// <summary>
        /// The _default encoding
        /// </summary>
        private static Encoding _defaultEncoding;

        /// <summary>
        /// The _game
        /// </summary>
        private static Game _game;

        /// <summary>
        /// The _languages
        /// </summary>
        private static Languages _languages;

        /// <summary>
        /// The allowed special chars
        /// </summary>
        private static readonly HashSet<char> AllowedSpecialChars = new HashSet<char>(new[]
        {
            '-', '.', ' ', 'Ã', '©', '¡', '­', 'º', '³', 'Ã', '‰', '_'
        });

        /// <summary>
        /// Check's if the Shutdown Has Started
        /// </summary>
        /// <value><c>true</c> if [shutdown started]; otherwise, <c>false</c>.</value>
        internal static bool ShutdownStarted { get; set; }

        public static bool ContainsAny(this string haystack, params string[] needles) => needles.Any(haystack.Contains);

        /// <summary>
        /// Start the Plugin System
        /// </summary>
        /// <returns>ICollection&lt;IPlugin&gt;.</returns>
        public static ICollection<IPlugin> LoadPlugins()
        {
            string path = Application.StartupPath + "Plugins";

            if (!Directory.Exists(path))
                return null;

            string[] files = Directory.GetFiles(path, "*.dll", SearchOption.AllDirectories);

            if (files.Length == 0)
                return null;

            List<Assembly> assemblies =
                files.Select(AssemblyName.GetAssemblyName)
                    .Select(Assembly.Load)
                    .Where(assembly => assembly != null)
                    .ToList();

            Type pluginType = typeof(IPlugin);
            var pluginTypes = new List<Type>();

            foreach (var types in from assembly in assemblies where assembly != null select assembly.GetTypes())
                pluginTypes.AddRange(types.Where(type => type != null && !type.IsInterface && !type.IsAbstract).Where(type => type.GetInterface(pluginType.FullName) != null));

            var plugins = new List<IPlugin>(pluginTypes.Count);

            plugins.AddRange(pluginTypes.Select(type => (IPlugin)Activator.CreateInstance(type)).Where(plugin => plugin != null));

            return plugins;
        }

        /// <summary>
        /// Get's Habbo By The User Id
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>Habbo.</returns>
        /// Table: users.id
        internal static Habbo GetHabboById(uint userId)
        {
            try
            {
                GameClient clientByUserId = GetGame().GetClientManager().GetClientByUserId(userId);

                if (clientByUserId != null)
                {
                    Habbo habbo = clientByUserId.GetHabbo();
                    if (habbo != null && habbo.Id > 0)
                    {
                        UsersCached.AddOrUpdate(userId, habbo, (key, value) => habbo);
                        return habbo;
                    }
                }
                else
                {
                    if (UsersCached.ContainsKey(userId))
                        return UsersCached[userId];

                    UserData userData = UserDataFactory.GetUserData((int)userId);

                    if (UsersCached.ContainsKey(userId))
                        return UsersCached[userId];

                    if (userData?.User == null)
                        return null;

                    UsersCached.TryAdd(userId, userData.User);
                    userData.User.InitInformation(userData);

                    return userData.User;
                }
            }
            catch (Exception e)
            {
                Writer.Writer.LogException("Habbo GetHabboForId: " + e);
            }
            return null;
        }

        /// <summary>
        /// Console Clear Thread
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ElapsedEventArgs"/> instance containing the event data.</param>
        internal static void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            Console.Clear();
            Console.WriteLine();

            Out.WriteLine($"Console Cleared in: {DateTime.Now} Next Time on: {ConsoleTimer} Seconds ", "Azure.Boot", ConsoleColor.DarkGreen);

            Console.WriteLine();
            GC.Collect();

            Timer.Start();
        }

        /// <summary>
        /// Main Void, Initializes the Emulator.
        /// </summary>
        internal static void Initialize()
        {
            Console.Title = "Azure Emulator | Loading [...]";
            ServerStarted = DateTime.Now;
            _defaultEncoding = Encoding.Default;
            MutedUsersByFilter = new Dictionary<uint, uint>();
            ChatEmotions.Initialize();

            CultureInfo = CultureInfo.CreateSpecificCulture("en-GB");
            try
            {
                ConfigurationData.Load(Path.Combine(Application.StartupPath, "Settings/main.ini"));
                ConfigurationData.Load(Path.Combine(Application.StartupPath, "Settings/Welcome/settings.ini"), true);

                DatabaseConnectionType = ConfigurationData.Data["db.type"];

                var mySqlConnectionStringBuilder = new MySqlConnectionStringBuilder
                {
                    Server = (ConfigurationData.Data["db.hostname"]),
                    Port = (uint.Parse(ConfigurationData.Data["db.port"])),
                    UserID = (ConfigurationData.Data["db.username"]),
                    Password = (ConfigurationData.Data["db.password"]),
                    Database = (ConfigurationData.Data["db.name"]),
                    MinimumPoolSize = (uint.Parse(ConfigurationData.Data["db.pool.minsize"])),
                    MaximumPoolSize = (uint.Parse(ConfigurationData.Data["db.pool.maxsize"])),
                    Pooling = (true),
                    AllowZeroDateTime = (true),
                    ConvertZeroDateTime = (true),
                    DefaultCommandTimeout = (300u),
                    ConnectionTimeout = (10u)
                };

                Manager = new DatabaseManager(mySqlConnectionStringBuilder.ToString(), DatabaseConnectionType);

                using (var queryReactor = GetDatabaseManager().GetQueryReactor())
                {
                    ConfigData = new ConfigData(queryReactor);
                    PetCommandHandler.Init(queryReactor);
                    PetLocale.Init(queryReactor);
                    OfflineMessages = new Dictionary<uint, List<OfflineMessage>>();
                    OfflineMessage.InitOfflineMessages(queryReactor);
                }

                ConsoleTimer = (int.Parse(ConfigurationData.Data["console.clear.time"]));
                ConsoleTimerOn = (bool.Parse(ConfigurationData.Data["console.clear.enabled"]));
                FriendRequestLimit = ((uint)int.Parse(ConfigurationData.Data["client.maxrequests"]));

                LibraryParser.Incoming = new Dictionary<int, LibraryParser.StaticRequestHandler>();
                LibraryParser.Library = new Dictionary<string, string>();
                LibraryParser.Outgoing = new Dictionary<string, int>();
                LibraryParser.Config = new Dictionary<string, string>();

                LibraryParser.RegisterLibrary();
                LibraryParser.RegisterOutgoing();
                LibraryParser.RegisterIncoming();
                LibraryParser.RegisterConfig();

                Plugins = new Dictionary<string, IPlugin>();

                ICollection<IPlugin> plugins = LoadPlugins();

                if (plugins != null)
                {
                    foreach (var item in plugins.Where(item => item != null))
                    {
                        Plugins.Add(item.PluginName, item);

                        Out.WriteLine("Loaded Plugin: " + item.PluginName + " Version: " + item.PluginVersion, "Azure.Plugins", ConsoleColor.DarkBlue);
                    }
                }

                ExtraSettings.RunExtraSettings();
                FurniDataParser.SetCache();
                CrossDomainPolicy.Set();

                _game = new Game(int.Parse(ConfigurationData.Data["game.tcp.conlimit"]));
                _game.GetNavigator().LoadNewPublicRooms();
                _game.ContinueLoading();
                FurniDataParser.Clear();

                ServerLanguage = (Convert.ToString(ConfigurationData.Data["system.lang"]));
                _languages = new Languages(ServerLanguage);
                Out.WriteLine("Loaded " + _languages.Count() + " Languages Vars", "Azure.Lang");

                if (plugins != null)
                    foreach (var itemTwo in plugins)
                        itemTwo?.message_void();

                if (ConsoleTimerOn)
                    Out.WriteLine("Console Clear Timer is Enabled, with " + ConsoleTimer + " Seconds.", "Azure.Boot");

                ClientMessageFactory.Init();

                Out.WriteLine("Starting up asynchronous sockets server for game connections for port " + int.Parse(ConfigurationData.Data["game.tcp.port"]), "Server.AsyncSocketListener");

                _connectionManager = new ConnectionHandling(int.Parse(ConfigurationData.Data["game.tcp.port"]),
                   int.Parse(ConfigurationData.Data["game.tcp.conlimit"]),
                   int.Parse(ConfigurationData.Data["game.tcp.conperip"]),
                   ConfigurationData.Data["game.tcp.antiddos"].ToLower() == "true",
                   ConfigurationData.Data["game.tcp.enablenagles"].ToLower() == "true");

                if (LibraryParser.Config["Crypto.Enabled"] == "true")
                {
                    Handler.Initialize(LibraryParser.Config["Crypto.RSA.N"], LibraryParser.Config["Crypto.RSA.D"], LibraryParser.Config["Crypto.RSA.E"]);

                    Out.WriteLine("Started RSA crypto service", "Azure.Crypto");
                }
                else
                    Out.WriteLine("The encryption system is disabled. This affects badly to the safety.", "Azure.Crypto", ConsoleColor.DarkYellow);

                Console.WriteLine();

                Out.WriteLine(
                    "Asynchronous sockets server for game connections running on port " +
                    int.Parse(ConfigurationData.Data["game.tcp.port"]) + Environment.NewLine, "Server.AsyncSocketListener");


                // Removed MusSocket from the Server
                //string[] allowedIps = ConfigurationData.Data["mus.tcp.allowedaddr"].Split(';');
                // ReSharper disable once ObjectCreationAsStatement
                //new MusSocket(ConfigurationData.Data["mus.tcp.bindip"],
                //    int.Parse(ConfigurationData.Data["mus.tcp.port"]), allowedIps, 0);

                LibraryParser.Initialize();
                Console.WriteLine();

                if (ConsoleTimerOn)
                {
                    Timer = new Timer { Interval = ConsoleTimer };
                    Timer.Elapsed += TimerElapsed;
                    Timer.Start();
                }

                if (ConfigurationData.Data.ContainsKey("StaffAlert.MinRank"))
                    StaffAlertMinRank = uint.Parse(ConfigurationData.Data["StaffAlert.MinRank"]);

                if (ConfigurationData.Data.ContainsKey("SeparatedTasksInMainLoops.enabled") && ConfigurationData.Data["SeparatedTasksInMainLoops.enabled"] == "true")
                    SeparatedTasksInMainLoops = true;

                if (ConfigurationData.Data.ContainsKey("SeparatedTasksInGameClientManager.enabled") && ConfigurationData.Data["SeparatedTasksInGameClientManager.enabled"] == "true")
                    SeparatedTasksInGameClientManager = true;

                if (ConfigurationData.Data.ContainsKey("Debug"))
                    if (ConfigurationData.Data["Debug"] == "true")
                        DebugMode = true;

                Out.WriteLine("Azure Emulator ready. Status: idle", "Azure.Boot");

                IsLive = true;
            }
            catch (Exception e)
            {
                Out.WriteLine("Error loading config.ini: Configuration file is invalid" + Environment.NewLine + e.Message, "Azure.Boot", ConsoleColor.Red);
                Out.WriteLine("Please press Y to get more details or press other Key to Exit", "Azure.Boot", ConsoleColor.Red);
                ConsoleKeyInfo key = Console.ReadKey();

                if (key.Key == ConsoleKey.Y)
                {
                    Console.WriteLine();
                    Out.WriteLine(
                        Environment.NewLine + "[Message] Error Details: " + Environment.NewLine + e.StackTrace +
                        Environment.NewLine + e.InnerException + Environment.NewLine + e.TargetSite +
                        Environment.NewLine + "[Message]Press Any Key To Exit", "Azure.Boot", ConsoleColor.Red);
                    Console.ReadKey();
                    Environment.Exit(1);
                }
                else
                    Environment.Exit(1);
            }
        }

        /// <summary>
        /// Convert's Enum to Boolean
        /// </summary>
        /// <param name="theEnum">The theEnum.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal static bool EnumToBool(string theEnum) => theEnum == "1";

        /// <summary>
        /// Convert's Boolean to Integer
        /// </summary>
        /// <param name="theBool">if set to <c>true</c> [theBool].</param>
        /// <returns>System.Int32.</returns>
        internal static int BoolToInteger(bool theBool) => theBool ? 1 : 0;

        /// <summary>
        /// Convert's Boolean to Enum
        /// </summary>
        /// <param name="theBool">if set to <c>true</c> [theBool].</param>
        /// <returns>System.String.</returns>
        internal static string BoolToEnum(bool theBool) => theBool ? "1" : "0";

        /// <summary>
        /// Generates a Random Number in the Interval Min,Max
        /// </summary>
        /// <param name="min">The minimum.</param>
        /// <param name="max">The maximum.</param>
        /// <returns>System.Int32.</returns>
        internal static int GetRandomNumber(int min, int max) => RandomNumber.Get(min, max);

        /// <summary>
        /// Get's the Actual Timestamp in Unix Format
        /// </summary>
        /// <returns>System.Int32.</returns>
        internal static int GetUnixTimeStamp() => ((int)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds);

        /// <summary>
        /// Convert's a Unix TimeStamp to DateTime
        /// </summary>
        /// <param name="unixTimeStamp">The unix time stamp.</param>
        /// <returns>DateTime.</returns>
        internal static DateTime UnixToDateTime(double unixTimeStamp) => (new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Local)).AddSeconds(unixTimeStamp).ToLocalTime();

        internal static DateTime UnixToDateTime(int unixTimeStamp) => (new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Local)).AddSeconds(unixTimeStamp).ToLocalTime();

        /// <summary>
        /// Convert timestamp to GroupJoin String
        /// </summary>
        /// <param name="timeStamp">The target.</param>
        /// <returns>System.String.</returns>
        public static string GetGroupDateJoinString(long timeStamp)
        {
            string[] time = UnixToDateTime(timeStamp).ToString("MMMM/dd/yyyy", CultureInfo).Split('/');

            return $"{time[0].Substring(0, 3)} {time[1]}, {time[2]}";
        }

        /// <summary>
        /// Convert's a DateTime to Unix TimeStamp
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns>System.Int32.</returns>
        internal static int DateTimeToUnix(DateTime target) => Convert.ToInt32((target - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds);

        /// <summary>
        /// Get the Actual Time
        /// </summary>
        /// <returns>System.Int64.</returns>
        internal static long Now() => ((long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds);

        internal static int DifferenceInMilliSeconds(DateTime time, DateTime tFrom)
        {
            var time1 = tFrom.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            var time2 = time.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;

            if ((time1 >= double.MaxValue) || (time1 <= double.MinValue) || time1 <= 0.0)
                time1 = 0.0;

            if ((time2 >= double.MaxValue) || (time2 <= double.MinValue) || time2 <= 0.0)
                time2 = 0.0;

            return Convert.ToInt32(time1 - time2);
        }

        /// <summary>
        /// Filter's the Habbo Avatars Figure
        /// </summary>
        /// <param name="figure">The figure.</param>
        /// <returns>System.String.</returns>
        internal static string FilterFigure(string figure) => figure.Any(character => !IsValid(character)) ? "lg-3023-1335.hr-828-45.sh-295-1332.hd-180-4.ea-3168-89.ca-1813-62.ch-235-1332" : figure;

        /// <summary>
        /// Check if is a Valid AlphaNumeric String
        /// </summary>
        /// <param name="inputStr">The input string.</param>
        /// <returns><c>true</c> if [is valid alpha numeric] [the specified input string]; otherwise, <c>false</c>.</returns>
        internal static bool IsValidAlphaNumeric(string inputStr) => !string.IsNullOrEmpty(inputStr.ToLower()) && inputStr.ToLower().All(IsValid);

        /// <summary>
        /// Get a Habbo With the Habbo's Username
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns>Habbo.</returns>
        /// Table: users.username
        internal static Habbo GetHabboForName(string userName)
        {
            try
            {
                using (var queryReactor = GetDatabaseManager().GetQueryReactor())
                {
                    queryReactor.SetQuery("SELECT id FROM users WHERE username = @user");

                    queryReactor.AddParameter("user", userName);

                    int integer = queryReactor.GetInteger();

                    if (integer > 0)
                    {
                        Habbo result = GetHabboById((uint)integer);

                        return result;
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
            return null;
        }

        /// <summary>
        /// Check if the Input String is a Integer
        /// </summary>
        /// <param name="theNum">The theNum.</param>
        /// <returns><c>true</c> if the specified theNum is number; otherwise, <c>false</c>.</returns>
        internal static bool IsNum(string theNum)
        {
            double num;
            return double.TryParse(theNum, out num);
        }

        /// <summary>
        /// Get the Database Configuration Data
        /// </summary>
        /// <returns>ConfigData.</returns>
        internal static ConfigData GetDbConfig() => ConfigData;

        /// <summary>
        /// Get's the Default Emulator Encoding
        /// </summary>
        /// <returns>Encoding.</returns>
        internal static Encoding GetDefaultEncoding() => _defaultEncoding;

        /// <summary>
        /// Get's the Game Connection Manager Handler
        /// </summary>
        /// <returns>ConnectionHandling.</returns>
        internal static ConnectionHandling GetConnectionManager() => _connectionManager;

        /// <summary>
        /// Get's the Game Environment Handler
        /// </summary>
        /// <returns>Game.</returns>
        internal static Game GetGame() => _game;

        /// <summary>
        /// Gets the language.
        /// </summary>
        /// <returns>Languages.</returns>
        internal static Languages GetLanguage() => _languages;

        /// <summary>
        /// Filter's SQL Injection Characters
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>System.String.</returns>
        internal static string FilterInjectionChars(string input)
        {
            input = input.Replace('\u0001', ' ');
            input = input.Replace('\u0002', ' ');
            input = input.Replace('\u0003', ' ');
            input = input.Replace('\t', ' ');

            return input;
        }

        /// <summary>
        /// Get's the Database Manager Handler
        /// </summary>
        /// <returns>DatabaseManager.</returns>
        internal static DatabaseManager GetDatabaseManager() => Manager;

        /// <summary>
        /// Perform's the Emulator Shutdown
        /// </summary>
        internal static void PerformShutDown()
        {
            PerformShutDown(false);
        }

        /// <summary>
        /// Performs the restart.
        /// </summary>
        internal static void PerformRestart()
        {
            PerformShutDown(true);
        }

        /// <summary>
        /// Shutdown the Emulator
        /// </summary>
        /// <param name="restart">if set to <c>true</c> [restart].</param>
        /// Set a Different Message in Hotel
        internal static void PerformShutDown(bool restart)
        {
            DateTime now = DateTime.Now;

            Cache.StopProcess();

            ShutdownStarted = true;

            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("SuperNotificationMessageComposer"));
            serverMessage.AppendString("disconnection");
            serverMessage.AppendInteger(2);
            serverMessage.AppendString("title");
            serverMessage.AppendString("HEY EVERYONE!");
            serverMessage.AppendString("message");
            serverMessage.AppendString(
                restart
                    ? "<b>The hotel is shutting down for a break.<)/b>\nYou may come back later.\r\n<b>So long!</b>"
                    : "<b>The hotel is shutting down for a break.</b><br />You may come back soon. Don't worry, everything's going to be saved..<br /><b>So long!</b>\r\n~ This session was powered by AzureEmulator");
            GetGame().GetClientManager().QueueBroadcaseMessage(serverMessage);
            Console.Title = "Azure Emulator | Shutting down...";

            _game.StopGameLoop();
            _game.GetRoomManager().RemoveAllRooms();
            GetGame().GetClientManager().CloseAll();

            GetConnectionManager().Destroy();

            foreach (Guild group in _game.GetGroupManager().Groups.Values) group.UpdateForum();

            using (var queryReactor = Manager.GetQueryReactor())
            {
                queryReactor.RunFastQuery("UPDATE users SET online = '0'");
                queryReactor.RunFastQuery("UPDATE rooms_data SET users_now = 0");
                queryReactor.RunFastQuery("TRUNCATE TABLE users_rooms_visits");
            }

            _connectionManager.Destroy();
            _game.Destroy();

            try
            {
                Manager.Destroy();
                Out.WriteLine("Game Manager destroyed", "Azure.GameManager", ConsoleColor.DarkYellow);
            }
            catch (Exception e)
            {
                Writer.Writer.LogException("Azure.cs PerformShutDown GameManager" + e);
            }

            TimeSpan span = DateTime.Now - now;

            Out.WriteLine("Elapsed " + TimeSpanToString(span) + "ms on Shutdown Proccess", "Azure.Life", ConsoleColor.DarkYellow);

            if (!restart)
                Out.WriteLine("Shutdown Completed. Press Any Key to Continue...", string.Empty, ConsoleColor.DarkRed);

            if (!restart)
                Console.ReadKey();

            IsLive = false;

            if (restart)
                Process.Start(Assembly.GetEntryAssembly().Location);

            Console.WriteLine("Closing...");
            Environment.Exit(0);
        }

        /// <summary>
        /// Convert's a Unix TimeSpan to A String
        /// </summary>
        /// <param name="span">The span.</param>
        /// <returns>System.String.</returns>
        internal static string TimeSpanToString(TimeSpan span) => string.Concat(span.Seconds, " s, ", span.Milliseconds, " ms");

        /// <summary>
        /// Check's if Input Data is a Valid AlphaNumeric Character
        /// </summary>
        /// <param name="c">The c.</param>
        /// <returns><c>true</c> if the specified c is valid; otherwise, <c>false</c>.</returns>
        private static bool IsValid(char c) => char.IsLetterOrDigit(c) || AllowedSpecialChars.Contains(c);
    }
}