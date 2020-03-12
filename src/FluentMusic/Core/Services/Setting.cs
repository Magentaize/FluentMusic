using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;

namespace FluentMusic.Core.Services
{
    public class Setting
    {
        private static ApplicationDataContainer _container;

        private static void AddOrUpdate<T>(string key, T value)
        {
            var newValue = JsonSerializer.Serialize(value);
            _container.Values[key] = newValue;
        }

        private static T GetOrDefault<T>(string key, T defaultValue)
        {
            if (_container.Values.TryGetValue(key, out var value))
            {
                return JsonSerializer.Deserialize<T>((string)value);
            }
            else
            {
                AddOrUpdate(key, defaultValue);
                return defaultValue;
            }
        }

        private static T Get<T>(string key)
        {
            if (_container.Values.TryGetValue(key, out var value))
            {
                return JsonSerializer.Deserialize<T>((string)value);
            }
            else
            {
                throw new ArgumentException($"Request key [{key}] is invalid.");
            }
        }

        private static bool Contains(string key)
        {
            return _container.Values.ContainsKey(key);
        }

        public static void InitializeSetting<T>(ISubject<T> subject, string key, T defaultValue)
        {
            subject.OnNext(GetOrDefault(key, defaultValue));
            subject.Subscribe((T x) => AddOrUpdate(key, x));
        }

        private Setting() { }

        public static async Task InitializeAsync()
        {
            _container = ApplicationData.Current.LocalSettings;
            if (!Contains(Core.FirstRun))   Seed();

            LoadAll();
            await Task.CompletedTask;
        }

        private static void LoadAll()
        {
            Collection.AutoRefresh.OnNext(Get<bool>(nameof(Collection.AutoRefresh)));
            Collection.AutoRefresh
                .ObservableOnThreadPool()
                .Subscribe(x => AddOrUpdate(nameof(Collection.AutoRefresh), x));

            Behavior.AutoScroll.OnNext(Get<bool>(nameof(Behavior.AutoScroll)));
            Behavior.AutoScroll
                .ObservableOnThreadPool()
                .Subscribe(x => AddOrUpdate(nameof(Behavior.AutoScroll), x));
            Behavior.RepeatMode.OnNext(Get<MediaRepeatMode>(nameof(Behavior.RepeatMode)));
            Behavior.RepeatMode
                .ObservableOnThreadPool()
                .Subscribe(x => AddOrUpdate(nameof(Behavior.RepeatMode), x));
            Behavior.EnableShuffle.OnNext(Get<bool>(nameof(Behavior.EnableShuffle)));
            _ = Behavior.EnableShuffle
                .ObservableOnThreadPool()
                .Subscribe(x => AddOrUpdate(nameof(Behavior.EnableShuffle), x));
            Behavior.Volume.OnNext(Get<int>(nameof(Behavior.Volume)));
            Behavior.Volume
                .ObservableOnThreadPool()
                .Subscribe(x => AddOrUpdate(nameof(Behavior.Volume), x));
        }

        private static (string k, object v)[] initSettings = new (string k, object v)[]
        {
            (Core.FirstRun, false),
            (nameof(Collection.AutoRefresh), true),
            (nameof(Behavior.AutoScroll), true),
            (nameof(Behavior.RepeatMode), MediaRepeatMode.None),
            (nameof(Behavior.EnableShuffle), false),
            (nameof(Behavior.Volume), 100),
        };

        private static void Seed()
        {
            initSettings.ForEach(t => _container.Values[t.k] = JsonSerializer.Serialize(t.v));
        }

        public static class Core
        {
            public static string FirstRun = nameof(FirstRun);
        }

        public static class Collection
        {
            public static ISubject<bool> AutoRefresh { get; } = new ReplaySubject<bool>(1);
            public static string Indexing = nameof(Indexing);
        }

        public static class Behavior
        {
            public static ISubject<bool> AutoScroll { get; } = new ReplaySubject<bool>(1);
            public static ISubject<MediaRepeatMode> RepeatMode { get; } = new ReplaySubject<MediaRepeatMode>(1);
            public static ISubject<bool> EnableShuffle { get; } = new ReplaySubject<bool>(1);
            public static ISubject<int> Volume { get; } = new ReplaySubject<int>(1);
        }
    }
}
