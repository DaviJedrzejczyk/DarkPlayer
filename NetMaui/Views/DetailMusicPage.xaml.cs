using CommunityToolkit.Maui.Views;
using Entities;
using Entities.Enums;
using MediaManager.Queue;
using Models.Impl;
using Models.Interfaces;
using Mopups.Services;
using NetMaui.Models.ViewModels;
using Plugin.Maui.Audio;
using System.Reflection;
using System.Timers;

namespace NetMaui.Views
{
    public partial class DetailMusicPage : ContentPage
    {
        public MusicPlayerViewModel ViewModel { get; set; }


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
        private List<AudioItem> originalSongsList = [];
        private EMusicMode eMusicMode;

        public DetailMusicPage(AudioItem audioItem, IAudioManager audioManager, MusicPlayerViewModel viewModel)
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            this.favoriteService = new FavoriteService();

            this.ViewModel = viewModel;
            this.audioItem = audioItem;
            this.audioManager = audioManager;
            this.audioDuration = App.AudioPlayerViewModel.AudioDuration;

            this.ViewModel.LoadAudioItem(audioItem);
            originalSongsList = ViewModel.AudioItems;
            songs = ViewModel.AudioItems;
            BindingContext = this.ViewModel;

            eMusicMode = LoadMusicMode();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadAudioDuration();
            LoadMode();

            if (favoriteService.IsFavorite(audioItem.FilePath))
                BtnFavorite.ImageSource = ImageSource.FromFile("favorite_check_24.png");
            else
                BtnFavorite.ImageSource = ImageSource.FromFile("favorite_24.png");
        }

        private void LoadAudioDuration()
        {
            try
            {
                player = App.AudioPlayerViewModel.AudioPlayer;

                ViewModel.Duration = TimeSpan.FromMinutes(player.Duration);

                LblAudioDurationLabel.Text = FormatTime(ViewModel.Duration.TotalMinutes);

                LblCurrentAudioTime.Text = FormatTime(App.AudioPlayerViewModel.CurrentAudioTime);

                audioProgressSlider.Maximum = App.AudioPlayerViewModel.AudioDuration;

                if (ViewModel.IsPlaying)
                {
                    timer = new System.Timers.Timer(1000);
                    timer.Elapsed += Timer_Elapsed;
                    timer.Start();
                    BtnPlay.ImageSource = ImageSource.FromFile("pause_44");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        private void LoadMode()
        {
            BtnMode.ImageSource = eMusicMode switch
            {
                EMusicMode.CONTINUOUS => ImageSource.FromFile("replay_24.svg"),
                EMusicMode.REPLAY_UNIQUE => ImageSource.FromFile("restart_alt_24.svg"),
                EMusicMode.SHUFFLE => ImageSource.FromFile("shuffle_24.svg"),
                _ => ImageSource.FromFile("replay_24.svg"),
            };

            if (eMusicMode == EMusicMode.SHUFFLE)
                ShuffleSongs();

            else if (eMusicMode == EMusicMode.CONTINUOUS)
                songs = ViewModel.AudioItems;

            else
                player.Loop = true;
        }

        private void audioProgressSlider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            try
            {
                currentAudioTime = e.NewValue;
                App.AudioPlayerViewModel.CurrentAudioTime = e.NewValue;
                LblCurrentAudioTime.Text = FormatTime(App.AudioPlayerViewModel.CurrentAudioTime);
                player.Seek(App.AudioPlayerViewModel.CurrentAudioTime);
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
            if (favoriteService.IsFavorite(audioItem.FilePath))
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
            eMusicMode = LoadMusicMode();

            switch (eMusicMode)
            {
                case EMusicMode.CONTINUOUS:
                    ChangeToUniqueMode();
                    break;
                case EMusicMode.REPLAY_UNIQUE:
                    ChangeToShuffleMode();
                    break;
                case EMusicMode.SHUFFLE:
                    ChangeToContinuosMode();
                    break;
            }
        }

        private void BtnNext_Clicked(object sender, EventArgs e)
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
                    lblName.Text = previousMusic.Name;
                    PlayMusic(previousMusic.FilePath);
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
                    ViewModel.IsPlaying = false;
                }
                else
                {
                    BtnPlay.ImageSource = "pause_44.svg";
                    player.Play();

                    audioDuration = player.Duration;
                    var audioProgressSlider = (Slider)FindByName("audioProgressSlider");
                    audioProgressSlider.Maximum = audioDuration;
                    LblAudioDurationLabel.Text = FormatTime(audioDuration);

                    if (audioProgressSlider.Value >= player.Duration - 1)
                        audioProgressSlider.Value = 0;

                    ViewModel.IsPlaying = true;

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

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    if (player.IsPlaying)
                    {

                        if (App.AudioPlayerViewModel.CurrentAudioTime <= App.AudioPlayerViewModel.AudioDuration)
                        {
                            audioProgressSlider.Value = App.AudioPlayerViewModel.CurrentAudioTime;
                            LblCurrentAudioTime.Text = FormatTime(App.AudioPlayerViewModel.CurrentAudioTime);
                            ViewModel.CurrentAudioTime = App.AudioPlayerViewModel.CurrentAudioTime;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Entrou agora: " + DateTime.Now);
                        if (App.AudioPlayerViewModel.CurrentAudioTime >= App.AudioPlayerViewModel.AudioDuration)
                        {
                            if (eMusicMode != EMusicMode.REPLAY_UNIQUE)
                                NextMusic();
                            else
                            {
                                App.AudioPlayerViewModel.CurrentAudioTime = 0;


                                audioProgressSlider.Value = App.AudioPlayerViewModel.CurrentAudioTime;
                                LblCurrentAudioTime.Text = FormatTime(App.AudioPlayerViewModel.CurrentAudioTime);
                                ViewModel.CurrentAudioTime = App.AudioPlayerViewModel.CurrentAudioTime;

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

        private void NextMusic()
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
                lblName.Text = nextMusic.Name;
                PlayMusic(nextMusic.FilePath);
                ChangeImage(nextMusic.AlbumArt);
                //UpdateBackgroundBasedOnImage(nextMusic);
                Preferences.Set("LastPlayedMusic", nextMusic.FilePath);
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

        private void ChangeToUniqueMode()
        {
            Preferences.Set(Preference, EMusicMode.REPLAY_UNIQUE.ToString());
            BtnMode.ImageSource = ImageSource.FromFile("restart_alt_24.svg");
            player.Loop = true;
            ViewModel.EMusicMode = EMusicMode.REPLAY_UNIQUE;
        }

        private void ChangeToShuffleMode()
        {
            Preferences.Set(Preference, EMusicMode.SHUFFLE.ToString());
            BtnMode.ImageSource = ImageSource.FromFile("shuffle_24.svg");
            player.Loop = false;
            ShuffleSongs();
            ViewModel.EMusicMode = EMusicMode.SHUFFLE;
        }

        private void ChangeToContinuosMode()
        {
            Preferences.Set(Preference, EMusicMode.CONTINUOUS.ToString());
            BtnMode.ImageSource = ImageSource.FromFile("replay_24.svg");
            player.Loop = false;

            var audioFiles = audioItem.LoadMusicsFromDirectory("/storage/emulated/0/snaptube/Download/SnapTube Audio/");
            originalSongsList = new List<AudioItem>(audioFiles);

            ViewModel.AudioItems = originalSongsList;
            App.AudioPlayerViewModel.AudioItems = originalSongsList;
            songs = originalSongsList;

            ViewModel.EMusicMode = EMusicMode.CONTINUOUS;
        }
    }
}