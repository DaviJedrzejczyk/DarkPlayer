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

#if ANDROID
using Java.IO;
#endif
namespace NetMaui.Views
{

    public partial class MainPage : ContentPage
    {
        private readonly IAudioManager audioManager;
        private IAudioPlayer player;
        private double audioDuration = 0;
        private double currentAudioTime = 0;
        private System.Timers.Timer timer;
        private string currentAudioFilePath = string.Empty;
        private AudioItem audioItem = new();
        public MainPage(IAudioManager audioManager)
        {
            InitializeComponent();
            this.audioManager = audioManager;
            LoadAudioFiles();
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
                    var audioProgressSlider = (Slider)FindByName("audioProgressSlider");
                    btnPlay.ImageSource = "pause_24.svg";
                    player.Play();

                    audioDuration = player.Duration;

                    if (audioProgressSlider.Value >= player.Duration - 1)
                        audioProgressSlider.Value = 0;

                    audioProgressSlider.Maximum = audioDuration;

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
                var listMusic = AudioListView.ItemsSource as List<AudioItem>;

                var actualMusic = listMusic.FindIndex(m => m.FilePath == currentAudioFilePath) + 1;

                var nextMusic = listMusic[actualMusic];

                if (nextMusic != null)
                {
                    PlayMusic(nextMusic.FilePath);
                    ChangeLabel(nextMusic.Name);
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

        private async Task VerifyPermission()
        {
            PermissionsViewModel permissionsViewModel = new();
            await permissionsViewModel.RequestPermissions();
        }

        private void ChangeLabel(string nameMusic)
        {
            try
            {
                var lblNome = (Label)FindByName("lblNome");
                int limiteCaracteres = 15;

                if (nameMusic.Length > limiteCaracteres)
                    nameMusic = nameMusic[..limiteCaracteres] + "...";

                lblNome.Text = nameMusic;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
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
            currentAudioTime = e.NewValue;
            player.Seek(currentAudioTime);
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {

            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    currentAudioTime += 1;

                    if (currentAudioTime <= audioDuration)
                    {
                        var audioProgressSlider = (Slider)FindByName("audioProgressSlider");
                        audioProgressSlider.Value = currentAudioTime;
                    }
                    else
                    {
                        if (!player.IsPlaying && player.CurrentPosition >= player.Duration)
                        {
                            var btnPlay = (Button)FindByName("btnPlay");
                            btnPlay.ImageSource = "play_arrow_24.svg";
                        }

                        timer.Stop();
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message, ex);
                }
            });
        }

        private void LoadAudioFiles()
        {
            try
            {
                var audioFiles = audioItem.LoadMusicsFromDirectory("/storage/emulated/0/snaptube/Download/SnapTube Audio/");
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
                    currentAudioFilePath = audioItem.FilePath;
                    ChangeLabel(audioItem.Name);
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

                if (System.IO.File.Exists(audioFilePath))
                {
                    //await PlayMusic(audioFilePath);
                    ChangeLabel(audioItem.Name);
                    ChangeImage(audioItem.AlbumArt);
                    UpdateBackgroundBasedOnImage(audioItem);
                    Preferences.Set("LastPlayedMusic", audioFilePath);
                    await Navigation.PushAsync(new DetailMusicPage(audioItem, audioManager));
                }
                ((ListView)sender).SelectedItem = null;
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
    }
}