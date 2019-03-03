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

        public async Task ShowAllTracksAsync()
        {
            TrackList = new ObservableCollection<Track>(await ServiceFacade.IndexService.GetAllTracksAsync());
        }
    }
}