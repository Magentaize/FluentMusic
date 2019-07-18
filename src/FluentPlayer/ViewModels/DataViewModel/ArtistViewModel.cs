using DynamicData;
using Magentaize.FluentPlayer.Data;
using ReactiveUI;

namespace Magentaize.FluentPlayer.ViewModels.DataViewModel
{
    public class ArtistViewModel : ReactiveObject
    {
        public Artist Artist { get; }

        public ISourceList<AlbumViewModel> AlbumViewModels { get; } = new SourceList<AlbumViewModel>();

        public ArtistViewModel(Artist artist)
        {
            Artist = artist;
        }
    }
}