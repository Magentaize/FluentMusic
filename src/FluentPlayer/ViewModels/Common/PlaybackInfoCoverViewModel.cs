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
            pbs.CurrentTrack
                .Select(x => x.IsPlayingPreviousTrack ? SlideDirection.Down : SlideDirection.Up)
                .ToPropertyEx(this, x => x.Direction, SlideDirection.Up);
            pbs.CurrentTrack
                .Select(x =>
                {
                    var _ = ViewModelAccessor.AlbumVmSource.First(y => y.Album == x.Track.Album).AlbumCoverFsPath.Value;
                    return new PlaybackInfoCoverThumbnailViewModel
                    {
                        Uri = _
                    };
                })
                .ToPropertyEx(this, x => x.Thumbnail);
        }
    }
}
