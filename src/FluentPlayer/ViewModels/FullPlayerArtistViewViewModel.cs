using Magentaize.FluentPlayer.Collections;
using Magentaize.FluentPlayer.Core;
using Magentaize.FluentPlayer.ViewModels.DataViewModel;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Prism.Commands;

namespace Magentaize.FluentPlayer.ViewModels
{
    internal sealed class FullPlayerArtistViewViewModel : BindableBase
    {
        private GroupedObservableCollection<char, TrackViewModel> _trackCvsSource;

        public GroupedObservableCollection<char, TrackViewModel> TrackCvsSource
        {
            get => _trackCvsSource;
            set => SetProperty(ref _trackCvsSource, value);
        }

        private ObservableCollection<AlbumViewModel> _albums;

        public ObservableCollection<AlbumViewModel> Albums
        {
            get => _albums;
            set => SetProperty(ref _albums, value);
        }

        private int _albumSelectedIndex;

        public int AlbumSelectedIndex
        {
            get => _albumSelectedIndex;
            set => SetProperty(ref _albumSelectedIndex, value);
        }

        private GroupedObservableCollection<char, ArtistViewModel> _artistCvsSource;

        public GroupedObservableCollection<char, ArtistViewModel> ArtistCvsSource
        {
            get => _artistCvsSource;
            set => SetProperty(ref _artistCvsSource, value);
        }

        private int _artistSelectedIndex;

        public int ArtistSelectedIndex
        {
            get => _artistSelectedIndex;
            set => SetProperty(ref _artistSelectedIndex, value);
        }

        public ICommand RestoreArtistsCommand { get; }

        public FullPlayerArtistViewViewModel()
        {
            RestoreArtistsCommand = new DelegateCommand(async ()=> await RestoreArtistsAsync());
        }

        private CombinedDbViewModel _originalData;

        public async Task FillCvsSourceAsync(CombinedDbViewModel data)
        {
            _originalData = data;

            await RestoreCvsSourceAsync();
        }

        public async Task PlayAsync(TrackViewModel track)
        {
            await ServiceFacade.PlaybackService.PlayAsync(track.Track);
            track.IsPlaying = true;
        }

        public async Task RestoreCvsSourceAsync()
        {
            TrackCvsSource = await GroupedObservableCollection.CreateAsync(_originalData.Tracks, t => t.Track.TrackTitle[0]);

            Albums = _originalData.Albums;
            AlbumSelectedIndex = -1;

            ArtistCvsSource = await GroupedObservableCollection.CreateAsync(_originalData.Artists, a => a.Artist.Name[0]);
            ArtistSelectedIndex = -1;
        }

        public async Task ArtistItem_OnTapped(ArtistViewModel artist)
        {
            Albums.Clear();
            foreach (var a in artist.Artist.Albums)
            {
                Albums.Add(new AlbumViewModel(a));
            }

            AlbumSelectedIndex = -1;

            TrackCvsSource.Clear();
            foreach (var t in artist.Artist.Tracks)
            {
                TrackCvsSource.Add(new TrackViewModel(t));
            }
        }

        private async Task RestoreArtistsAsync()
        {
            await RestoreCvsSourceAsync();
        }
    }
}