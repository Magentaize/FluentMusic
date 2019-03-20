using Magentaize.FluentPlayer.Core;
using Magentaize.FluentPlayer.Core.Services;
using Magentaize.FluentPlayer.ViewModels.DataViewModel;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive.Linq;
using Windows.ApplicationModel.Core;
using Windows.Media.Playback;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace Magentaize.FluentPlayer.ViewModels
{
    public class FullPlayerPageViewModel : ReactiveObject
    {
        [ObservableAsProperty]
        public bool IsPlaying { get; }

        private CombinedDbViewModel _dataSource;

        //public CombinedDbViewModel DataSource
        //{
        //    get => _dataSource;
        //    set => SetProperty(ref _dataSource, value);
        //}

        [Reactive]
        public string PositionInfo { get; set; }

        [Reactive]
        public string TrackTitle { get; set; }

        [Reactive]
        public string TrackArtist { get; set; }

        [Reactive]
        public double SliderCurrentPosition { get; set; }

        [Reactive]
        public double SliderNaturalPosition { get; set; }

        private TimeSpan _naturalPosition;
        private bool _progressSliderIsDragging = false;

        public FullPlayerPageViewModel()
        {
            ServiceFacade.PlaybackService.PlayerPositionChanged += PlaybackService_PlayerPositionChanged;
            ServiceFacade.PlaybackService.NewTrackPlayed += PlaybackService_NewTrackPlayed;

            //ServiceFacade.PlaybackService.IsPlaying.DistinctUntilChanged()
            //    .ToPropertyEx(this, x => x.IsPlaying);
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
    }
}