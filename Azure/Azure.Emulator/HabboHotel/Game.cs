#region

using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Configuration;
using Azure.Database.Manager.Database.Session_Details.Interfaces;
using Azure.HabboHotel.Achievements;
using Azure.HabboHotel.Catalogs;
using Azure.HabboHotel.Commands;
using Azure.HabboHotel.GameClients;
using Azure.HabboHotel.Groups;
using Azure.HabboHotel.Guides;
using Azure.HabboHotel.Items;
using Azure.HabboHotel.Misc;
using Azure.HabboHotel.Navigators;
using Azure.HabboHotel.Pets;
using Azure.HabboHotel.Polls;
using Azure.HabboHotel.Quests;
using Azure.HabboHotel.Roles;
using Azure.HabboHotel.RoomBots;
using Azure.HabboHotel.Rooms;
using Azure.HabboHotel.SoundMachine;
using Azure.HabboHotel.Support;
using Azure.HabboHotel.Users;
using Azure.HabboHotel.Users.Inventory;
using Azure.Manager;
using Azure.Messages;
using Azure.Security;
using Azure.Security.BlackWords;

#endregion

namespace Azure.HabboHotel
{
    /// <summary>
    /// Class Game.
    /// </summary>
    internal class Game
    {
        /// <summary>
        /// The game loop enabled
        /// </summary>
        internal static bool GameLoopEnabled = true;

        /// <summary>
        /// The client manager cycle ended
        /// </summary>
        internal bool ClientManagerCycleEnded, RoomManagerCycleEnded;

        /// <summary>
        /// The _client manager
        /// </summary>
        private readonly GameClientManager _clientManager;

        /// <summary>
        /// The _ban manager
        /// </summary>
        private readonly ModerationBanManager _banManager;

        /// <summary>
        /// The _role manager
        /// </summary>
        private readonly RoleManager _roleManager;

        /// <summary>
        /// The _catalog
        /// </summary>
        private readonly Catalog _catalog;

        /// <summary>
        /// The _navigator
        /// </summary>
        private readonly Navigator _navigator;

        /// <summary>
        /// The _item manager
        /// </summary>
        private readonly ItemManager _itemManager;

        /// <summary>
        /// The _room manager
        /// </summary>
        private readonly RoomManager _roomManager;

        /// <summary>
        /// The _hotel view
        /// </summary>
        private readonly HotelView _hotelView;

        /// <summary>
        /// The _pixel manager
        /// </summary>
        private readonly CoinsManager _pixelManager;

        /// <summary>
        /// The _achievement manager
        /// </summary>
        private readonly AchievementManager _achievementManager;

        /// <summary>
        /// The _moderation tool
        /// </summary>
        private readonly ModerationTool _moderationTool;

        /// <summary>
        /// The _bot manager
        /// </summary>
        private readonly BotManager _botManager;

        /// <summary>
        /// The _quest manager
        /// </summary>
        private readonly QuestManager _questManager;

        /// <summary>
        /// The _group manager
        /// </summary>
        private readonly GroupManager _groupManager;

        /// <summary>
        /// The _events
        /// </summary>
        private readonly RoomEvents _events;

        /// <summary>
        /// The _talent manager
        /// </summary>
        private readonly TalentManager _talentManager;

        /// <summary>
        /// The _pinata handler
        /// </summary>
        private readonly PinataHandler _pinataHandler;

        /// <summary>
        /// The _clothing manager
        /// </summary>
        private readonly ClothingManager _clothingManager;

        /// <summary>
        /// The _clothing manager
        /// </summary>
        private readonly CrackableEggHandler _crackableEggHandler;

        /// <summary>
        /// The _poll manager
        /// </summary>
        private readonly PollManager _pollManager;

        /// <summary>
        /// The _guide manager
        /// </summary>
        private readonly GuideManager _guideManager;

        private readonly HallOfFame _hallOfFame;

        private readonly TargetedOfferManager _targetedOfferManager;

        /// <summary>
        /// The _game loop
        /// </summary>
        private Task _gameLoop;

        /// <summary>
        /// Initializes a new instance of the <see cref="Game"/> class.
        /// </summary>
        /// <param name="conns">The conns.</param>
        internal Game(int conns)
        {
            Console.WriteLine();
            Out.WriteLine(@"Starting up Azure Emulator for " + Environment.MachineName + "...", @"Azure.Boot");
            Console.WriteLine();

            _clientManager = new GameClientManager();
            using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                AbstractBar bar = new AnimatedBar();
                const int wait = 15, end = 5;

                uint itemsLoaded;
                uint navigatorLoaded;
                uint roomModelLoaded;
                uint achievementLoaded;
                uint pollLoaded;

                Progress(bar, wait, end, "Cleaning dirty in database...");
                DatabaseStartupCleanup(queryReactor);

                Progress(bar, wait, end, "Loading Bans...");
                _banManager = new ModerationBanManager();
                _banManager.LoadBans(queryReactor);

                Progress(bar, wait, end, "Loading Roles...");
                _roleManager = new RoleManager();
                _roleManager.LoadRights(queryReactor);

                Progress(bar, wait, end, "Loading Items...");
                _itemManager = new ItemManager();
                _itemManager.LoadItems(queryReactor, out itemsLoaded);

                Progress(bar, wait, end, "Loading Catalog...");
                _catalog = new Catalog();

                Progress(bar, wait, end, "Loading Targeted Offers...");
                _targetedOfferManager = new TargetedOfferManager();

                Progress(bar, wait, end, "Loading Clothing...");
                _clothingManager = new ClothingManager();
                _clothingManager.Initialize(queryReactor);

                Progress(bar, wait, end, "Loading Rooms...");
                _roomManager = new RoomManager();
                _roomManager.LoadModels(queryReactor, out roomModelLoaded);

                Progress(bar, wait, end, "Loading Navigator...");
                _navigator = new Navigator();
                _navigator.Initialize(queryReactor, out navigatorLoaded);

                Progress(bar, wait, end, "Loading Groups...");
                _groupManager = new GroupManager();
                _groupManager.InitGroups();
                
                Progress(bar, wait, end, "Loading PixelManager...");
                _pixelManager = new CoinsManager();

                Progress(bar, wait, end, "Loading HotelView...");
                _hotelView = new HotelView();

                Progress(bar, wait, end, "Loading Hall Of Fame...");
                _hallOfFame = new HallOfFame();

                Progress(bar, wait, end, "Loading ModerationTool...");
                _moderationTool = new ModerationTool();
                _moderationTool.LoadMessagePresets(queryReactor);
                _moderationTool.LoadPendingTickets(queryReactor);

                Progress(bar, wait, end, "Loading Bots...");
                _botManager = new BotManager();

                Progress(bar, wait, end, "Loading Quests...");
                _questManager = new QuestManager();
                _questManager.Initialize(queryReactor);

                Progress(bar, wait, end, "Loading Events...");
                _events = new RoomEvents();

                Progress(bar, wait, end, "Loading Talents...");
                _talentManager = new TalentManager();
                _talentManager.Initialize(queryReactor);

                //this.SnowStormManager = new SnowStormManager();

                Progress(bar, wait, end, "Loading Pinata...");
                _pinataHandler = new PinataHandler();
                _pinataHandler.Initialize(queryReactor);

                Progress(bar, wait, end, "Loading Crackable Eggs...");
                _crackableEggHandler = new CrackableEggHandler();
                _crackableEggHandler.Initialize(queryReactor);

                Progress(bar, wait, end, "Loading Polls...");
                _pollManager = new PollManager();
                _pollManager.Init(queryReactor, out pollLoaded);

                Progress(bar, wait, end, "Loading Achievements...");
                _achievementManager = new AchievementManager(queryReactor, out achievementLoaded);

                Progress(bar, wait, end, "Loading StaticMessages ...");
                StaticMessagesManager.Load();

                Progress(bar, wait, end, "Loading Guides ...");
                _guideManager = new GuideManager();

                Progress(bar, wait, end, "Loading and Registering Commands...");
                CommandsManager.Register();

                Cache.StartProcess();

                //Progress(bar, wait, end, "Loading AntiMutant...");
                //this.AntiMutant = new AntiMutant();

                Console.Write("\r".PadLeft(Console.WindowWidth - Console.CursorLeft - 1));
            }
        }

        /// <summary>
        /// Gets a value indicating whether [game loop enabled ext].
        /// </summary>
        /// <value><c>true</c> if [game loop enabled ext]; otherwise, <c>false</c>.</value>
        internal bool GameLoopEnabledExt
        {
            get { return GameLoopEnabled; }
        }

        /// <summary>
        /// Gets a value indicating whether [game loop active ext].
        /// </summary>
        /// <value><c>true</c> if [game loop active ext]; otherwise, <c>false</c>.</value>
        internal bool GameLoopActiveExt { get; private set; }

        /// <summary>
        /// Gets the game loop sleep time ext.
        /// </summary>
        /// <value>The game loop sleep time ext.</value>
        internal int GameLoopSleepTimeExt
        {
            get { return 25; }
        }

        /// <summary>
        /// Progresses the specified bar.
        /// </summary>
        /// <param name="bar">The bar.</param>
        /// <param name="wait">The wait.</param>
        /// <param name="end">The end.</param>
        /// <param name="message">The message.</param>
        public static void Progress(AbstractBar bar, int wait, int end, string message)
        {
            bar.PrintMessage(message);
            for (var cont = 0; cont < end; cont++)
                bar.Step();
        }


        private static void DatabaseStartupCleanup(IQueryAdapter dbClient)
        {
            dbClient.RunFastQuery("UPDATE `server_status` SET status = '1', users_online = '0', rooms_loaded = '0', server_ver = 'Azure Emulator', stamp = '" + Azure.GetUnixTimeStamp() + "' LIMIT 1;");
        }

        /// <summary>
        /// Databases the cleanup.
        /// </summary>
        /// <param name="dbClient">The database client.</param>
        internal static void DatabaseShutdownCleanup(IQueryAdapter dbClient)
        {
            dbClient.RunFastQuery("UPDATE users SET online = '0' WHERE online <> '0'");
            dbClient.RunFastQuery("UPDATE rooms_data SET users_now = 0 WHERE users_now <> 0");
            dbClient.RunFastQuery(
                string.Format(
                    "UPDATE server_status SET status = 1, users_online = 0, rooms_loaded = 0, server_ver = 'Mercury Emulator', stamp = '{0}' ",
                    Azure.GetUnixTimeStamp()));
        }

        /*internal AntiMutant GetAntiMutant()
        {
        //return this.AntiMutant;
        }*/

        /// <summary>
        /// Gets the client manager.
        /// </summary>
        /// <returns>GameClientManager.</returns>
        internal GameClientManager GetClientManager()
        {
            return _clientManager;
        }

        /// <summary>
        /// Gets the ban manager.
        /// </summary>
        /// <returns>ModerationBanManager.</returns>
        internal ModerationBanManager GetBanManager()
        {
            return _banManager;
        }

        /// <summary>
        /// Gets the role manager.
        /// </summary>
        /// <returns>RoleManager.</returns>
        internal RoleManager GetRoleManager()
        {
            return _roleManager;
        }

        /// <summary>
        /// Gets the catalog.
        /// </summary>
        /// <returns>Catalog.</returns>
        internal Catalog GetCatalog()
        {
            return _catalog;
        }

        /// <summary>
        /// Gets the room events.
        /// </summary>
        /// <returns>RoomEvents.</returns>
        internal RoomEvents GetRoomEvents()
        {
            return _events;
        }

        /// <summary>
        /// Gets the guide manager.
        /// </summary>
        /// <returns>GuideManager.</returns>
        internal GuideManager GetGuideManager()
        {
            return _guideManager;
        }

        /// <summary>
        /// Gets the navigator.
        /// </summary>
        /// <returns>Navigator.</returns>
        internal Navigator GetNavigator()
        {
            return _navigator;
        }

        /// <summary>
        /// Gets the item manager.
        /// </summary>
        /// <returns>ItemManager.</returns>
        internal ItemManager GetItemManager()
        {
            return _itemManager;
        }

        /// <summary>
        /// Gets the room manager.
        /// </summary>
        /// <returns>RoomManager.</returns>
        internal RoomManager GetRoomManager()
        {
            return _roomManager;
        }

        /// <summary>
        /// Gets the pixel manager.
        /// </summary>
        /// <returns>CoinsManager.</returns>
        internal CoinsManager GetPixelManager()
        {
            return _pixelManager;
        }

        /// <summary>
        /// Gets the hotel view.
        /// </summary>
        /// <returns>HotelView.</returns>
        internal HotelView GetHotelView()
        {
            return _hotelView;
        }

        internal HallOfFame GetHallOfFame()
        {
            return _hallOfFame;
        }

        internal TargetedOfferManager GetTargetedOfferManager()
        {
            return _targetedOfferManager;
        }

        /// <summary>
        /// Gets the achievement manager.
        /// </summary>
        /// <returns>AchievementManager.</returns>
        internal AchievementManager GetAchievementManager()
        {
            return _achievementManager;
        }

        /// <summary>
        /// Gets the moderation tool.
        /// </summary>
        /// <returns>ModerationTool.</returns>
        internal ModerationTool GetModerationTool()
        {
            return _moderationTool;
        }

        /// <summary>
        /// Gets the bot manager.
        /// </summary>
        /// <returns>BotManager.</returns>
        internal BotManager GetBotManager()
        {
            return _botManager;
        }

        /// <summary>
        /// Gets the quest manager.
        /// </summary>
        /// <returns>QuestManager.</returns>
        internal QuestManager GetQuestManager()
        {
            return _questManager;
        }

        /// <summary>
        /// Gets the group manager.
        /// </summary>
        /// <returns>GroupManager.</returns>
        internal GroupManager GetGroupManager()
        {
            return _groupManager;
        }

        /// <summary>
        /// Gets the talent manager.
        /// </summary>
        /// <returns>TalentManager.</returns>
        internal TalentManager GetTalentManager()
        {
            return _talentManager;
        }

        /// <summary>
        /// Gets the pinata handler.
        /// </summary>
        /// <returns>PinataHandler.</returns>
        internal PinataHandler GetPinataHandler()
        {
            return _pinataHandler;
        }

        internal CrackableEggHandler GetCrackableEggHandler()
        {
            return _crackableEggHandler;
        }

        /// <summary>
        /// Gets the poll manager.
        /// </summary>
        /// <returns>PollManager.</returns>
        internal PollManager GetPollManager()
        {
            return _pollManager;
        }

        /// <summary>
        /// Gets the clothing manager.
        /// </summary>
        /// <returns>ClothingManager.</returns>
        internal ClothingManager GetClothingManager()
        {
            return _clothingManager;
        }

        /// <summary>
        /// Continues the loading.
        /// </summary>
        internal void ContinueLoading()
        {
            using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                uint catalogPageLoaded;
                PetRace.Init(queryReactor);
                _catalog.Initialize(queryReactor, out catalogPageLoaded);
                Filter.Load();
                BobbaFilter.InitSwearWord();
                BlackWordsManager.Load();
                SongManager.Initialize();
                LowPriorityWorker.Init(queryReactor);
                _roomManager.InitVotedRooms(queryReactor);
                _roomManager.LoadCompetitionManager();
            }
            StartGameLoop();
            _pixelManager.StartTimer();
        }

        /// <summary>
        /// Starts the game loop.
        /// </summary>
        internal void StartGameLoop()
        {
            GameLoopActiveExt = true;
            _gameLoop = new Task(MainGameLoop);            
            _gameLoop.Start();
        }

        /// <summary>
        /// Stops the game loop.
        /// </summary>
        internal void StopGameLoop()
        {
            GameLoopActiveExt = false;
            while (!RoomManagerCycleEnded || !ClientManagerCycleEnded)
                Thread.Sleep(25);
        }

        /// <summary>
        /// Destroys this instance.
        /// </summary>
        internal void Destroy()
        {
            using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
                DatabaseShutdownCleanup(queryReactor);
            GetClientManager();
            Out.WriteLine("Client Manager destroyed", "Azure.Game", ConsoleColor.DarkYellow);
        }

        /// <summary>
        /// Reloaditemses this instance.
        /// </summary>
        internal void Reloaditems()
        {
            using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                _itemManager.LoadItems(queryReactor);
            }
        }

        /// <summary>
        /// Mains the game loop.
        /// </summary>
        private void MainGameLoop()
        {
            while (GameLoopActiveExt)
            {
                LowPriorityWorker.Process();
                try
                {
                    RoomManagerCycleEnded = false;
                    ClientManagerCycleEnded = false;
                    _roomManager.OnCycle();
                    _clientManager.OnCycle();
                }
                catch (Exception ex)
                {
                    Logging.LogCriticalException(string.Format("Exception in Game Loop!: {0}", ex));
                }
                Thread.Sleep(GameLoopSleepTimeExt);
            }
        }
    }
}