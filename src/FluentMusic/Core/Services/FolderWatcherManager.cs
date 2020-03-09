using DynamicData;
using FluentMusic.Core.Extensions;
using FluentMusic.ViewModels.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Search;

namespace FluentMusic.Core.Services
{
    public sealed class FolderWatcherManager
    {
        private static ISubject<Unit> _contentChanged = new Subject<Unit>();
        public static IObservable<Unit> ContentChanged { get; } = _contentChanged.AsObservable();

        public static IObservableCache<FolderWatcher, long> Watchers { get; private set; }
        public static async Task InitializeAsync()
        {
            Watchers = IndexService.MusicFolders.Connect()
                .TransformAsync(x => FolderWatcher.CreateAsync(x))
                .DisposeMany()
                .AsObservableCache();
        }

        public sealed class FolderWatcher : IDisposable
        {
            private static IObservable<bool> autoRefresh = Setting.SettingChanged[Setting.Collection.AutoRefresh].Select(x => (bool)x);
            public StorageFileQueryResult Query { get; private set; }

            private IDisposable _subscription;
            public static async Task<FolderWatcher> CreateAsync(FolderViewModel vm)
            {
                var fw = new FolderWatcher();
                await fw.Create(vm);
                return fw;
            }

            private async Task Create(FolderViewModel vm)
            {
                var sf = await vm.GetStorageFolderAsync();
                var options = new QueryOptions
                {
                    FolderDepth = FolderDepth.Deep,
                    IndexerOption = IndexerOption.DoNotUseIndexer,
                    ApplicationSearchFilter = ComposeFilters(),
                };
                options.FileTypeFilter.AddRange(Statics.AudioFileTypes);

                Query = sf.CreateFileQueryWithOptions(options);
                // Event "ContentsChanged" only fires after GetFilesAsync has been called at least once.
                await Query.GetFilesAsync();

                _subscription = Observable.FromEventPattern<TypedEventHandler<IStorageQueryResultBase, object>, object>(
                    h => Query.ContentsChanged += h, h => Query.ContentsChanged -= h)
                .SkipUntil(autoRefresh.Where(x => x == true))
                .TakeUntil(autoRefresh.Where(x => x == false))
                .Repeat()
                .Select(_ => Unit.Default)
                .Subscribe(_contentChanged);
            }

            private static string ComposeFilters()
            {
                string q = string.Empty;
                //if (Settings.Current.FileSizeFilterEnabled)
                //{
                //    q += $" System.Size:>{Settings.Current.GetSystemSize()} ";
                //}
                return q;
            }

            public void Dispose()
            {
                _subscription.Dispose();
            }
        }
    }
}
