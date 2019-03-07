using System;
using System.Collections;
using System.Collections.Async;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace Magentaize.FluentPlayer.Collections
{
    public class GroupedObservableCollection<TKey, TElement> : ObservableCollection<Grouping<TKey, TElement>>
            where TKey : IComparable<TKey>
    {
        private readonly Func<TElement, TKey> _selector;

        /// <summary>
        /// This is used as an optimisation for when items are likely to be added in key order and there is a good probability
        /// that when an item is added, then next one will be in the same grouping.
        /// </summary>
        private Grouping<TKey, TElement> _lastEffectedGroup;

        public GroupedObservableCollection(Func<TElement, TKey> selector)
        {
            _selector = selector;
        }

        public void Add(TElement item)
        {
            var key = _selector(item);
            FindOrCreateGroup(key).Add(item);
        }

        internal ObservableCollection<TElement> InnerObservableCollection;

        internal async void InnerObservableCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            await e.NewItems.ToAsyncEnumerable().ForEachAsync(i => Add((TElement)i));
            await e.OldItems.ToAsyncEnumerable().ForEachAsync(i => Remove((TElement)i));
        }

        public async Task<GroupedObservableCollection<TKey, TElement>> AddRangeAsync(IEnumerable<TElement> collection)
        {
            await collection.ToAsyncEnumerable().ForEachAsync(i => { Add(i); });

            return this;
        }

        public IEnumerable<TKey> Keys => this.Select(i => i.Key);

        public bool Remove(TElement item)
        {
            var key = _selector(item);
            var group = TryFindGroup(key);
            var success = group != null && group.Remove(item);

            if (group != null && group.Count == 0)
            {
                Remove(group);
                _lastEffectedGroup = null;
            }

            return success;
        }

        private Grouping<TKey, TElement> TryFindGroup(TKey key)
        {
            if (_lastEffectedGroup != null && _lastEffectedGroup.Key.Equals(key))
            {
                return _lastEffectedGroup;
            }

            var group = this.FirstOrDefault(i => i.Key.Equals(key));

            _lastEffectedGroup = group;

            return group;
        }

        private Grouping<TKey, TElement> FindOrCreateGroup(TKey key)
        {
            if (_lastEffectedGroup != null && _lastEffectedGroup.Key.Equals(key))
            {
                return _lastEffectedGroup;
            }

            var match = this.Select((group, index) => new { group, index }).FirstOrDefault(i => i.group.Key.CompareTo(key) >= 0);
            Grouping<TKey, TElement> result;
            if (match == null)
            {
                // Group doesn't exist and the new group needs to go at the end
                result = new Grouping<TKey, TElement>(key);
                base.Add(result);
            }
            else if (!match.group.Key.Equals(key))
            {
                // Group doesn't exist, but needs to be inserted before an existing one
                result = new Grouping<TKey, TElement>(key);
                Insert(match.index, result);
            }
            else
            {
                result = match.group;
            }

            _lastEffectedGroup = result;

            return result;
        }
    }

    public class GroupedObservableCollection
    {
        public static async Task<GroupedObservableCollection<TKey, TElement>> CreateAsync<TKey, TElement>
        (IEnumerable<TElement> collection, Func<TElement, TKey> selector)
        where TKey:IComparable<TKey>
        {
            var ret = new GroupedObservableCollection<TKey, TElement>(selector);
            await ret.AddRangeAsync(collection);

            if (collection is ObservableCollection<TElement> ob)
            {
                ret.InnerObservableCollection = ob;
                ret.InnerObservableCollection.CollectionChanged += ret.InnerObservableCollection_CollectionChanged;
            }

            return ret;
        }
    }
}