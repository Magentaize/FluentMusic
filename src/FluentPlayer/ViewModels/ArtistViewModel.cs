using Magentaize.FluentPlayer.Data;
using Prism.Mvvm;

namespace Magentaize.FluentPlayer.ViewModels
{
    public class ArtistViewModel : BindableBase
    {
        public Artist Artist { get; }

        public ArtistViewModel(Artist artist)
        {
            Artist = artist;
        }
    }
}