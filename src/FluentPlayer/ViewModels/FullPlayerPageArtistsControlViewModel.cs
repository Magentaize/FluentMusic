using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Magentaize.FluentPlayer.Core;
using Windows.UI.Xaml.Data;
using Magentaize.FluentPlayer.Data;

namespace Magentaize.FluentPlayer.ViewModels
{
    public class FullPlayerPageArtistsControlViewModel : BindableBase
    {
        //private CollectionViewSource _tracksCvs;

        //public CollectionViewSource TracksCvs
        //{
        //    get => _tracksCvs;
        //    set => SetProperty(ref _tracksCvs, value);
        //}

        private ObservableCollection<IGrouping<string, Track>> _tracksCvsSource;

        public ObservableCollection<IGrouping<string, Track>> TracksCvsSource
        {
            get => _tracksCvsSource;
            set => SetProperty(ref _tracksCvsSource, value);
        }

        public FullPlayerPageArtistsControlViewModel() { }

        public async Task CreateAsync()
        {
            var tracks = await ServiceFacade.IndexService.GetAllTracksAsync();
            TracksCvsSource =
                new ObservableCollection<IGrouping<string, Track>>(
                    GroupedItem.CreateByAlpha(tracks, t => t.TrackTitle));
        }
    }
}