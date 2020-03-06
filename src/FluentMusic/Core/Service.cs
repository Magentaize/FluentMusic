using FluentMusic.Core.Services;
using FluentMusic.Data;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using System;

namespace FluentMusic.Core
{
    public static class Service
    {
        public static async Task StartupAsync()
        {
            //await ApplicationData.Current.LocalFolder.CreateFileAsync("FluentMusic.db", CreationCollisionOption.OpenIfExists);
            var dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "FluentMusic.db");
            await Db.InitializeAsync(dbpath);

            await Setting.InitializeAsync();
            IndexService = await new IndexService().InitializeAsync();
            await CacheService.InitializeAsync();
            PlaybackService = new PlaybackService();
            await PlaybackService.InitializeAsync();

        }

        public static IndexService IndexService;
        public static PlaybackService PlaybackService;
    }
}