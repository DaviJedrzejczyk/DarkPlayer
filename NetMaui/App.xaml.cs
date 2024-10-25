using MediaManager;
using NetMaui.Views;
using Plugin.Maui.Audio;
using Models.Interfaces;
using NetMaui.Models.ViewModels;

namespace NetMaui
{
    public partial class App : Application
    {
        public static AudioPlayerViewModel AudioPlayerViewModel { get; } = new AudioPlayerViewModel();
        public App(IAudioManager audioManager)
        {
            InitializeComponent();

            MainPage = new NavigationPage(new IntroVideoPage(audioManager)) 
            {
                BarBackgroundColor = Color.FromArgb("#1E1D1D"),
                BarTextColor = Colors.White
            };
        }
    }
}
