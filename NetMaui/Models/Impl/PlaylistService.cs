using Entities;
using Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Models.Impl
{
    public class PlaylistService : IPlaylistService
    {
        private string filePath = Path.Combine(FileSystem.AppDataDirectory, "playlists.json");

        public async Task AddToPlaylist(string playlistName, string musicPath)
        {
            var playlists = await LoadAllPlaylists();

            var playlist = playlists.FirstOrDefault(p => p.Name == playlistName);

            if (playlist != null)
            {
                if (!playlist.Musics.Contains(musicPath))
                {
                    playlist.Musics.Add(musicPath);

                    await SavePlaylistsAsync(playlists);
                }
            }
        }

        public async Task SavePlaylistsAsync(List<Playlist> playlist)
        {
            var json = JsonSerializer.Serialize(playlist);
            await File.WriteAllTextAsync(filePath, json);
        }

        public async Task DeletePlaylist(string namePlaylist)
        {
            List<Playlist> playlists = await LoadAllPlaylists();

            var playlistToRemove = playlists.FirstOrDefault(p => p.Name == namePlaylist);

            if (playlistToRemove != null)
            {
                playlists.Remove(playlistToRemove);

                await SavePlaylistsAsync(playlists);
            }
        }

        public async Task<List<Playlist>> LoadAllPlaylists()
        {
            if (!File.Exists(filePath))
                return [];

            var json = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<List<Playlist>>(json);
        }

        public async Task RemoveFromPlaylist(string namePlaylist, string musicPath)
        {
            var playlists = await LoadAllPlaylists();

            var playlist = playlists.FirstOrDefault(p => p.Name == namePlaylist);

            if (playlist != null)
            {
                if (playlist.Musics.Contains(musicPath))
                {
                    playlist.Musics.Remove(musicPath);

                    await SavePlaylistsAsync(playlists);
                }
            }
        }

        public async Task CreateAndSaveNewPlaylist(string playlistName, string musicPath)
        {
            var playlists = await LoadAllPlaylists();

            if (playlists.Any(p => p.Name == playlistName))
                return;

            var music = new List<string>
            {
                musicPath
            };


            var newPlaylist = new Playlist
            {
                Name = playlistName,
                Musics = music,
            };

            playlists.Add(newPlaylist);

            await SavePlaylistsAsync(playlists);
        }
    }
}
