using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Magentaize.FluentPlayer.Data;
using Prism.Mvvm;

namespace Magentaize.FluentPlayer.ViewModels
{
    public class ArtistListViewViewModel : BindableBase
    {
        private ObservableCollection<GroupedArtist> _cvsSource;

        public ObservableCollection<GroupedArtist> CvsSource
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

        public void FillCvsSource(IEnumerable<Artist> tracks)
        {
            var group = tracks.GroupBy(t => t.Name.Substring(0, 1)).OrderBy(g => g.Key).Select(g => new GroupedArtist(g));
            CvsSource = new ObservableCollection<GroupedArtist>(group);

            SelectedIndex = -1;
        }
    }
}