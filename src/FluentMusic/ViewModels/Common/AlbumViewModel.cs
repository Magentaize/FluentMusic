using DynamicData;
using FluentMusic.Core.Services;
using FluentMusic.Data;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Linq;

namespace FluentMusic.ViewModels.Common
{
    public class AlbumViewModel : ReactiveObject
    {
        private static readonly string DefaultCoverImage = "ms-appx:///Assets/Square150x150Logo.scale-200.png";

        public long Id { get; }

        public ArtistViewModel Artist { get; set; }

        [Reactive]
        public string Title { get; set; }

        [Reactive]
        public string CoverPath { get; set; }

        public ISourceCache<TrackViewModel, long> Tracks { get; } = new SourceCache<TrackViewModel, long>(x => x.Id);

        private AlbumViewModel(ArtistViewModel artistVm, Album album)
        {
            Artist = artistVm;
            Id = album.Id;
            Title = album.Title;
            CoverPath = string.IsNullOrEmpty(album.CoverCacheToken) ? DefaultCoverImage : CacheService.GetCachePath(album.CoverCacheToken);
        }

        public static AlbumViewModel Create(ArtistViewModel artistVm, Album album)
        {
            var vm = new AlbumViewModel(artistVm, album);
            var tracks = album.Tracks
                .Select(x =>
                {
                    var v = TrackViewModel.Create(vm, x);
                    v.Album = vm;

                    return v;
                });
            vm.Tracks.AddOrUpdate(tracks);

            return vm;
        }
    }
}