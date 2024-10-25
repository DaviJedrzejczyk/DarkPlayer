using Entities;
using Entities.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMaui.Models.ViewModels
{
    public class MusicPlayerViewModel : INotifyPropertyChanged
    {
        private string name;
        private string filePath;
        private TimeSpan duration;
        private string author;
        private string album;
        private string genre;
        private ImageSource albumArt;
        private bool _isPlaying;
        private double currentAudioTime;
        private double audioDuration;
        private List<AudioItem> audioItems;
        private EMusicMode _eMusicMode;

        public string Name
        {
            get => name;
            set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public string FilePath
        {
            get => filePath;
            set
            {
                if (filePath != value)
                {
                    filePath = value;
                    OnPropertyChanged(nameof(FilePath));
                }
            }
        }

        public TimeSpan Duration
        {
            get => duration;
            set
            {
                if (duration != value)
                {
                    duration = value;
                    OnPropertyChanged(nameof(Duration));
                }
            }
        }

        public string Author
        {
            get => author;
            set
            {
                if (author != value)
                {
                    author = value;
                    OnPropertyChanged(nameof(Author));
                }
            }
        }

        public string Album
        {
            get => album;
            set
            {
                if (album != value)
                {
                    album = value;
                    OnPropertyChanged(nameof(Album));
                }
            }
        }

        public string Genre
        {
            get => genre;
            set
            {
                if (genre != value)
                {
                    genre = value;
                    OnPropertyChanged(nameof(Genre));
                }
            }
        }

        public ImageSource AlbumArt
        {
            get => albumArt;
            set
            {
                if (albumArt != value)
                {
                    albumArt = value;
                    OnPropertyChanged(nameof(AlbumArt));
                }
            }
        }

        public bool IsPlaying
        {
            get => _isPlaying;
            set
            {
                _isPlaying = value;
                OnPropertyChanged(nameof(IsPlaying));
            }
        }

        public double CurrentAudioTime
        {
            get => currentAudioTime;
            set
            {
                if (currentAudioTime != value)
                {
                    currentAudioTime = value;
                    OnPropertyChanged(nameof(CurrentAudioTime));
                }
            }
        }

        public double AudioDuration
        {
            get => audioDuration;
            set
            {
                if (audioDuration != value)
                {
                    audioDuration = value;
                    OnPropertyChanged(nameof(AudioDuration)); 
                }
            }
        }

        public List<AudioItem> AudioItems
        {
            get => audioItems;
            set
            {
                if (audioItems != value)
                {
                    audioItems = value;
                    OnPropertyChanged(nameof(AudioItems));
                }
            }
        }

        public EMusicMode EMusicMode
        {
            get => _eMusicMode;
            set
            {
                if (_eMusicMode != value)
                {
                    _eMusicMode = value;
                    OnPropertyChanged(nameof(EMusicMode));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void LoadAudioItem(AudioItem audioItem)
        {
            Name = audioItem.Name;
            FilePath = audioItem.FilePath;
            Duration = audioItem.Duration;
            AudioDuration = audioItem.Duration.TotalMinutes;
            Author = audioItem.Author;
            Album = audioItem.Album;
            Genre = audioItem.Genre;
            AlbumArt = audioItem.AlbumArt ?? ImageSource.FromFile("standartimage.png");
        }
    }


}
