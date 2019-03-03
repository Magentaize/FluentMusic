using System.Collections.Generic;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Linq;
using Magentaize.FluentPlayer.Data;

namespace Magentaize.FluentPlayer.ViewModels
{
    public class TrackListViewViewModel : BindableBase
    {
        private ObservableCollection<GroupedTrack> _cvsSource;

        public ObservableCollection<GroupedTrack> CvsSource
        {
            get => _cvsSource;
            set => SetProperty(ref _cvsSource, value);
        }

        public void FillCvsSource(IEnumerable<Track> tracks)
        {
            var group = tracks.GroupBy(t => t.TrackTitle.Substring(0, 1)).OrderBy(g => g.Key).Select(g=>new GroupedTrack(g));
            CvsSource = new ObservableCollection<GroupedTrack>(group);
        }
    }
}