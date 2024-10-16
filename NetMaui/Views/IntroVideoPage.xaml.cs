using MediaManager;
using NetMaui.Views;
using Plugin.Maui.Audio;
using Models.Interfaces;

namespace NetMaui.Views;

public partial class IntroVideoPage : ContentPage
{
    private readonly IAudioManager audioManager;
    public IntroVideoPage(IAudioManager audioManager)
	{
		InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
        this.audioManager = audioManager;
    }

    private async void MediaElement_MediaEnded(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new MainPage(audioManager));
        Navigation.RemovePage(this);
    }
}