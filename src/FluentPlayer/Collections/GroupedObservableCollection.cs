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
    public class GroupedObservableCollection<TKey, TInElement, TOutElement> : ObservableCollection<Grouping<TKey, TOutElement>>
            where TKey : IComparable<TKey>
    {
        private readonly Func<TOutElement, TKey> _selector;
        private readonly Func<TInElement, TOutElement> _mapper;
        private readonly IDictionary<TInElement, TOutElement> _mapDict = new Dictionary<TInElement, TOutElement>();

        /// <summary>
        /// This is used as an optimisation for when items are likely to be added in key order and there is a good probability
        /// that when an item is added, then next one will be in the same grouping.
        /// </summary>
        private Grouping<TKey, TOutElement> _lastEffectedGroup;

        public GroupedObservableCollection(Func<TInElement, TOutElement> mapper, Func<TOutElement, TKey> selector)
        {
            _selector = selector;
            _mapper = mapper;
        }

        public void Add(TInElement item)
        {
            var mapped = _mapper(item);
            _mapDict.Add(item, mapped);

            var key = _selector(mapped);
            FindOrCreateGroup(key).Add(mapped);
        }

        internal ObservableCollection<TInElement> InnerObservableCollection;

        internal async void InnerObservableCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            await e.NewItems.ToAsyncEnumerable().ForEachAsync(i => Add((TInElement)i));
            await e.OldItems.ToAsyncEnumerable().ForEachAsync(i => Remove((TInElement)i));
        }

        public async Task<GroupedObservableCollection<TKey, TInElement, TOutElement>> AddRangeAsync(IEnumerable<TInElement> collection)
        {
            await collection.ToAsyncEnumerable().ForEachAsync(i => { Add(i); });

            return this;
        }

        public IEnumerable<TKey> Keys => this.Select(i => i.Key);

        public bool Remove(TInElement item)
        {
            if (!_mapDict.TryGetValue(item, out var mapped))
            {
                return false;
            }
            var key = _selector(mapped);
            var group = TryFindGroup(key);
            var success = group != null && group.Remove(mapped);

            if (group != null && group.Count == 0)
            {
                Remove(group);
                _mapDict.Remove(item);
                _lastEffectedGroup = null;
            }

            return success;
        }

        private Grouping<TKey, TOutElement> TryFindGroup(TKey key)
        {
            if (_lastEffectedGroup != null && _lastEffectedGroup.Key.Equals(key))
            {
                return _lastEffectedGroup;
            }

            var group = this.FirstOrDefault(i => i.Key.Equals(key));

            _lastEffectedGroup = group;

            return group;
        }

        private Grouping<TKey, TOutElement> FindOrCreateGroup(TKey key)
        {
            if (_lastEffectedGroup != null && _lastEffectedGroup.Key.Equals(key))
            {
                return _lastEffectedGroup;
            }

            var match = this.Select((group, index) => new { group, index }).FirstOrDefault(i => i.group.Key.CompareTo(key) >= 0);
            Grouping<TKey, TOutElement> result;
            if (match == null)
            {
                // Group doesn't exist and the new group needs to go at the end
                result = new Grouping<TKey, TOutElement>(key);
                base.Add(result);
            }
            else if (!match.group.Key.Equals(key))
            {
                // Group doesn't exist, but needs to be inserted before an existing one
                result = new Grouping<TKey, TOutElement>(key);
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
        public static async Task<GroupedObservableCollection<TKey, TInElement, TOutElement>> CreateAsync<TKey, TInElement, TOutElement>
        (IEnumerable<TInElement> collection, Func<TInElement, TOutElement> mapper, Func<TOutElement, TKey> selector)
        where TKey:IComparable<TKey>
        {
            var ret = new GroupedObservableCollection<TKey, TInElement, TOutElement>(mapper, selector);
            await ret.AddRangeAsync(collection);

            if (collection is ObservableCollection<TInElement> ob)
            {
                ret.InnerObservableCollection = ob;
                ret.InnerObservableCollection.CollectionChanged += ret.InnerObservableCollection_CollectionChanged;
            }

            return ret;
        }
    }
}