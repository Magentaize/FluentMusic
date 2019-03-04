using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Magentaize.FluentPlayer.Core;
using Magentaize.FluentPlayer.Data;
using Prism.Mvvm;

namespace Magentaize.FluentPlayer.ViewModels
{
    public class FullPlayerPageViewModel : BindableBase
    {
        private ObservableCollection<Track> _trackList;

        public ObservableCollection<Track> TrackList
        {
            get => _trackList;
            set => SetProperty(ref _trackList, value);
        }

        private ObservableCollection<Artist> _artistList;

        public ObservableCollection<Artist> ArtistList
        {
            get => _artistList;
            set => SetProperty(ref _artistList, value);
        }

        private ObservableCollection<Album> _albums;

        public ObservableCollection<Album> Albums
        {
            get => _albums;
            set => SetProperty(ref _albums, value);
        }

        public async Task ShowAllTracksAsync()
        {
            TrackList = ServiceFacade.IndexService.GetAllTracksAsync();
            //await ServiceFacade.PlaybackService.PlayAsync(TrackList[0]);
            ArtistList = new ObservableCollection<Artist>(await ServiceFacade.IndexService.GetAllArtistsAsync());
            Albums = ServiceFacade.IndexService.GetAllAlbums();
        }
    }
}