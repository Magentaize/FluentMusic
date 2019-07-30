using Magentaize.FluentPlayer.Core;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Linq;
using System.Reactive.Linq;
using static Magentaize.FluentPlayer.Controls.SlideTile;

namespace Magentaize.FluentPlayer.ViewModels.Common
{
    public class PlaybackInfoCoverViewModel : ReactiveObject
    {
        [ObservableAsProperty]
        public PlaybackInfoCoverThumbnailViewModel Thumbnail { get; } 

        [ObservableAsProperty]
        public SlideDirection Direction { get; private set; }

        public PlaybackInfoCoverViewModel()
        {
            var pbs = ServiceFacade.PlaybackService;
            pbs.NewTrackPlayed
                .Select(x => x.IsPlayingPreviousTrack ? SlideDirection.Down : SlideDirection.Up)
                .ToPropertyEx(this, x => x.Direction, SlideDirection.Up);
            //pbs.CurrentTrack
            //    .DistinctUntilChanged(x => x.Track.Album)
            //    .Select(x => ViewModelAccessor.AlbumVmSource.First(y => y.Album == x.Track.Album))
            //    .Select(x => new PlaybackInfoCoverThumbnailViewModel { Uri = x.AlbumCoverFsPath.Value })
            //    .ToPropertyEx(this, x => x.Thumbnail);
        }
    }
}
