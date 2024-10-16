using Entities;
using Models.Interfaces;

namespace Models.Impl
{
    public class FavoriteService : IFavoriteService
    {
        private const string FavoritesKey = "favorites";
        private const string PlaylistName = "Favorites";
        private readonly IPlaylistService playlistService;

        public FavoriteService()
        {
            playlistService = new PlaylistService();
        }

        public async void AddFavorite(string path)
        {
            var favorites = LoadFavorites();
            await VerifyIfExistsFavoritesPlaylist(path);

            if (!favorites.Contains(path))
            {
                favorites.Add(path);
                Preferences.Set(FavoritesKey, string.Join(",", favorites));
            }
        }

        public void RemoveFavorite(string path)
        {
            var favorites = LoadFavorites();
            if (favorites.Contains(path))
            {
                favorites.Remove(path);
                Preferences.Set(FavoritesKey, string.Join(",", favorites));
            }
        }

        public List<string> LoadFavorites()
        {
            var favoritesStr = Preferences.Get(FavoritesKey, string.Empty);
            return [.. favoritesStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)];
        }

        public bool IsFavorite(string path)
        {
            var favorites = LoadFavorites();
            return favorites.Contains(path);
        }

        private async Task VerifyIfExistsFavoritesPlaylist(string path)
        {
            List<Playlist> playlists = await playlistService.LoadAllPlaylists();

            if (playlists.Where(f => f.Name == PlaylistName).Any())
                await playlistService.AddToPlaylist(PlaylistName, path);
            else
                await playlistService.CreateAndSaveNewPlaylist(PlaylistName, path);
        }
    }
}
