using DynamicData;
using DynamicData.Binding;
using Magentaize.FluentPlayer.ViewModels.DataViewModel;
using System;
using System.Diagnostics;
using System.Reactive.Linq;

namespace Magentaize.FluentPlayer.Collections
{
    public class Grouping<TElement> 
    {
        public IObservableCollection<TElement> Items { get; } = new ObservableCollectionExtended<TElement>();

        protected Grouping() { }

        public Grouping(IGroup<TElement, string> group)         
        {
        }

        public string Key { get; set; }

        public override string ToString()
        {
            return base.ToString();
        }
    }

    public class GroupArtistViewModel : Grouping<ArtistViewModel>
    {
        public GroupArtistViewModel(IGroup<ArtistViewModel, string> group)
        {
            Key = group.GroupKey;
            group.List.Connect()
                .Sort(SortExpressionComparer<ArtistViewModel>.Ascending(x => x.Artist.Name))
                .ObserveOnDispatcher()
                .Bind(Items)
                .DisposeMany()
                .Subscribe(x => Console.WriteLine(x), ex => { Debugger.Break(); });
        }
    }
}