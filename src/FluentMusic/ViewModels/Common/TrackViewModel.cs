using FluentMusic.Data;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace FluentMusic.ViewModels.Common
{
    public class TrackViewModel : ReactiveObject
    {
        public AlbumViewModel Album { get; set; }

        public long Id { get; }

        [Reactive]
        public string Path { get; set; }

        [Reactive]
        public string Title { get; set; }

        [Reactive]
        public bool IsPlaying { get; set; }

        private TrackViewModel(Track track)
        {
            Id = track.Id;
            Path = track.Path;
            Title = track.TrackTitle;
        }

        public static TrackViewModel Create(Track track)
        {
            var vm = new TrackViewModel(track);
            return vm;
        }
    }
}