using Magentaize.FluentPlayer.Core;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive.Linq;
using static Magentaize.FluentPlayer.Controls.SlideTile;

namespace Magentaize.FluentPlayer.ViewModels.Common
{
    public class PlaybackInfoTextViewModel : ReactiveObject
    { 
        [ObservableAsProperty]
        public PlaybackInfoTextPropertyViewModel Property { get; set; }

        [ObservableAsProperty]
        public SlideDirection Direction { get; set; }

        public PlaybackInfoTextViewModel()
        {
            var pbs = ServiceFacade.PlaybackService;
            pbs.CurrentTrack
                .Select(x => x.IsPlayingPreviousTrack ? SlideDirection.Down : SlideDirection.Up)
                .ToPropertyEx(this, x => x.Direction, SlideDirection.Up);
            pbs.CurrentTrack
                .DistinctUntilChanged(x => x.Track)
                .Select(x => new PlaybackInfoTextPropertyViewModel
                {
                    Title = x.Track.TrackTitle,
                    Artist = x.Track.Artist.Name,
                    CurrentPosition = @"00:00",
                    NaturalPosition = $"{x.PlaybackItem.Source.Duration:mm\\:ss}",
                })
                .ToPropertyEx(this, x => x.Property);
            pbs.PlaybackPosition
                .Select(x => $"{x.Position:mm\\:ss}")
                .Subscribe(x => Property.CurrentPosition = x);
        }
    }
}
