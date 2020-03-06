using FluentMusic.Data;
using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace FluentMusic.Core.Extensions
{
    public static class ModelExtension
    {
        public static async Task<StorageFolder> GetStorageFolderAsync(this Folder folder)
        {
            return await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFolderAsync(folder.Token);
        }
    }
}
