using FluentMusic.Core;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;

namespace FluentMusic.ViewModels
{
    public class PlaybackSliderViewModel : ReactiveObject
    {
        [Reactive]
        public double SliderCurrentPosition { get; set; }
        [Reactive]
        public double SliderNaturalPosition { get; set; }

        public ICommand ProgressSliderOnManipulationStarting { get; }
        public ICommand ProgressSliderOnManipulationCompleted { get; }

        public PlaybackSliderViewModel()
        {
            var _progressSliderIsDragging = false;

            var pbs = Service.PlaybackService;
            pbs.NewTrackPlayed
                .ObserveOnCoreDispatcher()
                .Subscribe(x =>
                {
                    SliderNaturalPosition = x.PlaybackItem.Source.Duration.Value.TotalSeconds;
                });
            pbs.PlaybackPosition
                .ObserveOnCoreDispatcher()
                .Subscribe(x =>
                {
                    if (!_progressSliderIsDragging) SliderCurrentPosition = x.Position.TotalSeconds;
                });

            ProgressSliderOnManipulationStarting = ReactiveCommand.Create<object>(_ => _progressSliderIsDragging = true);
            ProgressSliderOnManipulationCompleted = ReactiveCommand.Create<object>(_ =>
            {
                _progressSliderIsDragging = false;

                pbs.Seek(TimeSpan.FromSeconds(SliderCurrentPosition));
            });
        }
    }
}