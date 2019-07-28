using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Windows.ApplicationModel.Core;

namespace System.Reactive
{
    public static class ReactiveExtension
    {
        internal static class Stubs<T>
        {
            public static readonly Action<T> Ignore = _ => { };
            public static readonly Func<T, T> I = _ => _;
        }

        public static IConnectableObservable<T> CacheReplay<T>(this IObservable<T> source, int buffer)
        {
            return source.Multicast(new ReplaySubject<T>(buffer));
        }

        public static IScheduler CoreDispatcherScheduler = new CoreDispatcherScheduler(CoreApplication.GetCurrentView().Dispatcher);

        public static IObservable<T> ObservableOnCoreDispatcher<T>(this IObservable<T> source)
        {
            return source.ObserveOn(CoreDispatcherScheduler);
        }

        public static IObservable<T> ObservableOnThreadPool<T>(this IObservable<T> source)
        {
            return source.ObserveOn(ThreadPoolScheduler.Instance);
        }

        public static IObservable<T> SubscribeOnThreadPool<T>(this IObservable<T> source)
        {
            return source.SubscribeOn(ThreadPoolScheduler.Instance);
        }
    }
}
