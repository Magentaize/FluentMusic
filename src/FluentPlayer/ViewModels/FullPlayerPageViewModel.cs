using Magentaize.FluentPlayer.Core;
using Magentaize.FluentPlayer.Core.Services;
using Prism.Mvvm;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace Magentaize.FluentPlayer.ViewModels
{
    internal class FullPlayerPageViewModel : BindableBase
    {
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

        public FullPlayerPageViewModel()
        {
            ServiceFacade.PlaybackService.PlayerPositionChanged += PlaybackService_PlayerPositionChanged;
            ServiceFacade.PlaybackService.NewTrackPlayed += PlaybackService_NewTrackPlayed;
        }

        private void PlaybackService_NewTrackPlayed(object sender, NewTrackPlayedEventArgs e)
        {
            TrackTitle = e.TrackTitle;
            TrackArtist = e.TrackArtist;
            _naturalPosition = e.NaturalDuration;
            SliderNaturalPosition = e.NaturalDuration.TotalSeconds;
        }

        private async void PlaybackService_PlayerPositionChanged(object sender, Windows.Media.Playback.MediaPlaybackSession e)
        {
            var d1 = e.Position;
            var d2 = _naturalPosition;
            var posInfo = d2.Hours != 0 ? $"{d1:hh\\.mm\\:ss}/{d2:hh\\.mm\\:ss}" : $"{d1:mm\\:ss}/{d2:mm\\:ss}";

            await CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                PositionInfo = posInfo;

                SliderCurrentPosition = d1.TotalSeconds;
            });
        }

        public async Task ShowAllTracksAsync()
        {
            var tracks = await ServiceFacade.IndexService.GetAllTracksAsync();
            var artists = await ServiceFacade.IndexService.GetAllArtistsAsync();
            var albums = await ServiceFacade.IndexService.GetAllAlbumsAsync();

            DataSource = new CombinedDbViewModel()
            {
                Albums = albums,
                Artists = artists,
                Tracks = tracks,
            };
        }
    }
}