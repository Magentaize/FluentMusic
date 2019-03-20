using Magentaize.FluentPlayer.Core;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive.Linq;
using Windows.UI.Xaml.Controls;

namespace Magentaize.FluentPlayer.ViewModels
{
    public class FullPlayerPageViewModel : ReactiveObject
    {
        [ObservableAsProperty]
        public bool IsPlaying { get; }

        [Reactive]
        public string CurrentPosition { get; set; }
        [Reactive]
        public string NaturalPosition { get; set; }

        [Reactive]
        public string TrackTitle { get; set; }

        [Reactive]
        public string TrackArtist { get; set; }

        [Reactive]
        public double SliderCurrentPosition { get; set; }

        [Reactive]
        public double SliderNaturalPosition { get; set; }

        private bool _progressSliderIsDragging = false;

        public FullPlayerPageViewModel()
        {
            var pbs = ServiceFacade.PlaybackService;
            pbs.IsPlaying.ToPropertyEx(this, x => x.IsPlaying);
            pbs.CurrentTrack.Subscribe(x =>
            {
                TrackTitle = x.Track.TrackTitle;
                TrackArtist = x.Track.Artist.Name;
                CurrentPosition = @"00:00";
                NaturalPosition = $"{x.NaturalDuration:mm\\:ss}";
                SliderNaturalPosition = x.NaturalDuration.TotalSeconds;
            });
            ServiceFacade.PlaybackService.PlaybackPosition.Subscribe(x =>
            {
                CurrentPosition = $"{x.Position:mm\\:ss}";
                if (!_progressSliderIsDragging) SliderCurrentPosition = x.Position.TotalSeconds;
            });
        }

        public void ProgressSlider_OnManipulationStarting(object sender, EventArgs e)
        {
            _progressSliderIsDragging = true;
        }

        public void ProgressSlider_OnManipulationCompleted(object sender, EventArgs e)
        {
            _progressSliderIsDragging = false;

            ServiceFacade.PlaybackService.Seek(TimeSpan.FromSeconds(((Slider)sender).Value));
        }
    }
}