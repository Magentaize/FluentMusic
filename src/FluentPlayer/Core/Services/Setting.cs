using System.Reactive.Subjects;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;
using System;

namespace Magentaize.FluentPlayer.Core.Services
{
    public class Setting
    {
        private ApplicationDataContainer _container;

        public bool AddOrUpdateBinary(string key, object value)
        {
            return AddOrUpdate(key, JsonSerializer.Serialize(value));
        }

        public bool AddOrUpdate(string key, object value)
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

        public T GetOrDefaultBinary<T>(string key, T defaultValue)
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

        public T GetOrDefault<T>(string key, T defaultValue)
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

        public void InitializeSettingBinary<T>(ISubject<T> subject, string key, T defaultValue)
        {
            subject.OnNext(GetOrDefaultBinary<T>(key, defaultValue));
            subject.Subscribe((T x) => AddOrUpdateBinary(key, x));
        }

        public void InitializeSetting<T>(ISubject<T> subject, string key, T defaultValue)
        {
            subject.OnNext(GetOrDefault<T>(key, defaultValue));
            subject.Subscribe((T x) => AddOrUpdate(key, x));
        }

        internal Setting() { }

        public async Task InitializeAsync()
        {
            _container = ApplicationData.Current.LocalSettings;
            await Task.CompletedTask;
        }
    }
}
