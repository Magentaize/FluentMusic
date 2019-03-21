using Magentaize.FluentPlayer.Core;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using static Microsoft.Toolkit.Uwp.UI.Controls.RotatorTile;

namespace Magentaize.FluentPlayer.ViewModels.Common
{
    public class PlaybackInfoCoverViewModel : ReactiveObject
    {
        public ObservableCollection<string> Thumbnail { get; private set; } = new ObservableCollection<string>();

        [ObservableAsProperty]
        public RotateDirection Direction { get; private set; }

        public PlaybackInfoCoverViewModel()
        {
            var pbs = ServiceFacade.PlaybackService;
            pbs.CurrentTrack
                .Select(x => x.IsPlayingPreviousTrack ? RotateDirection.Down : RotateDirection.Up)
                .ToPropertyEx(this, x => x.Direction);
            pbs.CurrentTrack
                .Select(x =>
                {
                    var _ = ViewModelAccessor.AlbumVmSource.First(y => y.Album == x.Track.Album).AlbumCoverFsPath.Value;
                    return _;
                })
                .Subscribe(x =>
                {
                    if (Thumbnail.Count >= 2)
                    {
                        Thumbnail.RemoveAt(0);
                    }
                    Thumbnail.Add(x);
                });
        }
    }
}
