using Entities;
using Plugin.Maui.Audio;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMaui.Models.ViewModels
{
    public class AudioPlayerViewModel : INotifyPropertyChanged
    {
        private double _currentAudioTime;
        public double CurrentAudioTime
        {
            get => _currentAudioTime;
            set
            {
                if (_currentAudioTime != value)
                {
                    _currentAudioTime = value;
                    OnPropertyChanged(nameof(CurrentAudioTime));
                }
            }
        }

        private double _audioDuration;
        public double AudioDuration
        {
            get => _audioDuration;
            set
            {
                if (_audioDuration != value)
                {
                    _audioDuration = value;
                    OnPropertyChanged(nameof(AudioDuration));
                }
            }
        }

        private IAudioPlayer _audioPlayer;
        public IAudioPlayer AudioPlayer
        {
            get => _audioPlayer;
            set
            {
                if(_audioPlayer != value)
                {
                    _audioPlayer = value;
                    OnPropertyChanged(nameof(AudioPlayer));

                }
            }
        }

        private List<AudioItem> _audioItems;

        public List<AudioItem> AudioItems
        {
            get => _audioItems;
            set
            {
                if (_audioItems != value)
                {
                    _audioItems = value;
                    OnPropertyChanged(nameof(AudioItems));
                }
            }
        }

        private bool _isInDetailPage;

        public bool IsInDetailPage
        {
            get => _isInDetailPage;
            set
            {
                if (_isInDetailPage != value)
                {
                    _isInDetailPage = value;
                    OnPropertyChanged(nameof(IsInDetailPage));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
