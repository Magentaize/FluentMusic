using System.Collections.Generic;
using System.Linq;
using Windows.Storage;
using Aurora.Shared.Helpers;
using Magentaize.FluentPlayer.Core.Storage;

namespace Magentaize.FluentPlayer.Core.Services
{
    public class IndexService
    {
        public void BeginIndex()
        {
            var list = new List<StorageFolder>();
            list.Add(KnownFolders.MusicLibrary);

            var fs = list.SelectMany(f => AsyncHelper.RunSync(async()=> await StorageFolderQuery.Create(f).ExecuteQueryAsync())).ToList();
        }
    }
}