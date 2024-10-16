using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Plugin.Maui.Audio;
using MediaManager;
using CommunityToolkit.Maui.Core;
using Mopups.Hosting;
using NetMaui.Views;
using NetMaui.Views.Playlist;
using Models.Interfaces;
using Models.Impl;

namespace NetMaui
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkitCore()
                .UseMauiCommunityToolkitMediaElement()
                .UseMauiCommunityToolkit()
                .ConfigureMopups()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
            builder.Services.AddSingleton(AudioManager.Current);
            builder.Services.AddSingleton<IAudioManager, AudioManager>();
            builder.Services.AddTransient<IFavoriteService, FavoriteService>();
#endif

            return builder.Build();
        }
    }
}
