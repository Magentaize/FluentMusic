using Magentaize.FluentPlayer.Collections;
using Magentaize.FluentPlayer.Data;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Magentaize.FluentPlayer.ViewModels
{
    public class ArtistListViewViewModel : BindableBase
    {
        private GroupedObservableCollection<char, Artist> _cvsSource;

        public GroupedObservableCollection<char, Artist> CvsSource
        {
            get => _cvsSource;
            set => SetProperty(ref _cvsSource, value);
        }

        private int _selectedIndex;

        public int SelectedIndex
        {
            get => _selectedIndex;
            set => SetProperty(ref _selectedIndex, value);
        }

        public async Task FillCvsSource(ObservableCollection<Artist> artists)
        {
            CvsSource = await GroupedObservableCollection.CreateAsync(artists, t => t.Name[0]);
            SelectedIndex = -1;
        }
    }
}