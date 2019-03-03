using Magentaize.FluentPlayer.Core;
using Magentaize.FluentPlayer.Data;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Magentaize.FluentPlayer.ViewModels
{
    public class FullPlayerPageArtistsControlViewModel : BindableBase
    {
        private ObservableCollection<IGrouping<string, Track>> _tracksCvsSource;

        public ObservableCollection<IGrouping<string, Track>> TracksCvsSource
        {
            get => _tracksCvsSource;
            set => SetProperty(ref _tracksCvsSource, value);
        }

        public ICommand PlaySelectedTrackCommand { get; }

        public FullPlayerPageArtistsControlViewModel()
        {
            PlaySelectedTrackCommand = new DelegateCommand(ExecutePlaySelectedTrack);
        }

        public async Task CreateAsync()
        {
            var tracks = await ServiceFacade.IndexService.GetAllTracksAsync();
            TracksCvsSource =
                new ObservableCollection<IGrouping<string, Track>>(
                    GroupedItem.CreateByAlpha(tracks, t => t.TrackTitle));
        }

        public void ExecutePlaySelectedTrack()
        {

        }
    }
}