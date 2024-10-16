using CommunityToolkit.Maui.Views;
using Entities;
using MediaManager.Queue;
using Models.Impl;
using Models.Interfaces;
using Mopups.Services;
using Plugin.Maui.Audio;
using System.Timers;

namespace NetMaui.Views
{
    public partial class DetailMusicPage : ContentPage
    {
        private AudioItem audioItem;
        private IAudioManager audioManager;
        private IAudioPlayer player;
        private double audioDuration = 0;
        private double currentAudioTime = 0;
        private System.Timers.Timer timer;
        private double _translationY = 0;
        private const double SwipeDownThreshold = 100;
        private readonly IFavoriteService favoriteService;
        private double _totalY;
        private AudioItem teste = new();
        private const string Preference = "MODE";
        private List<AudioItem> songs = [];

        public DetailMusicPage(AudioItem audioItem, IAudioManager audioManager)
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            
            this.audioItem = audioItem;

            var test = this.teste.LoadMusicFromDirectory(audioItem.FilePath);
            BindingContext = test;

            this.audioManager = audioManager;
            this.favoriteService = new FavoriteService();
            songs = audioItem.LoadMusicsFromDirectory("/storage/emulated/0/snaptube/Download/SnapTube Audio/");
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadMode();

            if (favoriteService.IsFavorite(audioItem.FilePath))
            {
                BtnFavorite.ImageSource = ImageSource.FromFile("favorite_check_24.png");
            }
            else
            {
                BtnFavorite.ImageSource = ImageSource.FromFile("favorite_24.png");
            }
        }

        private void LoadMode()
        {
            string mode = SearchActualPreferencesMode();

            BtnMode.ImageSource = mode switch
            {
                "continuous" => ImageSource.FromFile("replay_24.svg"),
                "replayUnique" => ImageSource.FromFile("restart_alt_24.svg"),
                "shuffle" => ImageSource.FromFile("shuffle_24.svg"),
                _ => ImageSource.FromFile("replay_24.svg"),
            };
        }

        private void audioProgressSlider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            try
            {
                currentAudioTime = e.NewValue;
                LblCurrentAudioTime.Text = FormatTime(currentAudioTime);
                player.Seek(currentAudioTime);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        private async void BtnBack_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private void PanGestureRecognizer_PanUpdated(object sender, PanUpdatedEventArgs e)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    _translationY = this.TranslationY;
                    _totalY = 0;  
                    break;

                case GestureStatus.Running:
                    if (e.TotalY > 0)
                    {
                        this.TranslationY = _translationY + e.TotalY;
                        _totalY = e.TotalY;  
                    }
                    break;

                case GestureStatus.Completed:
                case GestureStatus.Canceled:
                    if (_totalY > SwipeDownThreshold)
                        ClosePageWithAnimation();
                    else
                        ResetPagePosition();
                    break;
            }
        }

        private async void ClosePageWithAnimation()
        {
            await this.TranslateTo(0, Application.Current.MainPage.Height, 100, Easing.Linear);
            await Navigation.PopAsync();
        }

        private async void ResetPagePosition()
        {
            await this.TranslateTo(0, 0, 100, Easing.Linear);
        }

        private void BtnFavorite_Clicked(object sender, EventArgs e)
        {
            if(favoriteService.IsFavorite(audioItem.FilePath))
            {
                favoriteService.RemoveFavorite(audioItem.FilePath);
                BtnFavorite.ImageSource = ImageSource.FromFile("favorite_24.png");
            }
            else
            {
                favoriteService.AddFavorite(audioItem.FilePath);
                BtnFavorite.ImageSource = ImageSource.FromFile("favorite_check_24.png");
            }
        }

        private void BtnMore_Clicked(object sender, EventArgs e)
        {
            MopupService.Instance.PushAsync(new MorePopup(audioItem));
        }

        private void BtnMenu_Clicked(object sender, EventArgs e)
        {

        }

        private void BtnMode_Clicked(object sender, EventArgs e)
        {
            SwitchMode();
        }

        private void SwitchMode()
        {
            string mode = SearchActualPreferencesMode();

            switch (mode)
            {
                case "continuous":
                    Preferences.Set(Preference, "replayUnique");
                    BtnMode.ImageSource = ImageSource.FromFile("restart_alt_24.svg");
                    break;
                case "replayUnique":
                    Preferences.Set(Preference, "shuffle");
                    BtnMode.ImageSource = ImageSource.FromFile("shuffle_24.svg");

                    break;
                case "shuffle":
                    Preferences.Set(Preference, "continuous");
                    BtnMode.ImageSource = ImageSource.FromFile("replay_24.svg");
                    break;
                default:
                    Preferences.Set(Preference, "continuous");
                    BtnMode.ImageSource = ImageSource.FromFile("replay_24.svg");
                    break;
            }
        }

        private string SearchActualPreferencesMode()
        {
            var mode = Preferences.Get(Preference, string.Empty);
            if (string.IsNullOrWhiteSpace(mode))
            {
                Preferences.Set(Preference, "continuous");
                mode = "continuous";
            }

            return mode;
        }

        private void BtnNext_Clicked(object sender, EventArgs e)
        {
            try
            {
                var listMusic = songs;

                var actualMusic = listMusic.FindIndex(m => m.FilePath == audioItem.FilePath) + 1;

                if (actualMusic >= listMusic.Count)
                {
                    actualMusic = 0;
                }

                var nextMusic = listMusic[actualMusic];

                if (nextMusic != null)
                {
                    PlayMusic(nextMusic.FilePath);
                    ChangeLabel(nextMusic.Name);
                    ChangeImage(nextMusic.AlbumArt);
                    //UpdateBackgroundBasedOnImage(nextMusic);
                    Preferences.Set("LastPlayedMusic", nextMusic.FilePath);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }


        private void BtnPlay_Clicked(object sender, EventArgs e)
        {
            try
            {
                PlayMusic(audioItem.FilePath);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        private void BtnPrevious_Clicked(object sender, EventArgs e)
        {
            try
            {
                var listMusic = songs;

                var actualMusic = listMusic.FindIndex(m => m.FilePath == audioItem.FilePath) - 1;

                if (actualMusic < 0)
                {
                    actualMusic = listMusic.Count - 1;
                }

                var previousMusic = listMusic[actualMusic];

                if (previousMusic != null)
                {
                    PlayMusic(previousMusic.FilePath);
                    ChangeLabel(previousMusic.Name);
                    ChangeImage(previousMusic.AlbumArt);
                    //UpdateBackgroundBasedOnImage(previousMusic);
                    Preferences.Set("LastPlayedMusic", previousMusic.FilePath);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        private void PlayMusic(string filePath)
        {
            try
            {
                if (player == null || audioItem.FilePath != filePath)
                {
                    if (player != null)
                    {
                        player.Stop();
                        player.Dispose();
                        timer?.Stop();
                        timer?.Dispose();
                    }

                    audioItem.FilePath = filePath;
                    player = audioManager.CreatePlayer(System.IO.File.OpenRead(audioItem.FilePath));

                    var audioProgressSlider = (Slider)FindByName("audioProgressSlider");
                    audioProgressSlider.Value = 0;
                }

                if (player.IsPlaying)
                {
                    BtnPlay.ImageSource = "play_arrow_44.svg";
                    player.Pause();
                    timer?.Stop();
                }
                else
                {
                    BtnPlay.ImageSource = "pause_44.svg";
                    player.Play();

                    audioDuration = player.Duration;
                    var audioProgressSlider = (Slider)FindByName("audioProgressSlider");
                    audioProgressSlider.Maximum = audioDuration;

                    if (audioProgressSlider.Value >= player.Duration - 1)
                        audioProgressSlider.Value = 0;

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

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    currentAudioTime = player.CurrentPosition;

                    var audioProgressSlider = (Slider)FindByName("audioProgressSlider");
                    if (currentAudioTime <= audioDuration)
                    {
                        audioProgressSlider.Value = currentAudioTime;
                        LblCurrentAudioTime.Text = FormatTime(currentAudioTime);  
                        LblAudioDurationLabel.Text = FormatTime(audioDuration);
                    }
                    else
                    {
                        if (!player.IsPlaying && player.CurrentPosition >= player.Duration)
                        {
                            BtnPlay.ImageSource = "play_arrow_44.svg";
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

        private string FormatTime(double time)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(time);
            return timeSpan.ToString(@"mm\:ss");
        }
    }
}
