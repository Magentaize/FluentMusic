using Magentaize.FluentPlayer.Core;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive;
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
            pbs.NewTrackPlayed
                .Select(x => x.IsPlayingPreviousTrack ? SlideDirection.Down : SlideDirection.Up)
                .ObservableOnCoreDispatcher()
                .ToPropertyEx(this, x => x.Direction, SlideDirection.Up);
            pbs.NewTrackPlayed
                .DistinctUntilChanged(x => x.Track)
                .Select(x => new PlaybackInfoTextPropertyViewModel
                {
                    Title = x.Track.Title,
                    Artist = x.Track.Album.Artist.Name,
                    CurrentPosition = @"00:00",
                    NaturalPosition = $"{x.PlaybackItem.Source.Duration:mm\\:ss}",
                })
                .ObservableOnCoreDispatcher()
                .ToPropertyEx(this, x => x.Property);
            pbs.PlaybackPosition
                .Select(x => $"{x.Position:mm\\:ss}")
                .ObservableOnCoreDispatcher()
                .Subscribe(x => Property.CurrentPosition = x);
        }
    }
}
