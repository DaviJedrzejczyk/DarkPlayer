using CommunityToolkit.Mvvm.Input;
using Entities;
using MediaManager;
using Microsoft.Maui.Controls;
using NetMaui.Models.ViewModels;
using Plugin.Maui.Audio;
using Models.Interfaces;
using SkiaSharp;
using System.Diagnostics;
using System.Reflection;
using Entities.Enums;

#if ANDROID
using Java.IO;
#endif
namespace NetMaui.Views
{

    public partial class MainPage : ContentPage
    {
        public MusicPlayerViewModel MusicPlayerViewModel { get; set; }

        private readonly IAudioManager audioManager;
        private double audioDuration = 0;
        private double currentAudioTime = 0;
        private System.Timers.Timer timer;
        private string currentAudioFilePath = string.Empty;
        private AudioItem audioItem = new();
        private EMusicMode eMusicMode;
        private List<AudioItem> songs = [];
        private const string Preference = "MODE";

        public MainPage(IAudioManager audioManager)
        {
            InitializeComponent();
            this.audioManager = audioManager;
            MusicPlayerViewModel = new MusicPlayerViewModel();
            BindingContext = MusicPlayerViewModel;

            if (MusicPlayerViewModel.IsPlaying)
                btnPlay.ImageSource = "pause_24.svg";
            else
                btnPlay.ImageSource = "play_arrow_24.svg";

            eMusicMode = LoadMusicMode();
        }

        protected override async void OnAppearing()
        {
            try
            {
                base.OnAppearing();
                await VerifyPermission();
                //StartLabelScrolling();
                var lastPlayedMusic = Preferences.Get("LastPlayedMusic", string.Empty);

                if (!string.IsNullOrEmpty(lastPlayedMusic))
                    LoadLastPlayedMusic(lastPlayedMusic);

                LoadAudioFiles();

                if(App.AudioPlayerViewModel.AudioItems != null)
                    songs = App.AudioPlayerViewModel.AudioItems;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        async void StartLabelScrolling()
        {

            //O intuito desse codigo é Fazer com que o nome da musica fique passando que nem um carrossel
            //var lblNome = (Label)FindByName("lblNome");
            //var nameContainer = (HorizontalStackLayout)FindByName("nameContainer");
            //while (true)
            //{
            //    double textWidth = lblNome.Measure(double.PositiveInfinity, lblNome.Height).Request.Width;

            //    double containerWidth = nameContainer.Width;

            //    lblNome.TranslationX = containerWidth;

            //    double distance = textWidth + containerWidth;

            //    uint duration = (uint)(distance * 10); 

            //    await lblNome.TranslateTo(textWidth, 0, duration, Easing.Linear);
            //}

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

        private async void Play_Clicked(object sender, EventArgs e)
        {
            try
            {
                await VerifyPermission();
                await PlayMusic(currentAudioFilePath);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        private async Task PlayMusic(string filePath)
        {
            try
            {
                if (App.AudioPlayerViewModel.AudioPlayer == null || currentAudioFilePath != filePath)
                {
                    if (App.AudioPlayerViewModel.AudioPlayer != null)
                    {
                        App.AudioPlayerViewModel.AudioPlayer.Stop();
                        App.AudioPlayerViewModel.AudioPlayer.Dispose();
                        timer?.Stop();
                        timer?.Dispose();
                    }

                    currentAudioFilePath = filePath;
                    App.AudioPlayerViewModel.AudioPlayer = audioManager.CreatePlayer(System.IO.File.OpenRead(currentAudioFilePath));

                    var audioProgressSlider = (Slider)FindByName("audioProgressSlider");
                    audioProgressSlider.Value = 0;
                }

                if (App.AudioPlayerViewModel.AudioPlayer.IsPlaying)
                {
                    btnPlay.ImageSource = "play_arrow_24.svg";
                    App.AudioPlayerViewModel.AudioPlayer.Pause();
                    timer?.Stop();
                }
                else
                {
                    btnPlay.ImageSource = "pause_24.svg";
                    App.AudioPlayerViewModel.AudioPlayer.Play();

                    audioDuration = App.AudioPlayerViewModel.AudioPlayer.Duration;

                    var audioProgressSlider = (Slider)FindByName("audioProgressSlider");
                    if (audioProgressSlider.Value >= audioDuration - 1)
                        audioProgressSlider.Value = 0;

                    audioProgressSlider.Maximum = audioDuration;

                    App.AudioPlayerViewModel.AudioDuration = App.AudioPlayerViewModel.AudioPlayer.Duration;

                    timer = new System.Timers.Timer(1000);
                    timer.Elapsed += Timer_Elapsed;
                    timer.Start();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }


        private void Next_Clicked(object sender, EventArgs e)
        {
            try
            {
                NextMusic();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }

        }

        private async Task VerifyPermission()
        {
            PermissionsViewModel permissionsViewModel = new();
            await permissionsViewModel.RequestPermissions();
        }

        private void ChangeImage(ImageSource source)
        {
            try
            {
                if (source == null)
                    imgMusic.Source = "standart_image.png";

                imgMusic.Source = source;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        private void audioProgressSlider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            try
            {
                if (App.AudioPlayerViewModel.AudioPlayer.IsPlaying)
                {
                    currentAudioTime = e.NewValue;
                    App.AudioPlayerViewModel.CurrentAudioTime = e.NewValue;
                    App.AudioPlayerViewModel.AudioPlayer.Seek(App.AudioPlayerViewModel.CurrentAudioTime);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    if (App.AudioPlayerViewModel.AudioPlayer.IsPlaying)
                    {
                        if (!App.AudioPlayerViewModel.IsInDetailPage)
                            App.AudioPlayerViewModel.CurrentAudioTime += 1;


                        if (App.AudioPlayerViewModel.CurrentAudioTime <= App.AudioPlayerViewModel.AudioDuration)
                        {
                            audioProgressSlider.Value = App.AudioPlayerViewModel.CurrentAudioTime;
                            MusicPlayerViewModel.CurrentAudioTime = App.AudioPlayerViewModel.CurrentAudioTime;
                        }

                        if (App.AudioPlayerViewModel.CurrentAudioTime >= App.AudioPlayerViewModel.AudioDuration && eMusicMode == EMusicMode.REPLAY_UNIQUE)
                        {
                            App.AudioPlayerViewModel.CurrentAudioTime = 0;
                            audioProgressSlider.Value = App.AudioPlayerViewModel.CurrentAudioTime;
                            MusicPlayerViewModel.CurrentAudioTime = App.AudioPlayerViewModel.CurrentAudioTime;
                        }
                    }
                    else if(App.AudioPlayerViewModel.CurrentAudioTime >= App.AudioPlayerViewModel.AudioDuration)
                    {
                        NextMusic();
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message, ex);
                }
            });
        }

        private async void NextMusic()
        {
            try
            {
                var actualMusic = songs.FindIndex(m => m.FilePath == currentAudioFilePath) + 1;

                if (actualMusic >= songs.Count)
                    actualMusic = 0;

                var nextMusic = songs[actualMusic];

                if (nextMusic != null)
                {
                    lblNome.Text = nextMusic.Name;
                    await PlayMusic(nextMusic.FilePath);
                    ChangeImage(nextMusic.AlbumArt);
                    UpdateBackgroundBasedOnImage(nextMusic);
                    Preferences.Set("LastPlayedMusic", nextMusic.FilePath);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        private void LoadAudioFiles()
        {
            try
            {
                var audioFiles = audioItem.LoadMusicsFromDirectory("/storage/emulated/0/snaptube/Download/SnapTube Audio/");
                MusicPlayerViewModel.AudioItems = audioFiles;
                AudioListView.ItemsSource = audioFiles;
                songs = MusicPlayerViewModel.AudioItems;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        private void LoadLastPlayedMusic(string lastPlayedMusic)
        {
            try
            {

                if (System.IO.File.Exists(lastPlayedMusic))
                {
                    audioItem = audioItem.LoadLastMusic(lastPlayedMusic);
                    lblNome.Text = audioItem.Name;
                    currentAudioFilePath = audioItem.FilePath;
                    ChangeImage(audioItem.AlbumArt);
                    UpdateBackgroundBasedOnImage(audioItem);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        private async void AudioListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            try
            {
                if (e.Item == null)
                    return;

                var audioItem = e.Item as AudioItem;

                string musicFolderPath = "/storage/emulated/0/snaptube/Download/SnapTube Audio/";

                string audioFilePath = Path.Combine(musicFolderPath, audioItem.Name + ".mp3");

                string lastPlayed = Preferences.Get("LastPlayedMusic", string.Empty);

                if (System.IO.File.Exists(audioFilePath))
                {
                    if (!MusicPlayerViewModel.IsPlaying || (MusicPlayerViewModel.IsPlaying && !lastPlayed.Equals(audioFilePath)))
                    {
                        lblNome.Text = audioItem.Name;
                        await PlayMusic(audioFilePath);
                        ChangeImage(audioItem.AlbumArt);
                        UpdateBackgroundBasedOnImage(audioItem);
                        Preferences.Set("LastPlayedMusic", audioFilePath);
                        MusicPlayerViewModel.IsPlaying = true;

                        if (!App.AudioPlayerViewModel.IsInDetailPage)
                            App.AudioPlayerViewModel.IsInDetailPage = true;

                        else if (MusicPlayerViewModel.EMusicMode == EMusicMode.REPLAY_UNIQUE)
                            App.AudioPlayerViewModel.AudioPlayer.Loop = true;
                    }

                }
                ((ListView)sender).SelectedItem = null;
                await Navigation.PushAsync(new DetailMusicPage(audioItem, audioManager, MusicPlayerViewModel));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        private async void UpdateBackgroundBasedOnImage(AudioItem selectedAudioItem)
        {
            var dominantColor = await GetDominantColor(selectedAudioItem.AlbumArt);

            border.BackgroundColor = dominantColor;
            verticalStack.BackgroundColor = dominantColor;
        }

        public async Task<Color> GetDominantColor(ImageSource imageSource)
        {
            try
            {
                if (imageSource is StreamImageSource streamImageSource)
                {
                    var stream = await streamImageSource.Stream(CancellationToken.None);
                    if (stream == null) return Color.FromArgb("#151229");

                    using (var bitmap = SKBitmap.Decode(stream))
                    {
                        MakeBackgroundTransparent(bitmap);
                        return GetDominantColorFromBitmap(bitmap);
                    }
                }

                return Color.FromArgb("#151229");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        private void MakeBackgroundTransparent(SKBitmap bitmap)
        {
            try
            {
                SKColor targetColor = SKColors.White;

                for (int x = 0; x < bitmap.Width; x++)
                {
                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        var pixel = bitmap.GetPixel(x, y);

                        if (pixel == targetColor)
                            bitmap.SetPixel(x, y, SKColors.Transparent);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        private Color GetDominantColorFromBitmap(SKBitmap bitmap)
        {
            try
            {
                var colorCounts = new Dictionary<string, int>();

                for (int y = 0; y < bitmap.Height; y++)
                {
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        var color = bitmap.GetPixel(x, y);
                        var colorKey = $"{color.Red},{color.Green},{color.Blue}";

                        if (!colorCounts.ContainsKey(colorKey))
                        {
                            colorCounts[colorKey] = 0;
                        }
                        colorCounts[colorKey]++;
                    }
                }

                var dominantColor = colorCounts.OrderByDescending(c => c.Value).FirstOrDefault();
                var rgb = dominantColor.Key.Split(',').Select(int.Parse).ToArray();

                return Color.FromRgb(rgb[0], rgb[1], rgb[2]);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        private EMusicMode LoadMusicMode()
        {
            string musicModeStr = Preferences.Get(Preference, EMusicMode.CONTINUOUS.ToString());
            if (Enum.TryParse(musicModeStr, out EMusicMode musicMode))
                return musicMode;

            return EMusicMode.CONTINUOUS;
        }
    }
}