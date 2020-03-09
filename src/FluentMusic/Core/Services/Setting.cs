using System.Reactive.Subjects;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FluentMusic.Core.Services
{
    public class Setting
    {

        private static IDictionary<string, ReplaySubject<object>> _settingChanged;
        public static IReadOnlyDictionary<string, IObservable<object>> SettingChanged { get; private set; }

        private static ApplicationDataContainer _container;

        public static bool AddOrUpdate<T>(string key, T value)
        {
            bool valueChanged = false;
            var newValue = JsonSerializer.Serialize(value);
            
            if (_container.Values.ContainsKey(key))
            {
                var oldValue = (string)_container.Values[key];
                if (newValue != oldValue)
                {
                    valueChanged = true;
                }
            }
            else
            {
                valueChanged = true;
            }

            if (valueChanged)
            {
                _container.Values[key] = newValue;
                _settingChanged[key].OnNext(value);
            }

            return valueChanged;
        }

        public static T GetOrDefault<T>(string key, T defaultValue)
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

        public static T Get<T>(string key)
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

        public static bool Contains(string key)
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

            _settingChanged = initSettings.ToDictionary(x => x.k, _ => new ReplaySubject<object>(1));
            SettingChanged = new ReadOnlyDictionary<string, IObservable<object>>(_settingChanged.ToDictionary(x => x.Key, x => x.Value.AsObservable()));
            initSettings.ForEach(x => _settingChanged[x.k].OnNext(JsonSerializer.Deserialize((string)_container.Values[x.k], x.t)));

            await Task.CompletedTask;
        }

        private static (string k, Type t, object v)[] initSettings = new (string k, Type t, object v)[]
        {
            (Core.FirstRun, typeof(bool), false),
            (Collection.AutoRefresh, typeof(bool), true),
            (Behavior.AutoScroll, typeof(bool), true),
            (Behavior.RepeatMode, typeof(MediaRepeatMode), MediaRepeatMode.None),
            (Behavior.EnableShuffle, typeof(bool), false),
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
            public static string AutoRefresh = nameof(AutoRefresh);
            public static string Indexing = nameof(Indexing);
        }

        public static class Behavior
        {
            public static string AutoScroll = nameof(AutoScroll);
            public static string RepeatMode = nameof(RepeatMode);
            public static string EnableShuffle = nameof(EnableShuffle);
        }
    }
}
