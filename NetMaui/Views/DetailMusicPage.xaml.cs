using CommunityToolkit.Maui.Views;
using Entities;
using Entities.Enums;
using MediaManager.Queue;
using Models.Impl;
using Models.Interfaces;
using Mopups.Services;
using NetMaui.Models.ViewModels;
using System.Text.Json;
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
        private static bool timerStarted = false;

        public DetailMusicPage(AudioItem audioItem, IAudioManager audioManager, MusicPlayerViewModel viewModel)
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            this.favoriteService = new FavoriteService();

            this.ViewModel = viewModel;
            this.audioItem = audioItem;
            this.audioManager = audioManager;

            this.ViewModel.LoadAudioItem(audioItem);
            originalSongsList = ViewModel.AudioItems;
            LoadSongs();
            eMusicMode = LoadMusicMode();
        }

        private void LoadSongs()
        {
            try
            {
                if (Preferences.ContainsKey("ShuffledSongs"))
                {
                    string shuffledSongsJson = Preferences.Get("ShuffledSongs", string.Empty);

                    if (!string.IsNullOrEmpty(shuffledSongsJson))
                    {
                        songs = JsonSerializer.Deserialize<List<AudioItem>>(shuffledSongsJson);

                        foreach (var shuffledSong in songs)
                        {
                            var matchingFile = App.AudioPlayerViewModel.AudioItems.FirstOrDefault(audio => audio.FilePath == shuffledSong.FilePath);

                            if (matchingFile != null && shuffledSong.AlbumArt == null)
                            {
                                shuffledSong.AlbumArt = matchingFile.AlbumArt;
                            }
                        }

                        ViewModel.AudioItems = songs;
                    }
                }
                else
                    songs = ViewModel.AudioItems;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            LoadAudioDuration();
            
            LoadMode();

            if (favoriteService.IsFavorite(audioItem.FilePath))
                BtnFavorite.ImageSource = ImageSource.FromFile("favorite_check_24.png");
            else
                BtnFavorite.ImageSource = ImageSource.FromFile("favorite_24.png");

            BindingContext = this.ViewModel;

            //Sim é necessário esse Task.Delay, se diminuir mais a imagem não vai carregar quando você entra nessa página. Não me pergunte sobre.
            await Task.Delay(5);
            LoadImage();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            App.AudioPlayerViewModel.IsInDetailPage = false;

            if (timerStarted)
            {
                timer.Elapsed -= Timer_Elapsed;
                timerStarted = false;
            }
        }

        private void LoadImage()
        {
            int index = songs.FindIndex(m => m.FilePath == audioItem.FilePath);
            var music = songs[index];
            imgMusic.Source = music.AlbumArt ?? ImageSource.FromFile("standarimage.png");
        }

        private void LoadAudioDuration()
        {
            try
            {
                if (App.AudioPlayerViewModel.AudioPlayer == null)
                {
                    App.AudioPlayerViewModel.AudioPlayer = audioManager.CreatePlayer(System.IO.File.OpenRead(audioItem.FilePath));
                    ViewModel.Duration = TimeSpan.FromMinutes(App.AudioPlayerViewModel.AudioPlayer.Duration);
                }
                
                LblAudioDurationLabel.Text = FormatTime(App.AudioPlayerViewModel.AudioPlayer.Duration);
                LblCurrentAudioTime.Text = FormatTime(App.AudioPlayerViewModel.CurrentAudioTime);
                audioProgressSlider.Maximum = App.AudioPlayerViewModel.AudioPlayer.Duration;
                audioProgressSlider.Value = App.AudioPlayerViewModel.AudioPlayer.CurrentPosition;

                if (ViewModel.IsPlaying && !timerStarted)
                {
                    timer = new System.Timers.Timer(1000);
                    timer.Elapsed += Timer_Elapsed;
                    timer.Start();
                    BtnPlay.ImageSource = ImageSource.FromFile("pause_44");
                    timerStarted = true;
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

            if (eMusicMode == EMusicMode.CONTINUOUS)
                songs = ViewModel.AudioItems;

            else if (eMusicMode == EMusicMode.REPLAY_UNIQUE)
                App.AudioPlayerViewModel.AudioPlayer.Loop = true;
        }

        private void audioProgressSlider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            try
            {
                currentAudioTime = e.NewValue;
                App.AudioPlayerViewModel.CurrentAudioTime = e.NewValue;
                LblCurrentAudioTime.Text = FormatTime(App.AudioPlayerViewModel.CurrentAudioTime);
                App.AudioPlayerViewModel.AudioPlayer.Seek(App.AudioPlayerViewModel.CurrentAudioTime);
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
                if (App.AudioPlayerViewModel.AudioPlayer == null || audioItem.FilePath != filePath)
                {
                    if (App.AudioPlayerViewModel.AudioPlayer != null)
                    {
                        App.AudioPlayerViewModel.AudioPlayer.Stop();
                        App.AudioPlayerViewModel.AudioPlayer.Dispose();
                        timer?.Stop();
                        timer?.Dispose();
                    }

                    audioItem.FilePath = filePath;
                    App.AudioPlayerViewModel.AudioPlayer = audioManager.CreatePlayer(System.IO.File.OpenRead(audioItem.FilePath));

                    var audioProgressSlider = (Slider)FindByName("audioProgressSlider");
                    audioProgressSlider.Value = 0;
                }

                if (App.AudioPlayerViewModel.AudioPlayer.IsPlaying)
                {
                    BtnPlay.ImageSource = "play_arrow_44.svg";
                    App.AudioPlayerViewModel.AudioPlayer.Pause();
                    timer?.Stop();
                    ViewModel.IsPlaying = false;
                    timerStarted = false;
                }
                else
                {
                    BtnPlay.ImageSource = "pause_44.svg";
                    App.AudioPlayerViewModel.AudioPlayer.Play();

                    audioDuration = App.AudioPlayerViewModel.AudioPlayer.Duration;
                    var audioProgressSlider = (Slider)FindByName("audioProgressSlider");
                    audioProgressSlider.Maximum = audioDuration;
                    LblAudioDurationLabel.Text = FormatTime(audioDuration);

                    if (audioProgressSlider.Value >= audioDuration - 1)
                        audioProgressSlider.Value = 0;

                    ViewModel.IsPlaying = true;

                    App.AudioPlayerViewModel.AudioDuration = audioDuration;

                    timer = new System.Timers.Timer(1000);
                    timer.Elapsed += Timer_Elapsed;
                    timer.Start();
                    timerStarted = true;
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
                        if (App.AudioPlayerViewModel.IsInDetailPage)
                        {
                            App.AudioPlayerViewModel.CurrentAudioTime += 1;
                            Preferences.Set("CurrentAudioTime", App.AudioPlayerViewModel.CurrentAudioTime);
                        }

                        if (App.AudioPlayerViewModel.CurrentAudioTime <= App.AudioPlayerViewModel.AudioDuration)
                        {
                            audioProgressSlider.Value = App.AudioPlayerViewModel.CurrentAudioTime;
                            LblCurrentAudioTime.Text = FormatTime(App.AudioPlayerViewModel.CurrentAudioTime);
                            ViewModel.CurrentAudioTime = App.AudioPlayerViewModel.CurrentAudioTime;
                        }

                        if (App.AudioPlayerViewModel.CurrentAudioTime >= App.AudioPlayerViewModel.AudioDuration && App.AudioPlayerViewModel.AudioPlayer.Loop)
                        {
                            App.AudioPlayerViewModel.CurrentAudioTime = 0;

                            audioProgressSlider.Value = App.AudioPlayerViewModel.CurrentAudioTime;
                            LblCurrentAudioTime.Text = FormatTime(App.AudioPlayerViewModel.CurrentAudioTime);
                            ViewModel.CurrentAudioTime = App.AudioPlayerViewModel.CurrentAudioTime;
                        }
                    }
                    else if (App.AudioPlayerViewModel.CurrentAudioTime >= App.AudioPlayerViewModel.AudioDuration - 1)
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

        private string FormatTime(double time)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(time);
            return timeSpan.ToString(@"mm\:ss");
        }

        private void NextMusic()
        {
            int actualMusic = songs.FindIndex(m => m.FilePath == audioItem.FilePath) + 1;

            if (actualMusic >= songs.Count)
            {
                actualMusic = 0;
            }

            var nextMusic = songs[actualMusic];

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
            ViewModel.AudioItems = songs;

            string shuffledSongsJson = JsonSerializer.Serialize(songs);
            Preferences.Set("ShuffledSongs", shuffledSongsJson);
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
            App.AudioPlayerViewModel.AudioPlayer.Loop = true;
            ViewModel.EMusicMode = EMusicMode.REPLAY_UNIQUE;
        }

        private void ChangeToShuffleMode()
        {
            Preferences.Set(Preference, EMusicMode.SHUFFLE.ToString());
            BtnMode.ImageSource = ImageSource.FromFile("shuffle_24.svg");
            App.AudioPlayerViewModel.AudioPlayer.Loop = false;
            ShuffleSongs();
            ViewModel.EMusicMode = EMusicMode.SHUFFLE;
        }

        private void ChangeToContinuosMode()
        {
            Preferences.Set(Preference, EMusicMode.CONTINUOUS.ToString());
            BtnMode.ImageSource = ImageSource.FromFile("replay_24.svg");
            App.AudioPlayerViewModel.AudioPlayer.Loop = false;

            var audioFiles = audioItem.LoadMusicsFromDirectory("/storage/emulated/0/snaptube/Download/SnapTube Audio/");
            originalSongsList = new List<AudioItem>(audioFiles);

            ViewModel.AudioItems = originalSongsList;
            App.AudioPlayerViewModel.AudioItems = originalSongsList;
            songs = originalSongsList;

            ViewModel.EMusicMode = EMusicMode.CONTINUOUS;
        }
    }
}