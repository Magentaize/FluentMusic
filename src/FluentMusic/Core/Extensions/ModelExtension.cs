using FluentMusic.Data;
using FluentMusic.ViewModels.Common;
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
    }
}
