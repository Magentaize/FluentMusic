using Magentaize.FluentPlayer.Data;
using System.Collections.ObjectModel;

namespace Magentaize.FluentPlayer.ViewModels
{
    public class CombinedDbViewModel
    {
        public ObservableCollection<Track> Tracks { get; set; }
        public ObservableCollection<Artist> Artists { get; set; }
        public ObservableCollection<Album> Albums { get; set; }
    }
}