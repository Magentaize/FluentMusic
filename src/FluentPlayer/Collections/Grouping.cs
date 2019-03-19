using DynamicData;
using DynamicData.Binding;
using System;
using System.Linq;

namespace Magentaize.FluentPlayer.Collections
{
    public class Grouping<TKey, TElement> : ObservableCollectionExtended<TElement>, IGrouping<TKey, TElement>
    {
        public Grouping(IGroup<TElement, TKey> group)         
        {
            Key = group.GroupKey;
            group.List.Connect().Bind(this).Subscribe();
        }

        public TKey Key { get; }
    }
}