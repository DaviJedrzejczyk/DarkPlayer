using Entities;
using Plugin.Maui.Audio;

namespace NetMaui.Views
{
    public partial class DetailMusicPage : ContentPage
    {
        private AudioItem audioItem;
        private IAudioManager audioManager;
        private IAudioPlayer player;
        private double currentAudioTime = 0;
        private double _translationY = 0;
        const double SwipeDownThreshold = 10;

        public DetailMusicPage(AudioItem audioItem, IAudioManager audioManager)
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);

            this.audioItem = audioItem;

            imgMusic.Source = this.audioItem.AlbumArt;
            lblName.Text = this.audioItem.Name;
            lblAuthor.Text = this.audioItem.Author;

            this.audioManager = audioManager;
        }

        private void audioProgressSlider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            currentAudioTime = e.NewValue;
            player.Seek(currentAudioTime);
        }

        private async void BtnBack_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private void PanGestureRecognizer_PanUpdated(object sender, PanUpdatedEventArgs e)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    _translationY = this.TranslationY;
                    break;

                case GestureStatus.Running:
                    if (e.TotalY > 0)
                    {
                        this.TranslationY = _translationY + e.TotalY;
                    }
                    break;

                case GestureStatus.Completed:
                case GestureStatus.Canceled:
                    if (e.TotalY > SwipeDownThreshold)
                    {
                        ClosePageWithAnimation();
                    }
                    else
                    {
                        ResetPagePosition();
                    }
                    break;
            }
        }

        private async void ClosePageWithAnimation()
        {
            await this.TranslateTo(0, Application.Current.MainPage.Height, 100, Easing.Linear);
            await Navigation.PopAsync();
        }

        private async void ResetPagePosition()
        {
            await this.TranslateTo(0, 0, 100, Easing.Linear);
        }

    }

}
