using Magentaize.FluentPlayer.Collections;
using Magentaize.FluentPlayer.Core;
using Magentaize.FluentPlayer.Data;
using Prism.Mvvm;
using System.Collections.Async;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Magentaize.FluentPlayer.ViewModels.DataViewModel;

namespace Magentaize.FluentPlayer.ViewModels
{
    internal sealed class FullPlayerArtistViewViewModel : BindableBase
    {
        private GroupedObservableCollection<char, Track, TrackViewModel> _trackCvsSource;

        public GroupedObservableCollection<char, Track, TrackViewModel> TrackCvsSource
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

        private GroupedObservableCollection<char, Artist, ArtistViewModel> _artistCvsSource;

        public GroupedObservableCollection<char, Artist, ArtistViewModel> ArtistCvsSource
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

        private CombinedDbViewModel _originalData;

        public async Task FillCvsSourceAsync(CombinedDbViewModel data)
        {
            _originalData = data;

            TrackCvsSource = await GroupedObservableCollection.CreateAsync(data.Tracks, t => new TrackViewModel(t),
                t => t.Track.TrackTitle[0]);

            Albums = new ObservableCollection<AlbumViewModel>(data.Albums.Select(a => new AlbumViewModel(a)));
            AlbumSelectedIndex = -1;

            ArtistCvsSource = await GroupedObservableCollection.CreateAsync(data.Artists, a => new ArtistViewModel(a),
                a => a.Artist.Name[0]);
            ArtistSelectedIndex = -1;
        }

        public async Task PlayAsync(TrackViewModel track)
        {
            await ServiceFacade.PlaybackService.PlayAsync(track.Track);
            track.IsPlaying = true;
        }
    }
}