using akarnokd.reactive_extensions;
using DynamicData;
using FluentMusic.Core.Extensions;
using FluentMusic.Data;
using FluentMusic.ViewModels.Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TagLib;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.Storage.Search;
using Windows.Storage.Streams;
using Z.Linq;

namespace FluentMusic.Core.Services
{
    public class IndexService
    {
        public static IObservableCache<ArtistViewModel, long> ArtistSource => _artistSource.AsObservableCache();
        public static IObservableCache<TrackViewModel, long> TrackSource { get; private set; }
        public static IObservableCache<AlbumViewModel, long> AlbumSource { get; private set; }
        public static IObservableCache<FolderViewModel, long> MusicFolders => _musicFolders.AsObservableCache();

        private readonly static ISourceCache<ArtistViewModel, long> _artistSource = new SourceCache<ArtistViewModel, long>(x => x.Id);
        private readonly static ISourceCache<FolderViewModel, long> _musicFolders = new SourceCache<FolderViewModel, long>(x => x.Id);

        public event EventHandler IndexBegin;
        public event EventHandler IndexProgressChanged;
        public event EventHandler IndexFinished;

        private static readonly IList<string> AlbumCoverFileNames = new List<string> { ".jpg", ".jpeg", ".png", ".bmp" }.Select(x => $"cover{x}").ToList();

        public int QueueIndexingCount { get; private set; }
        public int QueueIndexedCount { get; private set; }

        private IndexService()
        {
        }

        public static async Task RunAsync()
        {
            await IndexAutomatically();
        }

        public static async Task IndexAutomatically()
        {
            if (!Setting.Get<bool>(Setting.Collection.AutoRefresh))
                return;

            await BeginIndexAsync();
        }

        private static async Task<(IEnumerable<Folder> removedFolders, IEnumerable<StorageFolderMixin> changedFolders)> GetChangedLibraryFolders()
        {
            var r = new List<Folder>();
            var c = new List<StorageFolderMixin>();
            var db = new Db();
            var dbFolders = await db.Set<Folder>().AsQueryable().ToListAsync();
            await db.DisposeAsync();

            foreach (var f in dbFolders)
            {
                try
                {
                    var sf = await f.GetStorageFolderAsync();
                    var prop = await sf.GetBasicPropertiesAsync();
                    if (f.NeedIndex || prop.DateModified != f.DateModified)
                    {
                        c.Add(new StorageFolderMixin { Folder = f, StorageFolder = sf, Properties = prop });
                    }
                }
                catch (FileNotFoundException)
                {
                    r.Add(f);
                }
            }

            return (r, c);
        }

        private static async Task<IList<StorageFile>> GetFilesAsync(StorageFolder folder)
        {
            // TODO: determine is ondrive on demand
            var files = new List<StorageFile>();
            files.AddRange(await new FileTracker(folder).SearchFolder());
            return files;
        }

        private static async Task IndexChangedFolders(IEnumerable<StorageFolderMixin> folders)
        {
            folders = await Task.WhenAll(folders.Select(async x => { x.Files = await GetFilesAsync(x.StorageFolder); return x; }));
            var group = await folders.GroupJoinAsync(TrackSource.Items, x => x.Folder.Id, x => x.FolderId, (mix, dbFiles) => (mix, dbFiles.ToList()));

            await group.ForEachAsync(async g =>
            {
                var dbFiles = g.Item2;
                var diskFiles = g.mix.Files;

                await diskFiles.ForEachAsync(async f =>
                {
                    var trackRx = await dbFiles.FirstOrDefaultAsync(x => x.FileName == f.Name && x.Path == f.Path);

                    if (trackRx == null)
                    {
                        var db = new Db();
                        var tf = TagLib.File.Create(await UwpFileAbstraction.CreateAsync(f));
                        var tag = tf.Tag;

                        var track = await CreateTrackAsync(f, tf);
                        await db.Tracks.AddAsync(track);

                        // create artist entity
                        var artistName = string.IsNullOrEmpty(tag.FirstPerformer) ? "Unknown" : tag.FirstPerformer;
                        var artist = await db.Artists
                            .Include(x => x.Albums)
                            .FirstOrDefaultAsync(a => a.Name == artistName);

                        var createArtist = artist == default;

                        if (createArtist)
                        {
                            artist = new Artist()
                            {
                                Name = artistName,
                            };

                            await db.Artists.AddAsync(artist);
                        }

                        // create album entity
                        var albumTitle = string.IsNullOrEmpty(tag.Album) ? "Unknown" : tag.Album;
                        var album = artist.Albums.FirstOrDefault(x => x.Title == albumTitle);

                        var createAlbum = album == default;

                        if (createAlbum)
                        {
                            album = new Album()
                            {
                                Title = albumTitle,
                            };

                            await db.Albums.AddAsync(album);
                        }

                        if (string.IsNullOrEmpty(album.CoverCacheToken))
                        {
                            var picData = await FindAlbumCoverAsync(tag, f.Path);

                            if (picData != default(IBuffer))
                            {
                                var cover = await CacheService.CacheAsync(picData);
                                album.CoverCacheToken = cover;
                            }
                        }

                        track.Album = album;
                        track.Folder = g.mix.Folder;
                        album.Tracks.Add(track);
                        if (createAlbum)
                        {
                            album.Artist = artist;
                            artist.Albums.Add(album);
                        }

                        await db.SaveChangesAsync();

                        if (createArtist)
                        {
                            var artistVm = ArtistViewModel.Create(artist);
                            _artistSource.AddOrUpdate(artistVm);
                        }
                        else
                        {
                            var artistVm = _artistSource.Lookup(artist.Id).Value;
                            if (createAlbum)
                            {
                                var albumVm = AlbumViewModel.Create(artistVm, album);
                                artistVm.Albums.AddOrUpdate(albumVm);
                            }
                            else
                            {
                                var albumVm = artistVm.Albums.Items.Single(x => x.Id == album.Id);
                                var trackVm = TrackViewModel.Create(albumVm, track);
                                albumVm.Tracks.AddOrUpdate(trackVm);
                            }
                        }
                    }
                    else
                    {
                        dbFiles.Remove(dbFiles.First(x => x.Id == trackRx.Id));
                    }
                });

                await dbFiles.ForEachAsync(async f =>
                {
                    await RemoveTrackAsync(f.Id);
                });
            });
        }

        private static async Task RemoveTrackAsync(long id)
        {
            Track track = default;
            Album album = default;
            Artist artist = default;
            var albumDeleted = false;
            var artistDeleted = false;
            var trackDeleted = false;

            async Task RemoveTrackDbAsync()
            {
                using (var db = Db.Instance)
                {
                    track = await db.Tracks.SingleAsync(x => x.Id == id);

                    using (var tr = await db.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            album = track.Album;
                            db.Tracks.Remove(track);
                            await db.SaveChangesAsync();

                            albumDeleted = album.Tracks.Count == 0;
                            if (albumDeleted)
                            {
                                artist = album.Artist;
                                db.Albums.Remove(album);
                                await db.SaveChangesAsync();

                                artistDeleted = artist.Albums.Count == 0;
                                if (artistDeleted)
                                {
                                    db.Artists.Remove(artist);
                                    await db.SaveChangesAsync();
                                }
                            }

                            await tr.CommitAsync();
                            trackDeleted = true;
                        }
                        catch (Exception)
                        {
                            await tr.RollbackAsync();
                            trackDeleted = false;
                        }
                    }
                }
            }

            async Task RemoveTrackVmAsync()
            {
                var artistVm = await ArtistSource.Items.SingleAsync(x => x.Id == artist.Id);
                var albumVm = await artistVm.Albums.Items.SingleAsync(x => x.Id == album.Id);
                //var trackVm = await albumVm.Tracks.Items.SingleAsync(x => x.Id == track.Id);
                albumVm.Tracks.RemoveKey(track.Id);
                if (albumDeleted)
                {
                    artistVm.Albums.RemoveKey(album.Id);
                    await CacheService.DeleteCacheAsync(album.CoverCacheToken);

                    if (artistDeleted)
                    {
                        _artistSource.RemoveKey(artist.Id);
                    }
                }
            }

            await RemoveTrackDbAsync();
            if (trackDeleted)
                await RemoveTrackVmAsync();
        }

        private static async Task IndexRemovedFolders(IEnumerable<Folder> folders)
        {
            var group = await folders.GroupJoinAsync(TrackSource.Items, x => x.Id, x => x.FolderId, (x, y) => (folder: x, tracks: y));

            await group.ForEachAsync(async g =>
            {
                await g.tracks.ForEachAsync(async x => await RemoveTrackAsync(x.Id));
                Db.Instance.Folders.Remove(g.folder);

                await Db.Instance.SaveChangesAsync();
            });
        }

        public static async Task BeginIndexAsync()
        {
            var (r, c) = await GetChangedLibraryFolders();
            await IndexChangedFolders(c);
            await IndexRemovedFolders(r);
        }

        internal static async Task InitializeAsync()
        {
            //Observable.Using(() => Db.Instance, db => db.Set<Folder>().ToObservable())
            //    .SubscribeOnThreadPool()
            //    .Select(FolderViewModel.Create)
            //    .Subscribe(_musicFolders.AddOrUpdate);

            //Observable.Using(() => Db.Instance, db => db.Set<Artist>().ToObservable())
            //    .SubscribeOnThreadPool()
            //    .Select(ArtistViewModel.Create)
            //    .Subscribe(_artistSource.AddOrUpdate);

            Observable.Using(
                () => Db.Instance,
                db => Observable.Merge(new IObservable<object>[]
                {
                    db.Set<Folder>().ToList()
                        .ToObservable()
                        .SubscribeOnThreadPool()
                        .Select(x => FolderViewModel.Create(x))
                        .Do(_musicFolders.AddOrUpdate),
                    db.Set<Artist>().ToList()
                        .ToObservable()
                        .SubscribeOnThreadPool()
                        .Select(x => ArtistViewModel.Create(x))
                        .Do(_artistSource.AddOrUpdate)
                }))
                .Subscribe();

            AlbumSource = ArtistSource.Connect()
            .SubscribeOnThreadPool()
            .MergeMany(x => x.Albums.Connect())
            .AsObservableCache();

            TrackSource = AlbumSource.Connect()
                .SubscribeOnThreadPool()
                .MergeMany(x => x.Tracks.Connect())
                .AsObservableCache();
        }

        private static async Task<IBuffer> FindAlbumCoverAsync(Tag tag, string path)
        {
            var picData = default(IBuffer);
            var folder = Path.GetDirectoryName(path);

            IStorageFile folderCover = null;
            foreach (var f in AlbumCoverFileNames)
            {
                var fn = Path.Combine(folder, f);

                try
                {
                    folderCover = await StorageFile.GetFileFromPathAsync(fn);
                    break;
                }
                catch { }
            }

            if (folderCover != default(IStorageFile))
            {
                picData = await FileIO.ReadBufferAsync(folderCover);
            }
            else if (tag.Pictures?.Length >= 1)
            {
                picData = tag.Pictures[0].Data.Data.AsBuffer();
            }

            return picData;
        }

        private static async Task<Track> CreateTrackAsync(IStorageFile file, TagLib.File tFile)
        {
            var tag = tFile.Tag;
            var prop = tFile.Properties;
            var fbp = await file.GetBasicPropertiesAsync();

            var track = new Track()
            {
                Path = file.Path,
                FileName = file.Name,
                IndexingSuccess = 0,
                DateAdded = DateTime.Now.Ticks,
                DateModified = fbp.DateModified,
                Title = tag.Title,
                Year = tag.Year,
                Duration = prop.Duration,
                Genres = tag.FirstGenre ?? "Unknown",
                FileSize = fbp.Size
            };

            return track;
        }

        public static async Task RequestRemoveFolderAsync(FolderViewModel vm) { }

        public static async Task RequestAddFolderAsync()
        {
            var folderPicker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.ComputerFolder
            };
            folderPicker.FileTypeFilter.Add("*");

            var folder = await folderPicker.PickSingleFolderAsync();
            if (folder == null)
                return;
            else
            {
                // If this new folder is a subfolder, it shouldn't be added to db.
                var isSubFolder = await _musicFolders.Items.AnyAsync(x => folder.Path.StartsWith(x.Path));
                if (isSubFolder)
                {
                    //TODO:Message
                    return;
                }

                // If this new folder is a parent folder, it shouldn't be added to db.
                var isParentFolder = await _musicFolders.Items.AnyAsync(x => x.Path.StartsWith(folder.Path));
                if (isParentFolder)
                {
                    //TODO:Message
                    return;
                }

                var props = await folder.GetBasicPropertiesAsync();
                var f = new Folder()
                {
                    DateModified = props.DateModified,
                    Path = folder.Path,
                    NeedIndex = true,
                    Token = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(folder)
                };

                using (var db = Db.Instance)
                {
                    await db.AddAsync(f);
                    await db.SaveChangesAsync();
                }

                _musicFolders.AddOrUpdate(FolderViewModel.Create(f));

                Setting.AddOrUpdate(Setting.Collection.Indexing, true);
            }
        }

    private class StorageFolderMixin
    {
        public Folder Folder { get; set; }
        public StorageFolder StorageFolder { get; set; }
        public BasicProperties Properties { get; set; }

        public IList<StorageFile> Files { get; set; }
    }
}

    public class FileTracker
    {
        public static event TypedEventHandler<IStorageQueryResultBase, object> FilesChanged;

        public FileTracker(StorageFolder f)
        {
            Folder = f;
            var options = new QueryOptions
            {
                FolderDepth = FolderDepth.Deep,
                IndexerOption = IndexerOption.DoNotUseIndexer,
                ApplicationSearchFilter = ComposeFilters(),
            };
            options.FileTypeFilter.AddRange(Statics.AudioFileTypes);

            Query = Folder.CreateFileQueryWithOptions(options);
            Query.ContentsChanged += Query_ContentsChanged;
        }

        private void Query_ContentsChanged(IStorageQueryResultBase sender, object args)
        {
            FilesChanged?.Invoke(sender, EventArgs.Empty);
        }

        public StorageFolder Folder { get; }
        public StorageFileQueryResult Query { get; }

        private string ComposeFilters()
        {
            string q = string.Empty;
            //if (Settings.Current.FileSizeFilterEnabled)
            //{
            //    q += $" System.Size:>{Settings.Current.GetSystemSize()} ";
            //}
            return q;
        }

        public async Task<IReadOnlyList<StorageFile>> SearchFolder()
        {
            Query.ContentsChanged -= Query_ContentsChanged;
            var files = await Query.GetFilesAsync();
            Query.ContentsChanged += Query_ContentsChanged;
            return files;
        }
    }

        public sealed class UwpFileAbstraction : TagLib.File.IFileAbstraction
    {
        private readonly IStorageFile _file;

        private UwpFileAbstraction() { }

        public static async Task<TagLib.File.IFileAbstraction> CreateAsync(IStorageFile file)
        {
            var fAbs = new UwpFileAbstraction();
            fAbs.Name = file.Path;
            var ras = await file.OpenAsync(FileAccessMode.Read);
            fAbs.ReadStream = ras.AsStream();

            return fAbs;
        }

        public string Name { get; private set; }

        public Stream ReadStream { get; private set; }

        public Stream WriteStream { get; }

        public void CloseStream(Stream stream)
        {
            stream.Close();
        }
    }
}