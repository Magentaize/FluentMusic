using DynamicData;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Windows.UI.Xaml.Controls
{
    public static class ControlExtension
    {
        public static IObservableList<TElement> SelectedItemsAsObservableList<TElement>(this ListViewBase control)
        {
            return control.Events()
                   .SelectionChanged
                   .Select(x => new ChangeSet<TElement>()
                   {
                       new Change<TElement>(ListChangeReason.RemoveRange, x.RemovedItems.Cast<TElement>().ToList()),
                       new Change<TElement>(ListChangeReason.AddRange, x.AddedItems.Cast<TElement>().ToList()),
                   })
                   .AsObservableList();
        }

        public static IObservableList<TElement> AsObservableList<TElement>(this IObservable<SelectionChangedEventArgs> source)
        {
            return source
                .Select(x => new ChangeSet<TElement>()
                {
                    new Change<TElement>(ListChangeReason.RemoveRange, x.RemovedItems.Cast<TElement>().ToList()),
                    new Change<TElement>(ListChangeReason.AddRange, x.AddedItems.Cast<TElement>().ToList()),
                })
                .AsObservableList();
        }

        public static IObservable<IChangeSet<TElement>> SelectedItemsAsObservable<TControl, TElement>(this TControl control) where TControl: ListViewBase
        {
            return Observable.Create<IChangeSet<TElement>>(o =>
            {
                var disposes = new CompositeDisposable();
                var items = new SourceList<TElement>();

                control.Events()
                    .SelectionChanged
                    .Subscribe(x =>
                    {
                        items.Edit(y =>
                        {
                            y.RemoveMany(x.RemovedItems.Cast<TElement>());
                            y.AddRange(x.AddedItems.Cast<TElement>());
                        });
                    })
                    .DisposeWith(disposes);
                items.Connect()
                    .Subscribe(o)
                    .DisposeWith(disposes);

                return disposes;
            });
        }
    }
}
