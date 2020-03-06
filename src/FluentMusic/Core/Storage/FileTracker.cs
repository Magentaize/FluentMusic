using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Search;
using Windows.Storage;
using Windows.Foundation;

namespace FluentMusic.Core.Storage
{
    sealed class FileTracker
    {
        public static event TypedEventHandler<IStorageQueryResultBase, object> FilesChanged;

        public StorageFolder Folder { get; }
        public StorageFileQueryResult Query { get; }

        public FileTracker(StorageFolder f)
        {
            Folder = f;
            var options = new QueryOptions
            {
                FolderDepth = FolderDepth.Deep,
                IndexerOption = IndexerOption.DoNotUseIndexer,
                ApplicationSearchFilter = ComposeFilters(),
            };
            foreach (var ext in Consts.FileTypes)
{
                options.FileTypeFilter.Add(ext);
}
            Query = Folder.CreateFileQueryWithOptions(options);
            Query.ContentsChanged += Query_ContentsChanged;
        }

        private string ComposeFilters()
        {
            string q = string.Empty;

            if (Settings.Current.FileSizeFilterEnabled)
            {
                q += $" System.Size:>{Settings.Current.GetSystemSize()} ";
            }
            return q;
        }

        private void Query_ContentsChanged(IStorageQueryResultBase sender, object args)
        {
            FilesChanged?.Invoke(sender, EventArgs.Empty);
        }

        public async Task<IReadOnlyList<StorageFile>> SearchFolder()
        {
            Query.ContentsChanged -= Query_ContentsChanged;
            var files = await Query.GetFilesAsync();
            Query.ContentsChanged += Query_ContentsChanged;
            return files;
        }
    }
}
