using DynamicData;
using DynamicData.Annotations;
using Magentaize.FluentPlayer.Core.Storage;
using Magentaize.FluentPlayer.Data;
using Magentaize.FluentPlayer.ViewModels.DataViewModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Toolkit.Extensions;
using ReactiveUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TagLib;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.Storage.Streams;

namespace Magentaize.FluentPlayer.Core.Services
{
    public class IndexService
    {
        private readonly ISourceList<Track> _trackSource = new SourceList<Track>();

        public IObservable<IChangeSet<Track>> TrackSource => _trackSource.Connect();

        private readonly ISourceCache<ArtistViewModel, long> _artistSource;

        public IObservableCache<ArtistViewModel, long> ArtistSource => _artistSource.AsObservableCache();

        private readonly ISourceCache<AlbumViewModel, long> _albumSource;

        public IObservable<IChangeSet<AlbumViewModel, long>> AlbumSource { get; private set; }

        private readonly ObservableCollection<StorageFolder> _musicFolders = new ObservableCollection<StorageFolder>();

        public ReadOnlyObservableCollection<StorageFolder> MusicFolders { get; private set; }

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
            _artistSource = new SourceCache<ArtistViewModel, long>(x => x.Id);

            _albumSource = new SourceCache<AlbumViewModel, long>(x => x.Id);
            AlbumSource = _albumSource.Connect();
        }

        private async Task<(IEnumerable<Folder> removedFolders, IEnumerable<StorageFolder> changedFolders)> GetChangedLibraryFolders()
        {
            var dbFolders = await ServiceFacade.Db.Folders.ToListAsync();
            var removedFolders = dbFolders.Except(_musicFolders, a => a.Path, b => b.Path).ToList();

            dbFolders.RemoveAll(x => removedFolders.Contains(x));
            var musicFoldersMixin = await Task.WhenAll(
                _musicFolders.Select(async x =>
                {
                    var prop = await x.GetBasicPropertiesAsync();
                    return new { Obj = x, Prop = prop };
                }));

            var indexNeededFolders = musicFoldersMixin.Where(x =>
            {
                var f = dbFolders.FirstOrDefault(y => y.Path == x.Obj.Path);
                if (f == default(Folder)) return true;
                if (f.ModifiedDate != x.Prop.DateModified) return true;
                return false;
            }).Select(x => x.Obj);

            return (removedFolders, indexNeededFolders);
        }

        private async Task IndexChangedFolders(IEnumerable<StorageFolder> folders)
        {
            var libFolderSfq =
               folders
               .Select(x => new StorageFolderQuery(x, StorageFolderQuery.AudioExtensions))
               .ToList();

            await IndexChangedFolders(libFolderSfq);

            _libForlderContentChangedStream = Observable.Never<EventPattern<IStorageQueryResultBase, object>>();
            libFolderSfq.ForEach(x => _libForlderContentChangedStream = _libForlderContentChangedStream.Merge(x.ContentsChangedStream));
            var d = _libForlderContentChangedStream
                .Throttle(TimeSpan.FromSeconds(3))
                .Subscribe(async _ => await IndexChangedFolders(libFolderSfq));
        }

        private async Task IndexChangedFolders(IEnumerable<StorageFolderQuery> folderSfq)
        {
            var libFiles = (await folderSfq.SelectManyAsync<StorageFolderQuery, StorageFile>(async f =>
                await f.ExecuteQueryAsync())).ToList();
            var dbTracks = await ServiceFacade.Db.Tracks.ToListAsync();
            var commonPath = libFiles.Intersect(dbTracks, a => a.Path, b => b.Path).ToHashSet();

            var newFiles = libFiles.Except(commonPath, f => f.Path).ToArray();
            var deletedTracks = dbTracks.Except(commonPath, t => t.Path);

            foreach (var file in newFiles)
            {
                await IndexFileAsync(file);
            }

            await RemoveIndexedFileAsync(deletedTracks);
        }

        private async Task IndexRemovedFolders(IEnumerable<Folder> folders)
        {

        }

        public async Task BeginIndexAsync()
        {
            var (r, i) = await GetChangedLibraryFolders();
            await IndexRemovedFolders(r);
            await IndexChangedFolders(i);
        }

        internal async Task<IndexService> InitializeAsync()
        {
            MusicFolders = new ReadOnlyObservableCollection<StorageFolder>(_musicFolders);
            _musicLibrary = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Music);
            _musicFolders.AddRange(_musicLibrary.Folders);

            var _musicLibrary_DefinitionChanged = Observable.FromEventPattern<TypedEventHandler<StorageLibrary, object>, StorageLibrary, object>(
                h => _musicLibrary.DefinitionChanged += h, h => _musicLibrary.DefinitionChanged -= h);

            ServiceFacade.Db.Artists
                .Include(a => a.Albums)
                .ThenInclude(a => a.Tracks)
                .ToAsyncEnumerable()
                .Select(x => ArtistViewModel.Create(x))
                .ToObservable()
                .SubscribeOn(ThreadPoolScheduler.Instance)
                .Subscribe(x =>
                {
                    _artistSource.AddOrUpdate(x);
                });

            return this;
        }

        //remove deleted tracks in database
        private async Task RemoveIndexedFileAsync(IEnumerable<Track> tracks)
        {
            var group = tracks.GroupBy(t => t.Artist).GroupBy(at => at.Key.Albums);

            ServiceFacade.Db.Tracks.RemoveRange(tracks);
            await ServiceFacade.Db.SaveChangesAsync();

            foreach(var artist in group)
            {

            }
        }

        // search for existed artist
        private async Task<Artist> FindOrCreateArtist(Tag tag)
        {
            var artistName = string.IsNullOrEmpty(tag.FirstPerformer) ? "Unknown" : tag.FirstPerformer;
            var artist = await ServiceFacade.Db.Artists
                .Include(x => x.Albums)
                .FirstOrDefaultAsync(a => a.Name == artistName);

            if (artist == default(Artist))
            {
                artist = new Artist()
                {
                    Name = artistName,
                };

                await ServiceFacade.Db.Artists.AddAsync(artist);
                await ServiceFacade.Db.SaveChangesAsync();

                _artistSource.AddOrUpdate(ArtistViewModel.Create(artist));
            }

            await ServiceFacade.Db.SaveChangesAsync();

            return artist;
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

        // search for existed album
        private async Task<Album> FindOrCreateAlbum(Tag tag, string path, [NotNull] Artist artist)
        {
            var albumTitle = string.IsNullOrEmpty(tag.Album) ? "Unknown" : tag.Album;
            var album = artist.Albums.FirstOrDefault(x => x.Title == albumTitle);
            AlbumViewModel vm = default;

            if (album == default)
            {
                album = new Album()
                {
                    Title = albumTitle,
                };

                artist.Albums.Add(album);
                album.Artist = artist;

                await ServiceFacade.Db.SaveChangesAsync();

                vm = AlbumViewModel.Create(album);
                _artistSource.Lookup(artist.Id).Value.AddAlbum(vm);
            }

            if (string.IsNullOrEmpty(album.AlbumCover))
            {
                var picData = await FindAlbumCoverAsync(tag, path);

                if (picData != default(IBuffer))
                {
                    var cover = await ServiceFacade.CacheService.CacheAsync(picData);
                    album.AlbumCover = cover;
                    if (vm == default(AlbumViewModel))
                    {
                        vm = _artistSource.Lookup(artist.Id).Value.Albums.Items.First(x => x.Title == albumTitle);
                    }
                    vm.AlbumCover = cover;
                }
            }

            await ServiceFacade.Db.SaveChangesAsync();

            return album;
        }

        private async Task<Track> CreateTrackAsync(IStorageFile file, TagLib.File tFile, Artist artist, Album album)
        {
            var tag = tFile.Tag;
            var prop = tFile.Properties;
            var fbp = await file.GetBasicPropertiesAsync();

            var track = new Track()
            {
                Path = file.Path,
                FileName = Path.GetFileNameWithoutExtension(file.Path),
                IndexingSuccess = 0,
                DateAdded = DateTime.Now.Ticks,
                TrackTitle = tag.Title,
                Year = tag.Year,
                Duration = prop.Duration,
                Artist = artist,
                Genres = tag.FirstGenre ?? "Unknown",
                FileSize = fbp.Size
            };

            await ServiceFacade.Db.Tracks.AddAsync(track);
            await ServiceFacade.Db.SaveChangesAsync();

            var vm = TrackViewModel.Create(track);
            _artistSource.Lookup(artist.Id).Value.Albums.Items.First(x => x.Id == album.Id).AddTrack(vm);

            return track;
        }

        private async Task IndexFileAsync(IStorageFile file)
        {
            var path = file.Path;
            var tFile = TagLib.File.Create(await UwpFileAbstraction.CreateAsync(file));
       
            var tag = tFile.Tag;

            var artist = await FindOrCreateArtist(tag);
            var album = await FindOrCreateAlbum(tag, path, artist);
            var track = await CreateTrackAsync(file, tFile, artist, album);

            artist.Tracks.Add(track);
            album.Tracks.Add(track);

            await ServiceFacade.Db.SaveChangesAsync();
        }

        public async Task RequestRemoveFolderAsync(StorageFolder folder)
        {
            var result = await _musicLibrary.RequestRemoveFolderAsync(folder);
            if (result)
            {
                _musicFolders.Remove(folder);
            }
        }

        public async Task RequestAddFolderAsync()
        {
            var f = await _musicLibrary.RequestAddFolderAsync();
            if (f != null)
            {
                _musicFolders.Add(f);
            }
        }
    }
}