using Entities;
using Mopups.Services;
using Models.Interfaces;
using Models.Impl;
using NetMaui.Models.ViewModels;
using Microsoft.Maui.Controls.Shapes;

namespace NetMaui.Views.Playlist
{

    public partial class PlaylistPopupPage
    {
        private readonly IPlaylistService playlistService;
        private AudioItem audioItem;
        private readonly PlaylistViewModel viewModel;


        public PlaylistPopupPage(AudioItem audioItem)
        {
            InitializeComponent();
            AdjustHeight();
            this.playlistService = new PlaylistService();
            this.audioItem = audioItem;

            viewModel = new PlaylistViewModel();

            BindingContext = viewModel;

            Task.Run(async () => await viewModel.LoadPlaylists());

        }
        private void AdjustHeight()
        {
            var pageHeight = Application.Current.MainPage.Height * 0.3;

            FLPopup.HeightRequest = pageHeight;
            GPopup.HeightRequest = pageHeight;
            BPopup.HeightRequest = pageHeight;

            FLPopup.MaximumHeightRequest = pageHeight;
            GPopup.MaximumHeightRequest = pageHeight;
            BPopup.MaximumHeightRequest = pageHeight;
        }

        private async void BtnAddNewPlaylist_Clicked(object sender, EventArgs e)
        {
            await MopupService.Instance.PushAsync(new PlaylistPopupCreatePage(audioItem));
        }

        private async void BtnAddToPlaylist_Clicked(object sender, EventArgs e)
        {
            
        }

        private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
        {
            var view = sender as View;

            Entities.Playlist selectedPlaylist = view?.BindingContext as Entities.Playlist;

            if(selectedPlaylist != null)
            {
                playlistService.AddToPlaylist(selectedPlaylist.Name, audioItem.FilePath);
            }

        }
    }
}