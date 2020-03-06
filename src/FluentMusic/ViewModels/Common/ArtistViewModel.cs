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

        public IObservableCache<AlbumViewModel, long> Albums => _albums.AsObservableCache();

        private ISourceCache<AlbumViewModel, long> _albums = new SourceCache<AlbumViewModel, long>(x => x.Id);

        private ArtistViewModel(Artist artist)
        {
            Id = artist.Id;
            Name = artist.Name;
        }

        public ArtistViewModel AddAlbum(AlbumViewModel album)
        {
            album.Artist = this;
            _albums.AddOrUpdate(album);

            return this;
        }

        public static ArtistViewModel Create(Artist artist)
        {
            var vm = new ArtistViewModel(artist);
            var albums = artist.Albums
                .Select(x =>
                {
                    var v = AlbumViewModel.Create(x);
                    v.Artist = vm;

                    return v;
                });
            vm._albums.Edit(x => x.AddOrUpdate(albums));

            return vm;
        }
    }
}