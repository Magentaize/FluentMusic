using Magentaize.FluentPlayer.Collections;
using Magentaize.FluentPlayer.Data;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Magentaize.FluentPlayer.ViewModels
{
    public class TrackListViewViewModel : BindableBase
    {
        private GroupedObservableCollection<char, Track> _cvsSource;

        public GroupedObservableCollection<char, Track> CvsSource
        {
            get => _cvsSource;
            set => SetProperty(ref _cvsSource, value);
        }

        public async Task FillCvsSource(ObservableCollection<Track> tracks)
        {
            CvsSource = await GroupedObservableCollection.CreateAsync(tracks, t => t.TrackTitle[0]);
        }
    }
}