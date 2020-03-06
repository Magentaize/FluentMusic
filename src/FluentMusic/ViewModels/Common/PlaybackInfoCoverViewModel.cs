using FluentMusic.Core;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using static FluentMusic.Controls.SlideTile;

namespace FluentMusic.ViewModels.Common
{
    public class PlaybackInfoCoverViewModel : ReactiveObject
    {
        [ObservableAsProperty]
        public PlaybackInfoCoverThumbnailViewModel Thumbnail { get; } 

        [ObservableAsProperty]
        public SlideDirection Direction { get; private set; }

        public PlaybackInfoCoverViewModel()
        {
            var pbs = Service.PlaybackService;
            pbs.NewTrackPlayed
                .Select(x => x.IsPlayingPreviousTrack ? SlideDirection.Down : SlideDirection.Up)
                .ObservableOnCoreDispatcher()
                .ToPropertyEx(this, x => x.Direction, SlideDirection.Up);
            pbs.NewTrackPlayed
                .Select(x => x.Track.Album)
                .DistinctUntilChanged()
                .Select(x => new PlaybackInfoCoverThumbnailViewModel { Uri = x.AlbumCoverFsPath })
                .ObservableOnCoreDispatcher()
                .ToPropertyEx(this, x => x.Thumbnail);
        }
    }
}
