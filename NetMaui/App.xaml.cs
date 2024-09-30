using MediaManager;
using Plugin.Maui.Audio;

namespace NetMaui
{
    public partial class App : Application
    {
        public App(IMediaManager mediaManager)
        {
            InitializeComponent();

            MainPage = new NavigationPage(new IntroVideoPage(mediaManager)) 
            {
                BarBackgroundColor = Color.FromArgb("#1E1D1D"),
                BarTextColor = Colors.White
            };
        }
    }
}
