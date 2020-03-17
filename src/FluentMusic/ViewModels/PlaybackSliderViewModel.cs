using FluentMusic.Core.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Windows.UI.Xaml;

namespace FluentMusic.ViewModels
{
    public class PlaybackSliderViewModel : ReactiveObject
    {
        [Reactive]
        public double SliderCurrentPosition { get; set; }
        [Reactive]
        public double SliderNaturalPosition { get; set; }

        public ISubject<RoutedEventArgs> ProgressSliderManipulationStarted { get; } = new Subject<RoutedEventArgs>();
        public ISubject<RoutedEventArgs> ProgressSliderManipulationCompleted { get; } = new Subject<RoutedEventArgs>();

        public PlaybackSliderViewModel()
        {
            var _progressSliderIsDragging = false;

            ProgressSliderManipulationStarted
                .ObservableOnThreadPool()
                .Subscribe(_ => _progressSliderIsDragging = true);

            ProgressSliderManipulationCompleted
                .ObservableOnThreadPool()
                .Subscribe(_ =>
                {
                    _progressSliderIsDragging = false;
                    PlaybackService.Seek(TimeSpan.FromSeconds(SliderCurrentPosition));
                });

            PlaybackService.NewTrackPlayed
                .ObserveOnCoreDispatcher()
                .Subscribe(x =>
                {
                    SliderNaturalPosition = x.PlaybackItem.Source.Duration.Value.TotalSeconds;
                });

            PlaybackService.PlaybackPosition
                .Where(_ => !_progressSliderIsDragging)
                .ObserveOnCoreDispatcher()
                .Subscribe(x =>
                {
                    SliderCurrentPosition = x.Position.TotalSeconds;
                });
        }
    }
}