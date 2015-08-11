#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Azure.Database.Manager.Database.Session_Details.Interfaces;

#endregion

namespace Azure.HabboHotel.SoundMachine
{
    /// <summary>
    /// Class SongManager.
    /// </summary>
    internal class SongManager
    {
        /// <summary>
        /// The songs
        /// </summary>
        internal static Dictionary<uint, SongData> Songs;

        /// <summary>
        /// The _cache timer
        /// </summary>
        private static Dictionary<uint, double> _cacheTimer;

        /// <summary>
        /// Gets the song identifier.
        /// </summary>
        /// <param name="codeName">Name of the code.</param>
        /// <returns>System.UInt32.</returns>
        internal static uint GetSongId(string codeName)
        {
            return (from current in Songs.Values where current.CodeName == codeName select current.Id).FirstOrDefault();
        }

        /// <summary>
        /// Gets the song.
        /// </summary>
        /// <param name="codeName">Name of the code.</param>
        /// <returns>SongData.</returns>
        internal static SongData GetSong(string codeName)
        {
            return Songs.Values.FirstOrDefault(current => current.CodeName == codeName);
        }

        /// <summary>
        /// Gets the song by identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>SongData.</returns>
        internal static SongData GetSongById(uint id)
        {
            return Songs.Values.FirstOrDefault(current => current.Id == id);
        }

        /// <summary>
        /// Gets the code by identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>String.</returns>
        internal static String GetCodeById(uint id)
        {
            return (from current in Songs.Values where current.Id == id select current.CodeName).FirstOrDefault();
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        internal static void Initialize()
        {
            Songs = new Dictionary<uint, SongData>();
            _cacheTimer = new Dictionary<uint, double>();
            DataTable table;

            Songs.Clear();
            using (IQueryAdapter queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery("SELECT * FROM items_songs_data ORDER BY id");
                table = queryReactor.GetTable();
            }
            foreach (SongData songFromDataRow in from DataRow dRow in table.Rows select GetSongFromDataRow(dRow))
            {
                Songs.Add(songFromDataRow.Id, songFromDataRow);
            }
        }

        /// <summary>
        /// Processes the thread.
        /// </summary>
        internal static void ProcessThread()
        {
            double num = Azure.GetUnixTimeStamp();
            List<uint> list = (from current in _cacheTimer where num - current.Value >= 180.0 select current.Key).ToList();
            foreach (uint current2 in list)
            {
                Songs.Remove(current2);
                _cacheTimer.Remove(current2);
            }
        }

        /// <summary>
        /// Gets the song from data row.
        /// </summary>
        /// <param name="dRow">The d row.</param>
        /// <returns>SongData.</returns>
        internal static SongData GetSongFromDataRow(DataRow dRow)
        {
            return new SongData(Convert.ToUInt32(dRow["id"]), dRow["codename"].ToString(), (string)dRow["name"],
                (string)dRow["artist"], (string)dRow["song_data"], (double)dRow["length"]);
        }

        /// <summary>
        /// Gets the song.
        /// </summary>
        /// <param name="songId">The song identifier.</param>
        /// <returns>SongData.</returns>
        internal static SongData GetSong(uint songId)
        {
            SongData result;
            Songs.TryGetValue(songId, out result);
            return result;
        }
    }
}