using DynamicData.Binding;
using ReactiveUI.Fody.Helpers;
using System.Collections.Generic;

namespace FluentMusic.ViewModels.Common
{
    public class AlbumSorterViewModel
    {
        private static IList<AlbumSorterItem> _items = new List<AlbumSorterItem>
        {
            new AlbumSorterItem("Name",
                SortExpressionComparer<AlbumViewModel>.Ascending(x => x.Title)),
        };

        public AlbumSorterViewModel()
        {
            SelectedItem = _items[0];
        }

        [Reactive]
        public AlbumSorterItem SelectedItem { get; set; }
    }

    public class AlbumSorterItem : BaseSorterItem<AlbumViewModel>
    {
        public AlbumSorterItem(string description, IComparer<AlbumViewModel> comparer)
            : base(description, comparer)
        {
        }
    }

    public class BaseSorterItem<T>
    {
        public string Description { get; }

        public IComparer<T> Comparer { get; }

        public BaseSorterItem(string description, IComparer<T> comparer)
        {
            Description = description;
            Comparer = comparer;
        }
    }
}
