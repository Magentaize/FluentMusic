using DynamicData;
using FluentMusic.Data;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Linq;

namespace FluentMusic.ViewModels.Common
{
    public sealed class ArtistViewModel : ReactiveObject
    {
        public long Id { get; }

        [Reactive]
        public string Name { get; set; }

        public ISourceCache<AlbumViewModel, long> Albums { get; } = new SourceCache<AlbumViewModel, long>(x => x.Id);

        private ArtistViewModel(Artist artist)
        {
            Id = artist.Id;
            Name = artist.Name;
        }

        public static ArtistViewModel Create(Artist artist)
        {
            var vm = new ArtistViewModel(artist);
            var albums = artist.Albums
                .Select(x =>
                {
                    var v = AlbumViewModel.Create(vm, x);
                    v.Artist = vm;

                    return v;
                });
            vm.Albums.AddOrUpdate(albums);

            return vm;
        }
    }
}