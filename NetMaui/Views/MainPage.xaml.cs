using CommunityToolkit.Mvvm.Input;
using MediaManager;
using Plugin.Maui.Audio;

namespace NetMaui.Views;

public partial class MainPage : ContentPage
{
    private IMediaManager mediaManager;
    
    public MainPage(IMediaManager media)
	{
		InitializeComponent();
        this.mediaManager = media;
    }

    private void TBSearch_Clicked(object sender, EventArgs e)
    {

    }

    private void TBFilter_Clicked(object sender, EventArgs e)
    {

    }

    private void TBConfig_Clicked(object sender, EventArgs e)
    {

    }

    private void Play_Clicked(object sender, EventArgs e)
    {
        if (mediaManager.IsPlaying())
            mediaManager.Pause();

        else mediaManager.Play("musicaTeste.mp3");
    }

    private void Next_Clicked(object sender, EventArgs e)
    {

    }

    private void Back_Clicked(object sender, EventArgs e)
    {

    }

}