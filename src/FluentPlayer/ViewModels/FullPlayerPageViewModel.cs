using Magentaize.FluentPlayer.Core;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive.Linq;

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

        public FullPlayerPageViewModel()
        {
            var pbs = ServiceFacade.PlaybackService;
            pbs.IsPlaying.ToPropertyEx(this, x => x.IsPlaying);
            pbs.CurrentTrack.Subscribe(x =>
            {
                TrackTitle = x.Track.TrackTitle;
                TrackArtist = x.Track.Artist.Name;
                CurrentPosition = @"00:00";
                NaturalPosition = $"{x.PlaybackItem.Source.Duration:mm\\:ss}";
            });
            pbs.PlaybackPosition.Subscribe(x =>
            {
                CurrentPosition = $"{x.Position:mm\\:ss}";
            });
        }
    }
}