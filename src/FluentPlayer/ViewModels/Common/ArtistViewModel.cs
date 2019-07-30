using DynamicData;
using Magentaize.FluentPlayer.Data;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Linq;

namespace Magentaize.FluentPlayer.ViewModels.Common
{
    public sealed class ArtistViewModel : ReactiveObject
    {
        public long Id { get; }

        [Reactive]
        public string Name { get; set; }

        public IObservableList<AlbumViewModel> Albums => _albums.AsObservableList();

        private ISourceList<AlbumViewModel> _albums;

        private ArtistViewModel(Artist artist)
        {
            Id = artist.Id;
            Name = artist.Name;
        }

        public ArtistViewModel AddAlbum(AlbumViewModel album)
        {
            album.Artist = this;
            _albums.Add(album);

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
            vm._albums = SourceList.CreateFromEnumerable(albums);

            return vm;
        }
    }
}