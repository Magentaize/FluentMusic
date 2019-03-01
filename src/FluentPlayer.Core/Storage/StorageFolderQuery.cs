using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Search;

namespace Magentaize.FluentPlayer.Core.Storage
{
    public class StorageFolderQuery
    {
        private StorageFolderQuery() { }

        private StorageFolder _folder;
        private StorageFileQueryResult _queryResult;

        public static StorageFolderQuery Create(StorageFolder folder)
        {
            var sfq = new StorageFolderQuery {_folder = folder};
            var options = new QueryOptions
            {
                FileTypeFilter = { ".flac", ".wav", ".m4a", ".aac", ".mp3", ".wma", ".ogg", ".oga" },
                FolderDepth = FolderDepth.Deep,
                IndexerOption = IndexerOption.DoNotUseIndexer,
            };
            sfq._queryResult = sfq._folder.CreateFileQueryWithOptions(options);
            sfq._queryResult.ContentsChanged += _queryResult_ContentsChanged;

            return sfq;
        }

        private static void _queryResult_ContentsChanged(IStorageQueryResultBase sender, object args)
        {

        }

        public async Task<IReadOnlyList<StorageFile>> ExecuteQueryAsync()
        {
            _queryResult.ContentsChanged -= _queryResult_ContentsChanged;
            var files = await _queryResult.GetFilesAsync();
            _queryResult.ContentsChanged += _queryResult_ContentsChanged;
            return files;
        }
    }
}