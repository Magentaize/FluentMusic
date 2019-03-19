using Magentaize.FluentPlayer.Data;
using ReactiveUI;

namespace Magentaize.FluentPlayer.ViewModels.DataViewModel
{
    public class ArtistViewModel : ReactiveObject
    {
        public Artist Artist { get; }

        public ArtistViewModel(Artist artist)
        {
            Artist = artist;
        }
    }
}