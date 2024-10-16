using Entities;
using Mopups.Services;
using Models.Interfaces;
using Models.Impl;

namespace NetMaui.Views.Playlist
{
    public partial class PlaylistPopupCreatePage
    {
        private AudioItem audioItem;
        private IPlaylistService playlistService;
        public PlaylistPopupCreatePage(AudioItem audio)
        {
            InitializeComponent();
            this.audioItem = audio;
            BtnCreate.IsEnabled = false;
            LblCount.Text = "0/200";

            this.playlistService = new PlaylistService();
        }

        private async void TapGridBackground_Tapped(object sender, TappedEventArgs e)
        {
            await MopupService.Instance.PopAsync();
        }

        private void PlaylistNameEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(PlaylistNameEntry.Text))
            {
                int maxLength = 200;
                BtnCreate.IsEnabled = true;
                
                if (e.NewTextValue.Length > maxLength)
                {
                    PlaylistNameEntry.Text = e.OldTextValue;
                }
                LblCount.Text = e.NewTextValue.Length + "/200";
                
            } 
         
            else
                BtnCreate.IsEnabled = false;
        }

        private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
        {
            //Favor não apagar esse método
        }

        private void BtnCancel_Clicked(object sender, EventArgs e)
        {
            MopupService.Instance.PopAsync();
        }

        private async void BtnCreate_Clicked(object sender, EventArgs e)
        {
            await playlistService.CreateAndSaveNewPlaylist(PlaylistNameEntry.Text, audioItem.FilePath);
            await MopupService.Instance.PopAsync();
        }
    }
}