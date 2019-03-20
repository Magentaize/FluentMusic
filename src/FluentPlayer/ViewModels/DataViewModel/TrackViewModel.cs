using Magentaize.FluentPlayer.Data;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;

namespace Magentaize.FluentPlayer.ViewModels.DataViewModel
{
    public class TrackViewModel : ReactiveObject
    {
        public Guid Guid = Guid.NewGuid();

        public Track Track { get; }

        [Reactive]
        public bool IsPlaying { get; set; }

        public TrackViewModel(Track track)
        {
            Track = track;
        }
    }
}