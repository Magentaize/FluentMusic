using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Globalization.Collation;
using Magentaize.FluentPlayer.Data;

namespace Magentaize.FluentPlayer.ViewModels
{
    // It's a workaround for that XAML in UWP doesn't support generic type, so when making binding,
    // you can make x:bind on generated items and must make legacy binding on group key
    public class GroupedItem<T> : ObservableCollection<T>, IGrouping<string, T>
    {
        public string Key { get; }

        public override string ToString()
        {
            //if (Key.IsNullorEmpty())
            //{
            //    return "?";
            //}
            return Key;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public GroupedItem(string key, IEnumerable<T> items) : base(items)
        {
            Key = key;
        }

        public GroupedItem(IGrouping<string, T> group) : base(group)
        {
            Key = group.Key;
        }
    }

    public class GroupedTrack : GroupedItem<Track>
    {
        public GroupedTrack(IGrouping<string, Track> group) : base(group) { }
    }

    public class GroupedItem
    {
        public static IEnumerable<GroupedItem<T>> CreateByAlpha<T>(IEnumerable<T> items, Func<T, string> keySelector)
        {
            var g = new CharacterGroupings("zh-CN");
            var groups = items.GroupBy(i => RemovePinYin(g.Lookup(keySelector(i)))).OrderBy(z => z.Key);
            return groups.Select(i => new GroupedItem<T>(i));
        }

        private static string RemovePinYin(string v)
        {
            var a = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray().Where(x => v.Contains(x)).FirstOrDefault();
            if (a != default(char))
            {
                return a.ToString();
            }
            return v;
        }
    }
}