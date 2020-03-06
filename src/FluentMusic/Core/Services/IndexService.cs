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

        private StorageLibrary _musicLibrary;
        private IObservable<EventPattern<IStorageQueryResultBase, object>> _libForlderContentChangedStream;
        internal IndexService()
        {
        }

        private async Task<(IEnumerable<Folder> removedFolders, IEnumerable<StorageFolderMixin> changedFolders)> GetChangedLibraryFolders()
        {
            var r = new List<Folder>();
            var c = new List<StorageFolderMixin>();
            var db = new Db();
            var dbFolders = await db.Set<Folder>().AsQueryable().ToListAsync();
            await db.DisposeAsync();

            foreach(var f in dbFolders)
            {
                try
                {
                    var sf = await f.GetStorageFolderAsync();
                    var prop = await sf.GetBasicPropertiesAsync();
                    if(f.NeedIndex || prop.DateModified != f.DateModified)
                    {
                        c.Add(new StorageFolderMixin { Folder = f, StorageFolder = sf, Properties = prop });
                    }
                }
                catch(FileNotFoundException)
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

        private async Task IndexChangedFolders(IEnumerable<StorageFolderMixin> folders)
        {
            folders = await Task.WhenAll(folders.Select(async x => { x.Files = await GetFilesAsync(x.StorageFolder); return x; }));
            var group = await folders.GroupJoinAsync(TrackSource.Items, x => x.Folder.Id, x => x.FolderId, (mix, dbFiles) => (mix, dbFiles.ToList()));

            await group.ForEachAsync(async g =>
            {
                var dbFiles = g.Item2;
                var diskFiles = g.mix.Files;

                await diskFiles.ForEachAsync(async f =>
                {
                    var trackVm = await dbFiles.FirstOrDefaultAsync(x => x.FileName == f.Name && x.Path == f.Path);

                    if (trackVm == null)
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

                        if (string.IsNullOrEmpty(album.Cover))
                        {
                            var picData = await FindAlbumCoverAsync(tag, f.Path);

                            if (picData != default(IBuffer))
                            {
                                var cover = await CacheService.CacheAsync(picData);
                                album.Cover = cover;
                            }
                        }

                        track.Folder = g.mix.Folder;
                        album.Tracks.Add(track);
                        if (createAlbum)
                        {
                            album.Artist = artist;
                            artist.Albums.Add(album);
                        }

                        await db.SaveChangesAsync();

                        trackVm = TrackViewModel.Create(track);
                        var artistVm = createArtist ? ArtistViewModel.Create(artist) : _artistSource.Lookup(artist.Id).Value;
                        var albumVm = createAlbum? AlbumViewModel.Create(album) : artistVm.Albums.Items.First(x => x.Id == album.Id);
                        trackVm.Album = albumVm;
                        albumVm.AddTrack(trackVm);
                        if (createAlbum)
                        {
                            albumVm.Artist = artistVm;
                            artistVm.AddAlbum(albumVm);
                        }
                        if (createArtist)
                        {
                            _artistSource.AddOrUpdate(artistVm);
                        }
                    }
                    else
                    {
                        dbFiles.Remove(dbFiles.First(x => x.Id == trackVm.Id));
                    }
                });

                await dbFiles.ForEachAsync(async f =>
                {
                    await RemoveTrackAsync(f.Id);
                });
            });          
        }

        private async Task RemoveTrackAsync(long id)
        {

        }

        private async Task IndexRemovedFolders(IEnumerable<Folder> folders)
        {
            var group = await folders.GroupJoinAsync(TrackSource.Items, x => x.Id, x => x.FolderId, (x, y) => (folder:x, tracks:y));

            await group.ForEachAsync(async g =>
            {
                await g.tracks.ForEachAsync(async x => await RemoveTrackAsync(x.Id));
                Db.Instance.Folders.Remove(g.folder);

                await Db.Instance.SaveChangesAsync();
            });
        }

        public async Task BeginIndexAsync()
        {
            var (r, c) = await GetChangedLibraryFolders();
            await IndexChangedFolders(c);
            await IndexRemovedFolders(r);
        }

        internal async Task<IndexService> InitializeAsync()
        {
            var db = new Db();

            db.Set<Folder>()
                .ToList()
                .ToObservable()
                .SubscribeOnThreadPool()
                .Select(x => FolderViewModel.Create(x))
                .Subscribe(x => _musicFolders.AddOrUpdate(x));

            db.Artists
                .Include(a => a.Albums)
                .ThenInclude(a => a.Tracks)
                .ToList()
                .ToObservable()
                .SubscribeOnThreadPool()
                .Select(x => ArtistViewModel.Create(x))
                .Subscribe(x =>
                {
                    _artistSource.AddOrUpdate(x);
                });

            AlbumSource = ArtistSource.Connect()
                .SubscribeOnThreadPool()
                .MergeMany(x => x.Albums.Connect())
                .AsObservableCache();

            TrackSource = AlbumSource.Connect()
                .SubscribeOnThreadPool()
                .MergeMany(x => x.Tracks.Connect())
                .AsObservableCache();

            await db.DisposeAsync();

            return this;
        }

        private async Task<IBuffer> FindAlbumCoverAsync(Tag tag, string path)
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


        private async Task<Track> CreateTrackAsync(IStorageFile file, TagLib.File tFile)
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
                Title = tag.Title,
                Year = tag.Year,
                Duration = prop.Duration,
                Genres = tag.FirstGenre ?? "Unknown",
                FileSize = fbp.Size
            };

            return track;
        }

        public static async Task RequestRemoveFolderAsync(FolderViewModel vm)
        {

        }

        public async Task RequestAddFolderAsync()
        {
            var folderPicker = new Windows.Storage.Pickers.FolderPicker
            {
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder
            };
            folderPicker.FileTypeFilter.Add("*");

            var folder = await folderPicker.PickSingleFolderAsync();
            if (folder == null)
                return;
            else
            {
                // If this new folder is a subfolder, it shouldn't be added to db.
                var parentFolderIsContained = await _musicFolders.Items.AnyAsync(x => folder.Path.StartsWith(x.Path));
                if (parentFolderIsContained)
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
                var db = new Db();
                db.Add(f);
                await db.SaveChangesAsync();

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