using FluentMusic.Data;
using FluentMusic.ViewModels.Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace FluentMusic.Core.Extensions
{
    public static class ModelExtension
    {
        public static async Task<StorageFolder> GetStorageFolderAsync(this Folder folder)
        {
            return await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(folder.Token);
        }

        public static async Task<StorageFolder> GetStorageFolderAsync(this FolderViewModel folder)
        {
            return await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(folder.Token);
        }

        public static async Task<T> SingleByIdAsync<T>(this DbSet<T> source, long id) where T : class, IIdEntity
        {
            return await source.SingleAsync(x => x.Id == id);
        }
    }
}
