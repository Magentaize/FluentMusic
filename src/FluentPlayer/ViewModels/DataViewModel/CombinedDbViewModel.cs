using System.Collections.ObjectModel;

namespace Magentaize.FluentPlayer.ViewModels.DataViewModel
{
    public class CombinedDbViewModel
    {
        public ObservableCollection<TrackViewModel> Tracks { get; set; }
        public ObservableCollection<ArtistViewModel> Artists { get; set; }
        public ObservableCollection<AlbumViewModel> Albums { get; set; }
    }
}