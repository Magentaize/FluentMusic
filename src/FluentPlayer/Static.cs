using Windows.Foundation.Collections;
using Windows.Storage;
using AutoMapper;
using Magentaize.FluentPlayer.Data;
using Magentaize.FluentPlayer.ViewModels;

namespace Magentaize.FluentPlayer
{
    public static class Static
    {
        static Static()
        {
            Mapper.Initialize(cfg => cfg.CreateMap<Album, AlbumViewModel>());
        }

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