using System.Reactive.Subjects;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;
using System;

namespace FluentMusic.Core.Services
{
    public class Setting
    {
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

        public static void InitializeSettingBinary<T>(ISubject<T> subject, string key, T defaultValue)
        {
            subject.OnNext(GetOrDefaultBinary<T>(key, defaultValue));
            subject.Subscribe((T x) => AddOrUpdateBinary(key, x));
        }

        public static void InitializeSetting<T>(ISubject<T> subject, string key, T defaultValue)
        {
            subject.OnNext(GetOrDefault<T>(key, defaultValue));
            subject.Subscribe((T x) => AddOrUpdate(key, x));
        }

        internal Setting() { }

        public static async Task InitializeAsync()
        {
            _container = ApplicationData.Current.LocalSettings;
            await Task.CompletedTask;
        }

        public static string FirstRun = nameof(FirstRun);

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
