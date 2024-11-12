using CommunityToolkit.Maui.Core.Platform;
using Entities;

namespace NetMaui.Views
{
    public partial class SearchPage : ContentPage
    {
        private double _totalY;
        private double _translationY = 0;
        private const double SwipeDownThreshold = 100;
        private List<AudioItem> AudioItems;
        private List<AudioItem> FilteredAudioItems;
        public SearchPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            this.AudioItems = App.AudioPlayerViewModel.AudioItems;
        }

        private void PanGestureRecognizer_PanUpdated(object sender, PanUpdatedEventArgs e)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    _translationY = this.TranslationY;
                    _totalY = 0;
                    break;

                case GestureStatus.Running:
                    if (e.TotalY > 0)
                    {
                        this.TranslationY = _translationY + e.TotalY;
                        _totalY = e.TotalY;
                    }
                    break;

                case GestureStatus.Completed:
                case GestureStatus.Canceled:
                    if (_totalY > SwipeDownThreshold)
                        ClosePageWithAnimation();
                    else
                        ResetPagePosition();
                    break;
            }
        }

        private async void ClosePageWithAnimation()
        {
            await this.TranslateTo(0, Application.Current.MainPage.Height, 100, Easing.Linear);
            await Navigation.PopModalAsync();
        }

        private async void ResetPagePosition()
        {
            await this.TranslateTo(0, 0, 100, Easing.Linear);
        }

        private async void BtnArrowBack_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }

        private void BtnSearch_Clicked(object sender, EventArgs e)
        {
            string searchText = TxtSearch.Text?.ToLower();
            
            FilteredAudioItems = AudioItems.Where(item => item.Name.ToLower().Contains(searchText)).ToList();

            //Adicionar ListView para preencher com os dados filtrados
        }

        private async void OnTapOutside(object sender, EventArgs e)
        {
            if (TxtSearch.IsFocused)
            {
                TxtSearch.Unfocus();
                await TxtSearch.HideKeyboardAsync(default);
            }
        }
    }
}