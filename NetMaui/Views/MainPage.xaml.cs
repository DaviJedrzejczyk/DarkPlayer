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
        private IAudioPlayer player;
        private double audioDuration = 0;
        private double currentAudioTime = 0;
        private System.Timers.Timer timer;
        private string currentAudioFilePath = string.Empty;
        private AudioItem audioItem = new();
        private List<AudioItem> songs = new();
        private EMusicMode eMusicMode;
        private const string Preference = "MODE";

        public MainPage(IAudioManager audioManager)
        {
            InitializeComponent();
            this.audioManager = audioManager;
            MusicPlayerViewModel = new MusicPlayerViewModel();
            BindingContext = MusicPlayerViewModel;
            LoadAudioFiles();

            if (MusicPlayerViewModel.IsPlaying)
            {
                btnPlay.ImageSource = "pause_24.svg";
            }
            else
                btnPlay.ImageSource = "play_arrow_24.svg";

            songs = App.AudioPlayerViewModel.AudioItems;
            eMusicMode = LoadMusicMode();
        }

        protected override async void OnAppearing()
        {
            try
            {
                base.OnAppearing();
                await VerifyPermission();
                StartLabelScrolling();
                var lastPlayedMusic = Preferences.Get("LastPlayedMusic", string.Empty);

                if (!string.IsNullOrEmpty(lastPlayedMusic))
                    LoadLastPlayedMusic(lastPlayedMusic);
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
                if (player == null || currentAudioFilePath != filePath)
                {
                    if (player != null)
                    {
                        player.Stop();
                        player.Dispose();
                        timer?.Stop();
                        timer?.Dispose();
                    }

                    currentAudioFilePath = filePath;
                    player = audioManager.CreatePlayer(System.IO.File.OpenRead(currentAudioFilePath));

                    var audioProgressSlider = (Slider)FindByName("audioProgressSlider");
                    audioProgressSlider.Value = 0;
                }

                if (player.IsPlaying)
                {
                    btnPlay.ImageSource = "play_arrow_24.svg";
                    player.Pause();
                    timer?.Stop();
                }
                else
                {
                    btnPlay.ImageSource = "pause_24.svg";
                    player.Play();

                    audioDuration = player.Duration;

                    var audioProgressSlider = (Slider)FindByName("audioProgressSlider");
                    if (audioProgressSlider.Value >= player.Duration - 1)
                        audioProgressSlider.Value = 0;

                    audioProgressSlider.Maximum = audioDuration;

                    App.AudioPlayerViewModel.AudioPlayer = player;
                    App.AudioPlayerViewModel.AudioDuration = player.Duration;

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
                var imgMusic = (Image)FindByName("imgMusic");

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
                if (player.IsPlaying)
                {
                    currentAudioTime = e.NewValue;
                    App.AudioPlayerViewModel.CurrentAudioTime = e.NewValue;
                    player.Seek(App.AudioPlayerViewModel.CurrentAudioTime);
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
                    if (player.IsPlaying)
                    {
                        App.AudioPlayerViewModel.CurrentAudioTime += 1;


                        if (App.AudioPlayerViewModel.CurrentAudioTime <= App.AudioPlayerViewModel.AudioDuration)
                        {
                            audioProgressSlider.Value = App.AudioPlayerViewModel.CurrentAudioTime;
                            MusicPlayerViewModel.CurrentAudioTime = App.AudioPlayerViewModel.CurrentAudioTime;
                        }
                    }
                    else
                    {
                        if (App.AudioPlayerViewModel.CurrentAudioTime >= App.AudioPlayerViewModel.AudioDuration)
                        {
                            if (eMusicMode != EMusicMode.REPLAY_UNIQUE)
                                NextMusic();
                            else
                            {
                                App.AudioPlayerViewModel.CurrentAudioTime = 0;
                                audioProgressSlider.Value = App.AudioPlayerViewModel.CurrentAudioTime;
                                MusicPlayerViewModel.CurrentAudioTime = App.AudioPlayerViewModel.CurrentAudioTime;

                                player.Seek(0);
                                player.Play();
                            }
                        }
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
                if(eMusicMode == EMusicMode.CONTINUOUS)
                    songs = MusicPlayerViewModel.AudioItems;

                var actualMusic = songs.FindIndex(m => m.FilePath == currentAudioFilePath) + 1;

                var nextMusic = songs[actualMusic];

                if (nextMusic != null)
                {
                    lblNome.Text = nextMusic.Name;
                    await PlayMusic(nextMusic.FilePath);
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

                        if (MusicPlayerViewModel.EMusicMode == EMusicMode.SHUFFLE)
                            ShuffleSongs();

                        else if (MusicPlayerViewModel.EMusicMode == EMusicMode.REPLAY_UNIQUE)
                            player.Loop = true;
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

        private void ShuffleSongs()
        {
            Random random = new();
            for (int i = 0; i < songs.Count; i++)
            {
                int randomIndex = random.Next(0, songs.Count);
                AudioItem temp = songs[i];
                songs[i] = songs[randomIndex];
                songs[randomIndex] = temp;
            }

            App.AudioPlayerViewModel.AudioItems = songs;
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