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

        public static bool AddOrUpdateBinary(string key, object value)
        {
            return AddOrUpdate(key, JsonSerializer.Serialize(value));
        }

        public static bool AddOrUpdate(string key, object value)
        {
            bool valueChanged = false;

            if (_container.Values.ContainsKey(key))
            {
                if (_container.Values[key] != value)
                {
                    _container.Values[key] = value;
                    valueChanged = true;
                }
            }
            else
            {
                _container.Values.Add(key, value);
                valueChanged = true;
            }

            if (valueChanged) _settingChanged[key].OnNext(value);

            return valueChanged;
        }

        public static T GetOrDefaultBinary<T>(string key, T defaultValue)
        {
            string val;
            if (_container.Values.TryGetValue(key, out var value))
            {
                val = (string)value;
                return JsonSerializer.Deserialize<T>(val);
            }
            else
            {
                val = JsonSerializer.Serialize(defaultValue);
                AddOrUpdateBinary(key, val);
                return defaultValue;
            }
        }

        public static T GetOrDefault<T>(string key, T defaultValue)
        {
            if (_container.Values.TryGetValue(key, out var value))
            {
                return (T)value;
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
                return (T)value;
            }
            else
            {
                throw new ArgumentException("Request key is invalid.");
            }
        }

        public static bool Contains(string key)
        {
            return _container.Values.ContainsKey(key);
        }

        public static void InitializeSettingBinary<T>(ISubject<T> subject, string key, T defaultValue)
        {
            subject.OnNext(GetOrDefaultBinary(key, defaultValue));
            subject.Subscribe((T x) => AddOrUpdateBinary(key, x));
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

            if (!Contains(Core.FirstRun))
            {
                Seed();
            }

            _settingChanged = initSettings.ToDictionary(x => x.k, _ => new ReplaySubject<object>(1));
            SettingChanged = new ReadOnlyDictionary<string, IObservable<object>>(_settingChanged.ToDictionary(x => x.Key, x => x.Value.AsObservable()));

            await Task.CompletedTask;
        }

        private static (string k, object v)[] initSettings = new (string k, object v)[]
        {
            (Core.FirstRun, false),
            (Collection.AutoRefresh, true),
            (Behavior.AutoScroll, true),
        };

        private static void Seed()
        {
            initSettings.ForEach(t => AddOrUpdate(t.k, t.v));
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
        }
    }
}
