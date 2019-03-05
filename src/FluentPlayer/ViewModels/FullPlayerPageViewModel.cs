using Magentaize.FluentPlayer.Core;
using Prism.Mvvm;
using System.Threading.Tasks;

namespace Magentaize.FluentPlayer.ViewModels
{
    public class FullPlayerPageViewModel : BindableBase
    {
        private CombinedDbViewModel _dataSource;

        public CombinedDbViewModel DataSource
        {
            get => _dataSource;
            set => SetProperty(ref _dataSource, value);
        }

        public async Task ShowAllTracksAsync()
        {
            var tracks = await ServiceFacade.IndexService.GetAllTracksAsync();
            var artists = await ServiceFacade.IndexService.GetAllArtistsAsync();
            var albums = await ServiceFacade.IndexService.GetAllAlbumsAsync();

            DataSource = new CombinedDbViewModel()
            {
                Albums = albums,
                Artists = artists,
                Tracks = tracks,
            };
        }
    }
}