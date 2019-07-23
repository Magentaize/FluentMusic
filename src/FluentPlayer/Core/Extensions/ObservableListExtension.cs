using System;

namespace DynamicData
{
    public static class ObservableListExtension
    {
        public static ISourceList<T> ToSourceList<T>(this IObservable<IChangeSet<T>> source)
        {
            var ret = new SourceList<T>();
            source.Subscribe(ret.Edit);

            return ret;
        }
    }
}
