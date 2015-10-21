#region

using System.Collections.Generic;
using System.Linq;
using Azure.Database.Manager.Database.Session_Details.Interfaces;
using Azure.HabboHotel.Items.Interactions.Enums;
using Azure.HabboHotel.Items.Interfaces;
using Azure.HabboHotel.Rooms;
using Azure.HabboHotel.SoundMachine;
using Azure.HabboHotel.SoundMachine.Composers;
using Azure.Messages.Parsers;

#endregion

namespace Azure.Messages.Handlers
{
    /// <summary>
    /// Class GameClientMessageHandler.
    /// </summary>
    internal partial class GameClientMessageHandler
    {
        /// <summary>
        /// Retrieves the song identifier.
        /// </summary>
        internal void RetrieveSongId()
        {
            string text = Request.GetString();
            uint songId = SongManager.GetSongId(text);
            if (songId != 0u)
            {
                var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("RetrieveSongIDMessageComposer"));
                serverMessage.AppendString(text);
                serverMessage.AppendInteger(songId);
                Session.SendMessage(serverMessage);
            }
        }

        /// <summary>
        /// Gets the music data.
        /// </summary>
        internal void GetMusicData()
        {
            int num = Request.GetInteger();
            var list = new List<SongData>();

            {
                for (int i = 0; i < num; i++)
                {
                    SongData song = SongManager.GetSong(Request.GetUInteger());
                    if (song != null)
                        list.Add(song);
                }
                Session.SendMessage(JukeboxComposer.Compose(list));
                list.Clear();
            }
        }

        /// <summary>
        /// Adds the playlist item.
        /// </summary>
        internal void AddPlaylistItem()
        {
            if (Session == null || Session.GetHabbo() == null || Session.GetHabbo().CurrentRoom == null)
                return;
            Room currentRoom = Session.GetHabbo().CurrentRoom;
            if (!currentRoom.CheckRights(Session, true))
                return;
            RoomMusicController roomMusicController = currentRoom.GetRoomMusicController();
            if (roomMusicController.PlaylistSize >= roomMusicController.PlaylistCapacity)
                return;
            uint num = Request.GetUInteger();
            UserItem item = Session.GetHabbo().GetInventoryComponent().GetItem(num);
            if (item == null || item.BaseItem.InteractionType != Interaction.MusicDisc)
                return;
            var songItem = new SongItem(item);
            int num2 = roomMusicController.AddDisk(songItem);
            if (num2 < 0)
                return;
            songItem.SaveToDatabase(currentRoom.RoomId);
            Session.GetHabbo().GetInventoryComponent().RemoveItem(num, true);
            using (IQueryAdapter queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
                queryReactor.RunFastQuery(string.Format("UPDATE items_rooms SET user_id='0' WHERE id={0} LIMIT 1", num));
            Session.SendMessage(JukeboxComposer.Compose(roomMusicController.PlaylistCapacity, roomMusicController.Playlist.Values.ToList()));
        }

        /// <summary>
        /// Removes the playlist item.
        /// </summary>
        internal void RemovePlaylistItem()
        {
            if (Session == null || Session.GetHabbo() == null || Session.GetHabbo().CurrentRoom == null)
                return;
            Room currentRoom = Session.GetHabbo().CurrentRoom;
            if (!currentRoom.GotMusicController())
                return;
            RoomMusicController roomMusicController = currentRoom.GetRoomMusicController();
            SongItem songItem = roomMusicController.RemoveDisk(Request.GetInteger());
            if (songItem == null)
                return;
            songItem.RemoveFromDatabase();
            Session.GetHabbo().GetInventoryComponent().AddNewItem(songItem.ItemId, songItem.BaseItem.ItemId, songItem.ExtraData, 0u, false, true, 0, 0, songItem.SongCode);
            Session.GetHabbo().GetInventoryComponent().UpdateItems(false);
            using (IQueryAdapter queryReactor = Azure.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.RunFastQuery(string.Format("UPDATE items_rooms SET user_id='{0}' WHERE id='{1}' LIMIT 1;", Session.GetHabbo().Id, songItem.ItemId));
            }
            Session.SendMessage(JukeboxComposer.SerializeSongInventory(Session.GetHabbo().GetInventoryComponent().SongDisks));
            Session.SendMessage(JukeboxComposer.Compose(roomMusicController.PlaylistCapacity, roomMusicController.Playlist.Values.ToList()));
        }

        /// <summary>
        /// Gets the disks.
        /// </summary>
        internal void GetDisks()
        {
            if (Session == null || Session.GetHabbo() == null || Session.GetHabbo().GetInventoryComponent() == null)
                return;
            if (Session.GetHabbo().GetInventoryComponent().SongDisks.Count == 0)
                return;
            Session.SendMessage(JukeboxComposer.SerializeSongInventory(Session.GetHabbo().GetInventoryComponent().SongDisks));
        }

        /// <summary>
        /// Gets the playlists.
        /// </summary>
        internal void GetPlaylists()
        {
            if (Session == null || Session.GetHabbo() == null || Session.GetHabbo().CurrentRoom == null)
                return;
            Room currentRoom = Session.GetHabbo().CurrentRoom;
            if (!currentRoom.GotMusicController())
                return;
            RoomMusicController roomMusicController = currentRoom.GetRoomMusicController();
            Session.SendMessage(JukeboxComposer.Compose(roomMusicController.PlaylistCapacity, roomMusicController.Playlist.Values.ToList()));
        }
    }
}