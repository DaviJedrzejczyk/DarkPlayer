using Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Interfaces
{
    public interface IPlaylistService
    {
        Task SavePlaylistsAsync(List<Playlist> playlist);
        Task DeletePlaylist(string namePlaylist);
        Task<List<Playlist>> LoadAllPlaylists();
        Task AddToPlaylist(string playlistName, string musicPath);
        Task RemoveFromPlaylist(string namePlaylist, string musicPath);
        Task CreateAndSaveNewPlaylist(string playlistName, string musicPath);
    }
}
