#region

using System;
using System.Threading;
using Azure.Database.Manager.Database.Session_Details.Interfaces;

#endregion

namespace Azure.HabboHotel.Misc
{
    /// <summary>
    /// Class LowPriorityWorker.
    /// </summary>
    internal class LowPriorityWorker
    {
        /// <summary>
        /// The _user peak
        /// </summary>
        private static int _userPeak;

        private static string _lastDate;

        /// <summary>
        /// The _m timer
        /// </summary>
        private static Timer _mTimer;

        /// <summary>
        /// Initializes the specified database client.
        /// </summary>
        /// <param name="dbClient">The database client.</param>
        internal static void Init(IQueryAdapter dbClient)
        {
            dbClient.SetQuery("SELECT userpeak FROM server_status");
            _userPeak = dbClient.GetInteger();
        }

        /// <summary>
        /// Starts the processing.
        /// </summary>
        internal static void StartProcessing()
        {
            _mTimer = new Timer(Process, null, 0, 60000);
        }

        /// <summary>
        /// Processes the specified caller.
        /// </summary>
        /// <param name="caller">The caller.</param>
        internal static void Process(object caller)
        {
            try
            {
                var clientCount = Azure.GetGame().GetClientManager().ClientCount();
                var loadedRoomsCount = Azure.GetGame().GetRoomManager().LoadedRoomsCount;
                var dateTime = new DateTime((DateTime.Now - Azure.ServerStarted).Ticks);

                Console.Title = string.Concat("AzureEmulator v" + Azure.Version + "." + Azure.Build + " | TIME: ",
                    int.Parse(dateTime.ToString("dd")) - 1 + dateTime.ToString(":HH:mm:ss"), " | ONLINE COUNT: ",
                    clientCount, " | ROOM COUNT: ", loadedRoomsCount);
                using (var queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
                {
                    if (clientCount > _userPeak)
                        _userPeak = clientCount;

                    _lastDate = DateTime.Now.ToShortDateString();
                    queryReactor.RunFastQuery(string.Concat("UPDATE server_status SET stamp = '",
                        Azure.GetUnixTimeStamp(), "', users_online = ", clientCount, ", rooms_loaded = ",
                        loadedRoomsCount, ", server_ver = 'Azure Emulator', userpeak = ", _userPeak));
                }

                Azure.GetGame().GetNavigator().LoadNewPublicRooms();
            }
            catch (Exception e)
            {
                Writer.Writer.LogException(e.ToString());
            }
        }
    }
}