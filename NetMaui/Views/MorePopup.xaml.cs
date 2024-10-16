using Entities;
using Mopups.Services;
using NetMaui.Views.Playlist;

namespace NetMaui.Views
{
    public partial class MorePopup
    {
        private readonly AudioItem audioItem;
        public MorePopup(AudioItem audioItem)
        {
            InitializeComponent();
            AdjustHeight();
            this.audioItem = audioItem;
        }


        private void AdjustHeight()
        {
            var pageHeight = Application.Current.MainPage.Height * 0.2;

            VSPopup.HeightRequest = pageHeight;
            GPopup.HeightRequest = pageHeight;
        }

        private async void BtnAddPlaylistMusic_Clicked(object sender, EventArgs e)
        {
            MorePopup? currentPopup = MopupService.Instance.PopupStack.LastOrDefault() as MorePopup;

            if (currentPopup != null)
            {
                await currentPopup.TranslateTo(0, 1000, 500, Easing.CubicIn);

                await MopupService.Instance.PopAsync();
            }

            await MopupService.Instance.PushAsync(new PlaylistPopupPage(audioItem));
        }

        private void BtnDelete_Clicked(object sender, EventArgs e)
        {
            if(File.Exists(audioItem.FilePath))
            {
                File.Delete(audioItem.FilePath);
            }
            Navigation.PopToRootAsync();
        }
    }
}
