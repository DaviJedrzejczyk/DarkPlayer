using Entities;
using Models.Impl;
using Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NetMaui.Models.ViewModels
{
    public class PlaylistViewModel : BindableObject
    {
        private ObservableCollection<Playlist> playlists;
        public ObservableCollection<Playlist> Playlists
        {
            get => playlists;
            set
            {
                playlists = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<int> musicCount;
        public ObservableCollection<int> MusicCount
        {
            get => musicCount;
            set
            {
                musicCount = value;
                OnPropertyChanged();
            }
        }

        private readonly IPlaylistService playlistService;

        public PlaylistViewModel()
        {
            this.playlistService = new PlaylistService();
            Playlists = new ObservableCollection<Playlist>();
            MusicCount = new ObservableCollection<int>();
        }

        public async Task LoadPlaylists()
        {
            Playlists.Clear();

            var playlistsFromService = await playlistService.LoadAllPlaylists();

            foreach (var playlist in playlistsFromService)
            {
                MusicCount.Add(playlist.Musics.Count);
                Playlists.Add(playlist);
            }
        }
    }
}
