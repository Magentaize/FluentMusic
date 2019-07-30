using Magentaize.FluentPlayer.Core;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;

namespace Magentaize.FluentPlayer.ViewModels
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

            var pbs = ServiceFacade.PlaybackService;
            pbs.NewTrackPlayed
                .ObservableOnCoreDispatcher()
                .Subscribe(x =>
                {
                    SliderNaturalPosition = x.PlaybackItem.Source.Duration.Value.TotalSeconds;
                });
            pbs.PlaybackPosition
                .ObservableOnCoreDispatcher()
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