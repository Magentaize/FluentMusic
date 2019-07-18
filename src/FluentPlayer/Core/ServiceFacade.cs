using System.Threading.Tasks;
using Magentaize.FluentPlayer.Core.Services;
using Magentaize.FluentPlayer.Data;
using Microsoft.EntityFrameworkCore;

namespace Magentaize.FluentPlayer.Core
{
    public static class ServiceFacade
    {
        public static async Task StartupAsync()
        {
            Db = new FluentPlayerDbContext();
            Db.Database.Migrate();

            IndexService = await new IndexService().InitializeAsync();
            CacheService = await CacheService.CreateAsync();
            PlaybackService = await PlaybackService.CreateAsync();
        }

        internal static FluentPlayerDbContext Db;

        public static IndexService IndexService;
        public static CacheService CacheService;
        public static PlaybackService PlaybackService;
    }
}