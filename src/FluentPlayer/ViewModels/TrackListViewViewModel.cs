using System.Collections.Generic;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Linq;
using Magentaize.FluentPlayer.Data;

namespace Magentaize.FluentPlayer.ViewModels
{
    public class TrackListViewViewModel : BindableBase
    {
        private ObservableCollection<GroupedTrack> _trackCvsSource;

        public ObservableCollection<GroupedTrack> TrackCvsSource
        {
            get => _trackCvsSource;
            set => SetProperty(ref _trackCvsSource, value);
        }

        public void FillCvsSource(IEnumerable<Track> tracks)
        {
            var group = tracks.GroupBy(t => t.TrackTitle.Substring(0, 1)).OrderBy(g => g.Key).Select(g=>new GroupedTrack(g));
            TrackCvsSource = new ObservableCollection<GroupedTrack>(group);
        }
    }
}