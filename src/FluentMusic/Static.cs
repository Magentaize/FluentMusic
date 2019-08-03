using Windows.Foundation.Collections;
using Windows.Storage;

namespace FluentMusic
{
    public static class Static
    {
        public static IPropertySet LocalSettings = ApplicationData.Current.LocalSettings.Values;

        public static class Settings
        {
            public static string FirstRun = nameof(FirstRun);

            public static class Collection
            {
                public static string AutoRefresh = nameof(AutoRefresh);
            }

            public static class Behavior
            {
                public static string AutoScroll = nameof(AutoScroll);
            }
        }
    }
}