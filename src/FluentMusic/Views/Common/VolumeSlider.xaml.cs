using FluentMusic.Core;
using ReactiveUI.Fody.Helpers;
using Windows.UI.Xaml.Controls;
using System;
using ReactiveUI;
using System.Reactive.Linq;
using Windows.UI.Xaml.Controls.Primitives;

namespace FluentMusic.Views.Common
{
    public sealed partial class VolumeSlider : UserControl
    {
        [Reactive]
        public int MaximumVolume { get; } = 100;

        [Reactive]
        public int MinimumVolume { get; } = 0;

        [Reactive]
        public int CurrentVolume { get; private set; }

        public VolumeSlider()
        {
            InitializeComponent();

            ServiceFacade.PlaybackService.VolumeChanged.Subscribe(x =>
            {
                CurrentVolume = x;
            });

            this.WhenAnyValue(x => x.CurrentVolume)
                .Subscribe(x =>
                {
                    ServiceFacade.PlaybackService.ChangeVolume(x);
                });

            Observable.FromEventPattern<RangeBaseValueChangedEventHandler, RangeBaseValueChangedEventArgs>(
                h => ProgressSlider.ValueChanged += h, h => ProgressSlider.ValueChanged -= h)
                .Subscribe(x => { Console.WriteLine(CurrentVolume); });
        }
    }
}
