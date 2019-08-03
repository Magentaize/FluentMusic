using FluentMusic.Core.Services;
using FluentMusic.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace FluentMusic.Core
{
    public static class ServiceFacade
    {
        public static async Task StartupAsync()
        {
            Db = new FluentMusicDbContext();
            Db.Database.Migrate();

            Setting = new Setting();
            await Setting.InitializeAsync();
            IndexService = await new IndexService().InitializeAsync();
            CacheService = await CacheService.CreateAsync();
            PlaybackService = new PlaybackService();
            await PlaybackService.InitializeAsync();

        }

        internal static FluentMusicDbContext Db;

        internal static Setting Setting;
        public static IndexService IndexService;
        public static CacheService CacheService;
        public static PlaybackService PlaybackService;
    }
}