﻿using Magentaize.FluentPlayer.Core.Services;
using Magentaize.FluentPlayer.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Magentaize.FluentPlayer.Core
{
    public static class ServiceFacade
    {
        public static async Task StartupAsync()
        {
            Db = new FluentPlayerDbContext();
            Db.Database.Migrate();

            Setting = new Setting();
            await Setting.InitializeAsync();
            IndexService = await new IndexService().InitializeAsync();
            CacheService = await CacheService.CreateAsync();
            PlaybackService = new PlaybackService();
            await PlaybackService.InitializeAsync();

        }

        internal static FluentPlayerDbContext Db;

        internal static Setting Setting;
        public static IndexService IndexService;
        public static CacheService CacheService;
        public static PlaybackService PlaybackService;
    }
}