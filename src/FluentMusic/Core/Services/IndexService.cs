using akarnokd.reactive_extensions;
using DynamicData;
using FluentMusic.Core.Extensions;
using FluentMusic.Data;
using FluentMusic.ViewModels.Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TagLib;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Z.Linq;

namespace FluentMusic.Core.Services
{
    public sealed partial class IndexService
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

        private Db db;

        private IndexService()
        {
            db = new Db();
        }

        public async Task BeginIndexAsync()
        {
            var (r, c) = await GetChangedLibraryFolders();
            var mix = await GetFolderFilesAsync(c);
            await IndexFolders(mix);
            await RemovedFolders(r);
            await db.SaveChangesAsync();
        }

        public static async Task RunAsync()
        {
            var o = new IndexService();
            //await o.RunAsyncInner();
        }

        private async Task RunAsyncInner()
        {
            var (r, c) = await GetChangedLibraryFolders();
            var mix = await GetFolderFilesAsync(c);
            if (Setting.Get<bool>(Setting.Collection.AutoRefresh))
            {
                await IndexFolders(mix);
                await RemovedFolders(r);
                await db.SaveChangesAsync();
            }
        }

        private async Task<(IEnumerable<FolderViewModel> removedFolders, IEnumerable<FolderViewModel> changedFolders)> GetChangedLibraryFolders()
        {
            var r = new List<FolderViewModel>();
            var c = new List<FolderViewModel>();
            var folders = _musicFolders.Items;

            foreach (var f in folders)
            {
                try
                {
                    var sf = await f.GetStorageFolderAsync();
                    c.Add(f);
                }
                catch (FileNotFoundException)
                {
                    r.Add(f);
                }
            }

            return (r, c);
        }

        private static async Task<IEnumerable<GetFolderFilesResult>> GetFolderFilesAsync(IEnumerable<FolderViewModel> folders)
        {
            var t = folders.Select(async x =>
            {
                var files = await FolderWatcherManager.Watchers.Lookup(x.Id).Value.Query.GetFilesAsync();
                return new GetFolderFilesResult() { ViewModel = x, Files = files };
            });
            return (await Task.WhenAll(t)).ToList();
        }

        private async Task IndexFolders(IEnumerable<GetFolderFilesResult> folders)
        {
            async Task<IndexTrackTransactionResult> IndexTrackTransactionAsync(StorageFile f, Folder folder)
            {
                var tf = TagLib.File.Create(await UwpFileAbstraction.CreateAsync(f));
                var tag = tf.Tag;

                using (var tr = await db.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var track = await CreateTrackAsync(f, tf);
                        await db.Tracks.AddAsync(track);
                        await db.SaveChangesAsync();

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

                        await db.SaveChangesAsync();

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

                        await db.SaveChangesAsync();

                        track.Album = album;
                        track.Folder = folder;
                        album.Tracks.Add(track);
                        if (createAlbum)
                        {
                            album.Artist = artist;
                            artist.Albums.Add(album);
                        }

                        await db.SaveChangesAsync();

                        await tr.CommitAsync();
                        return new IndexTrackTransactionResult()
                        {
                            Successful = true,
                            Track = track,
                            ArtistCreated = createArtist,
                            Artist = artist,
                            AlbumCreated = createAlbum,
                            Album = album
                        };
                    }
                    catch (Exception)
                    {
                        await tr.RollbackAsync();
                        return new IndexTrackTransactionResult() { Successful = false };
                    }
                }
            }

            var group = await folders.GroupJoinAsync(TrackSource.Items, x => x.ViewModel.Id, x => x.FolderId, (mix, dbFiles) => (mix, dbFiles.ToList()));

            await group.ForEachAsync(async g =>
            {
                var dbFiles = g.Item2;
                var diskFiles = g.mix.Files;
                var dbFolder = await db.Folders.SingleAsync(x => x.Id == g.mix.ViewModel.Id);

                await diskFiles.ForEachAsync(async f =>
                {
                    var trackRx = await dbFiles.FirstOrDefaultAsync(x => x.FileName == f.Name && x.Path == f.Path);

                    if (trackRx == null)
                    {
                        var result = await IndexTrackTransactionAsync(f, dbFolder);
                        if (result.Successful == false) return;

                        if (result.ArtistCreated)
                        {
                            var artistVm = ArtistViewModel.Create(result.Artist);
                            _artistSource.AddOrUpdate(artistVm);
                        }
                        else
                        {
                            var artistVm = _artistSource.Lookup(result.Artist.Id).Value;
                            if (result.AlbumCreated)
                            {
                                var albumVm = AlbumViewModel.Create(artistVm, result.Album);
                                artistVm.Albums.AddOrUpdate(albumVm);
                            }
                            else
                            {
                                var albumVm = artistVm.Albums.Items.Single(x => x.Id == result.Album.Id);
                                var trackVm = TrackViewModel.Create(albumVm, result.Track);
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

        private async Task RemoveTrackAsync(long id)
        {
            Track track = default;
            Album album = default;
            Artist artist = default;
            var albumDeleted = false;
            var artistDeleted = false;
            var trackDeleted = false;

            async Task RemoveTrackDbAsync()
            {
                track = await db.Tracks.SingleAsync(x => x.Id == id);

                try
                {
                    album = track.Album;
                    artist = album.Artist;
                    db.Tracks.Remove(track);
                    await db.SaveChangesAsync();

                    albumDeleted = album.Tracks.Count == 0;
                    if (albumDeleted)
                    {
                        db.Albums.Remove(album);
                        await db.SaveChangesAsync();

                        artistDeleted = artist.Albums.Count == 0;
                        if (artistDeleted)
                        {
                            db.Artists.Remove(artist);
                            await db.SaveChangesAsync();
                        }
                    }

                    trackDeleted = true;
                }
                catch (Exception)
                {
                    trackDeleted = false;
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

        private async Task RemovedFolders(IEnumerable<FolderViewModel> vm)
        {
            var foldersId = await vm.Select(x => x.Id).ToListAsync();
            var folders = await db.Folders.WhereAsync(x => foldersId.Contains(x.Id));
            var group = await folders.GroupJoinAsync(TrackSource.Items, x => x.Id, x => x.FolderId, (x, y) => (folder: x, tracks: y));

            await group.ForEachAsync(async g =>
            {
                await g.tracks.ForEachAsync(async x => await RemoveTrackAsync(x.Id));

                db.Folders.Remove(g.folder);
            });
        }

        internal static async Task InitializeAsync()
        {
            Observable.Using(
                () => Db.Instance,
                db => Observable.Merge(new IObservable<object>[]
                {
                    db.Set<Folder>().ToList()
                        .ToObservable()
                        .ObservableOnThreadPool()
                        .Select(x => FolderViewModel.Create(x))
                        .Do(_musicFolders.AddOrUpdate),
                    db.Set<Artist>().ToList()
                        .ToObservable()
                        .ObservableOnThreadPool()
                        .Select(x => ArtistViewModel.Create(x))
                        .Do(_artistSource.AddOrUpdate)
                }).IgnoreElements())
                .Subscribe();

            AlbumSource = ArtistSource.Connect()
                .ObservableOnThreadPool()
                .MergeMany(x => x.Albums.Connect())
                .AsObservableCache();

            TrackSource = AlbumSource.Connect()
                .ObservableOnThreadPool()
                .MergeMany(x => x.Tracks.Connect())
                .AsObservableCache();

            Observable.Merge(new[]
            {
                FolderWatcherManager.ContentChanged.Select(_ => "FolderWatcherManager.ContentChanged"),
                //Setting.SettingChanged[Setting.Collection.AutoRefresh].Where(x => (bool)x == true).Select(_ => Unit.Default).Select(_ => "Setting.SettingChanged[Setting.Collection.AutoRefresh]"),
            })
            .ObservableOnThreadPool()
            //.Throttle(TimeSpan.FromSeconds(5))
            .Subscribe(async _ =>
            {
                Debug.WriteLine(_);
                var o = new IndexService();
                await o.BeginIndexAsync();
            });

            await FolderWatcherManager.InitializeAsync();
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
                var o = new IndexService();
                await o.RequestAddFolderAsyncInner(folder);
            }
        }

        private async Task RequestAddFolderAsyncInner(StorageFolder folder)
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
                Token = StorageApplicationPermissions.FutureAccessList.Add(folder)
            };

            await db.AddAsync(f);
            await db.SaveChangesAsync();

            _musicFolders.AddOrUpdate(FolderViewModel.Create(f));

            //Setting.AddOrUpdate(Setting.Collection.Indexing, true);
        }
    }
}