using DynamicData;
using DynamicData.Binding;
using System;
using System.Linq;
using System.Reactive.Linq;

namespace FluentMusic.ViewModels.Common
{
    public abstract class Grouping<TElement> : ObservableCollectionExtended<TElement>, IGrouping<string, TElement>, IDisposable
    {
        public string Key { get; }
        private IDisposable _disposable;

        public Grouping(IGroup<TElement, string> group)
        {
            Key = group.GroupKey;
            _disposable = group.List.Connect()
                .Bind(this)
                .Subscribe();
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }

    public class GroupArtistViewModel : Grouping<ArtistViewModel>
    {
        public GroupArtistViewModel(IGroup<ArtistViewModel, string> group) : base(group)
        { }
    }

    public class GroupTrackViewModel : Grouping<TrackViewModel>
    {
        public GroupTrackViewModel(IGroup<TrackViewModel, string> group) : base(group)
        { }
    }
}