using DynamicData.List.Internal;
using System;
using System.Linq;
using System.Reactive.Linq;

namespace DynamicData
{
    public static class ObservableListExtension
    {
        public static IObservable<IChangeSet<TDestination>> MergeManyEx<T, TDestination>(
                   this IObservable<IChangeSet<T>> source,
                   Func<T, IObservable<IChangeSet<TDestination>>> observableSelector)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (observableSelector == null)
            {
                throw new ArgumentNullException(nameof(observableSelector));
            }

            return new MergeManyEx<T, TDestination>(source, observableSelector).Run();
        }
    }
}

namespace DynamicData.List.Internal
{
    internal sealed class MergeManyEx<T, TDestination>
    {
        private readonly IObservable<IChangeSet<T>> _source;
        private readonly Func<T, IObservable<IChangeSet<TDestination>>> _observableSelector;

        public MergeManyEx(IObservable<IChangeSet<T>> source,
                           Func<T, IObservable<IChangeSet<TDestination>>> observableSelector)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _observableSelector = observableSelector ?? throw new ArgumentNullException(nameof(observableSelector));
        }

        private void ForwardWhenRemove(IObserver<IChangeSet<TDestination>> observer, T sourceItem)
        {
            var oblist = _observableSelector(sourceItem).AsObservableList();
            var changeset = new ChangeSet<TDestination>(new[]
            {
                new Change<TDestination>(ListChangeReason.RemoveRange, oblist.Items)
            });
            oblist.Dispose();
            observer.OnNext(changeset);
        }

        public IObservable<IChangeSet<TDestination>> Run()
        {
            return Observable.Create<IChangeSet<TDestination>>
                (
                    observer =>
                    {
                        var locker = new object();
                        return _source
                            .SubscribeMany(t => _observableSelector(t).Synchronize(locker).Subscribe(observer.OnNext))
                            .Subscribe(t =>
                            {
                                foreach (var x in t)
                                {
                                    switch (x.Reason)
                                    {
                                        case ListChangeReason.RemoveRange:
                                            {
                                                foreach (var item in x.Range) ForwardWhenRemove(observer, item);
                                                break;
                                            }
                                        case ListChangeReason.Remove:
                                            {
                                                ForwardWhenRemove(observer, x.Item.Current);
                                                break;
                                            }
                                    }
                                }
                            }, observer.OnError);
                    });
        }
    }
}
