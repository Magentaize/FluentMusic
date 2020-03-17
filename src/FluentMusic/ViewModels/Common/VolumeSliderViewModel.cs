using FluentMusic.Core.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Windows.UI.Xaml;

namespace FluentMusic.ViewModels.Common
{
    public sealed class VolumeSliderViewModel : ReactiveObject
    {
        [Reactive]
        public int CurrentVolume { get; set; }

        public ISubject<RoutedEventArgs> SliderManipulationStarted { get; } = new Subject<RoutedEventArgs>();
        public ISubject<RoutedEventArgs> SliderManipulationCompleted { get; } = new Subject<RoutedEventArgs>();

        private bool _gate = true;

        public VolumeSliderViewModel()
        {
            var locker = new object();
            SliderManipulationStarted
                .Synchronize(locker)
                .Subscribe(_ => _gate = false);
            SliderManipulationCompleted
                .Synchronize(locker)
                .Subscribe(_ => _gate = true);

            Setting.Playback.Volume
                .Where(_=> _gate)
                .ObserveOnCoreDispatcher()
                .Subscribe(x => CurrentVolume = x);

            this.WhenAnyValue(x => x.CurrentVolume)
                // Skip 2 items: 1) the initial value of CurrentVolume
                //               2) the initial value of Setting.Behavior.Volume
                .Skip(2)
                .Subscribe(Setting.Playback.Volume);
        }
    }
}
