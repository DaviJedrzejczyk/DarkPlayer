using CommunityToolkit.Mvvm.Input;
using MediaManager;
using NetMaui.Models.ViewModels;
using Plugin.Maui.Audio;
#if ANDROID
    using Java.IO;
#endif
namespace NetMaui.Views
{

    public partial class MainPage : ContentPage
    {
        private IAudioManager audioManager;
        private IAudioPlayer player;
        private double audioDuration = 0;
        private double currentAudioTime = 0;
        private System.Timers.Timer timer;
        private string currentAudioFilePath = string.Empty;
        private bool isButtonPause = false;

        public MainPage(IAudioManager audioManager)
        {
            InitializeComponent();
            this.audioManager = audioManager;
            LoadAudioFiles();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await VerifyPermission();
            StartLabelScrolling();
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
            var lblNome = (Label)FindByName("lblNome");
            int limiteCaracteres = 17;
            if (lblNome.Text.Length > limiteCaracteres)
            {
                lblNome.Text = lblNome.Text.Substring(0, limiteCaracteres) + "...";
            }

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
            await VerifyPermission();
            await PlayMusic(currentAudioFilePath);
        }

        private async Task PlayMusic(string filePath)
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
                btnPlay.ImageSource = "play_arrow.svg";
                player.Pause();
                timer?.Stop();
            }
            else
            {
                var audioProgressSlider = (Slider)FindByName("audioProgressSlider");
                btnPlay.ImageSource = "pause.svg";
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

        private void Next_Clicked(object sender, EventArgs e)
        {

        }

        private void Previous_Clicked(object sender, EventArgs e)
        {

        }

        private async Task VerifyPermission()
        {
            PermissionsViewModel permissionsViewModel = new();
            await permissionsViewModel.RequestPermissions();
        }

        public void ChangeLabel(string nameMusic)
        {
            var lblNome = (Label)FindByName("lblNome");
            int limiteCaracteres = 15;
            if (nameMusic.Length > limiteCaracteres)
            {
                nameMusic = nameMusic[..limiteCaracteres] + "...";
            }

            lblNome.Text = nameMusic;
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
                        btnPlay.ImageSource = "play_arrow.svg";
                    }

                    timer.Stop();
                }
            });
        }

        private void LoadAudioFiles()
        {
            List<string> audioFiles = [];
#if ANDROID
        audioFiles = GetAudioFilesFromCustomFolder();
#endif
            AudioListView.ItemsSource = audioFiles;
        }

#if ANDROID
    public List<string> GetAudioFilesFromCustomFolder()
    {
        List<string> audioFiles = new List<string>();

        Android.Net.Uri uri = Android.Provider.MediaStore.Audio.Media.ExternalContentUri;

        string[] projection = { Android.Provider.MediaStore.Audio.AudioColumns.Data };

       using (var cursor = Android.App.Application.Context.ContentResolver.Query(uri, projection, null, null, null))
    {
        if (cursor != null && cursor.MoveToFirst())
        {
            do
            {
                string filePath = cursor.GetString(cursor.GetColumnIndexOrThrow(Android.Provider.MediaStore.Audio.AudioColumns.Data));
                
                if (filePath.StartsWith("/storage/emulated/0/snaptube/Download/SnapTube Audio"))
                {
                    filePath = filePath.Replace("/storage/emulated/0/snaptube/Download/SnapTube Audio/", "");
                    filePath = filePath.Replace(".mp3", "");
                    audioFiles.Add(filePath); 
                }
            } while (cursor.MoveToNext());
        }
    }

        return audioFiles;
    }
#endif
        private async void AudioListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null)
                return;

            string selectedSongName = e.Item.ToString();

            string musicFolderPath = "/storage/emulated/0/snaptube/Download/SnapTube Audio/";

            string audioFilePath = Path.Combine(musicFolderPath, selectedSongName + ".mp3");

            if (System.IO.File.Exists(audioFilePath))
            {
                await PlayMusic(audioFilePath);
                ChangeLabel(selectedSongName);
            }

             ((ListView)sender).SelectedItem = null;
        }

    }
}