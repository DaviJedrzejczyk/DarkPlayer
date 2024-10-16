using MediaManager;
using NetMaui.Views;
using Plugin.Maui.Audio;
using Models.Interfaces;

namespace NetMaui
{
    public partial class App : Application
    {
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
