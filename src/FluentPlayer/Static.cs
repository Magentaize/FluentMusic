using Windows.Foundation.Collections;
using Windows.Storage;
using DryIoc;

namespace Magentaize.FluentPlayer
{
    public static class Static
    {
        public static Container Container;

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