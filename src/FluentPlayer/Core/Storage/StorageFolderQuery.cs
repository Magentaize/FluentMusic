using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Search;

namespace Magentaize.FluentPlayer.Core.Storage
{
    public class StorageFolderQuery
    {
        public static IList<string> AudioExtensions = new[] { ".flac", ".m4a", ".aac", ".mp3", ".wma", ".ogg", ".oga" };
        public static IList<string> AlbumCoverExtensions = new[] {".jpg", ".jpeg", ".png", ".bmp"};

        private StorageFolderQuery() { }

        private StorageFolder _folder;
        public StorageFileQueryResult QueryResult { get; private set; }
        public IObservable<EventPattern<IStorageQueryResultBase, object>> ContentsChangedStream { get; private set; }

        public StorageFolderQuery(StorageFolder folder, IList<string> typeFilter)
        {
            _folder = folder;
            var options = new QueryOptions(CommonFileQuery.DefaultQuery, typeFilter)
            {
                FolderDepth = FolderDepth.Deep,
                //IndexerOption = IndexerOption.DoNotUseIndexer,
            };
            QueryResult = _folder.CreateFileQueryWithOptions(options);

            ContentsChangedStream = Observable.FromEventPattern<TypedEventHandler<IStorageQueryResultBase, object>, IStorageQueryResultBase, object>(
                h => QueryResult.ContentsChanged += h, h => QueryResult.ContentsChanged -= h);
        }

        public async Task<IReadOnlyList<StorageFile>> ExecuteQueryAsync()
        {
            var files = await QueryResult.GetFilesAsync();
            return files;
        }
    }
}