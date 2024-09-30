using MediaManager;
using NetMaui.Views;
using Plugin.Maui.Audio;

namespace NetMaui;

public partial class IntroVideoPage : ContentPage
{
    private readonly IMediaManager media;
    public IntroVideoPage(IMediaManager media)
	{
		InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
        this.media = media;
    }

    private async void MediaElement_MediaEnded(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new MainPage(media));
        Navigation.RemovePage(this);
    }
}