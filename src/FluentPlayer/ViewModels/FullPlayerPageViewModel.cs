using Magentaize.FluentPlayer.Core;
using Magentaize.FluentPlayer.Core.Services;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Media.Playback;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Magentaize.FluentPlayer.ViewModels.DataViewModel;
using Prism.Commands;
using System.Windows.Input;

namespace Magentaize.FluentPlayer.ViewModels
{
    internal class FullPlayerPageViewModel : BindableBase
    {
        private bool _isPlaying;

        public bool IsPlaying
        {
            get => _isPlaying;
            set => SetProperty(ref _isPlaying, value);
        }

        private CombinedDbViewModel _dataSource;

        public CombinedDbViewModel DataSource
        {
            get => _dataSource;
            set => SetProperty(ref _dataSource, value);
        }

        private string _positionInfo;

        public string PositionInfo
        {
            get => _positionInfo;
            set => SetProperty(ref _positionInfo, value);
        }

        private string _trackTitle;

        public string TrackTitle
        {
            get => _trackTitle;
            set => SetProperty(ref _trackTitle, value);
        }

        private string _trackArtist;

        public string TrackArtist
        {
            get => _trackArtist;
            set => SetProperty(ref _trackArtist, value);
        }

        private double _sliderCurrentPosition;

        public double SliderCurrentPosition
        {
            get => _sliderCurrentPosition;
            set => SetProperty(ref _sliderCurrentPosition, value);
        }

        private double _sliderNaturalPosition;

        public double SliderNaturalPosition
        {
            get => _sliderNaturalPosition;
            set => SetProperty(ref _sliderNaturalPosition, value);
        }

        private TimeSpan _naturalPosition;
        private bool _progressSliderIsDragging = false;

        public ICommand ResumePauseCommand { get; }
        public ICommand PreviousCommand { get; }
        public ICommand NextCommand { get; }

        public FullPlayerPageViewModel()
        {
            ServiceFacade.PlaybackService.PlayerPositionChanged += PlaybackService_PlayerPositionChanged;
            ServiceFacade.PlaybackService.NewTrackPlayed += PlaybackService_NewTrackPlayed;
            ServiceFacade.PlaybackService.PlayingStatusChanged += PlaybackService_PlayingStatusChanged;

            ResumePauseCommand = new DelegateCommand(ResumePause);
            PreviousCommand = new DelegateCommand(Previous);
            NextCommand = new DelegateCommand(Next);
        }

        private void ResumePause()
        {
            if (ServiceFacade.PlaybackService.IsPlaying) ServiceFacade.PlaybackService.Pause();
            else ServiceFacade.PlaybackService.Resume();
        }
       
        private void Previous()
        {

        }

        private void Next()
        {

        }

        private void PlaybackService_PlayingStatusChanged(object sender, object _)
        {
            IsPlaying = ServiceFacade.PlaybackService.IsPlaying;
        }

        private void PlaybackService_NewTrackPlayed(object sender, NewTrackPlayedEventArgs e)
        {
            TrackTitle = e.TrackTitle;
            TrackArtist = e.TrackArtist;
            _naturalPosition = e.NaturalDuration;
            _progressSliderIsDragging = false;
            SliderNaturalPosition = e.NaturalDuration.TotalSeconds;
        }

        private async void PlaybackService_PlayerPositionChanged(object sender, MediaPlaybackSession e)
        {
            var d1 = e.Position;
            var d2 = _naturalPosition;
            var posInfo = d2.Hours != 0 ? $"{d1:hh\\.mm\\:ss}/{d2:hh\\.mm\\:ss}" : $"{d1:mm\\:ss}/{d2:mm\\:ss}";

            await CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                PositionInfo = posInfo;

                if (!_progressSliderIsDragging) SliderCurrentPosition = d1.TotalSeconds;
            });
        }

        public void ProgressSlider_OnManipulationStarting(Slider slider)
        {
            _progressSliderIsDragging = true;
        }

        public void ProgressSlider_OnManipulationCompleted(Slider slider)
        {
            _progressSliderIsDragging = false;

            ServiceFacade.PlaybackService.Seek(TimeSpan.FromSeconds(slider.Value));
        }

        public async Task ShowAllTracksAsync()
        {
            var tracks = await ServiceFacade.IndexService.GetAllTracksAsync();
            var artists = await ServiceFacade.IndexService.GetAllArtistsAsync();
            var albums = await ServiceFacade.IndexService.GetAllAlbumsAsync();

            DataSource = new CombinedDbViewModel()
            {
                Albums = new ObservableCollection<AlbumViewModel>(albums.Select(a=>new AlbumViewModel(a))),
                Artists = new ObservableCollection<ArtistViewModel>(artists.Select(a=>new ArtistViewModel(a))),
                Tracks = new ObservableCollection<TrackViewModel>(tracks.Select(t=>new TrackViewModel(t))),
            };
        }
    }
}