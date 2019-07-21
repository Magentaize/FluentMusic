using DynamicData;
using Magentaize.FluentPlayer.Core.Storage;
using Magentaize.FluentPlayer.Data;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TagLib;
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

        private readonly ISourceList<Artist> _artistSource = new SourceList<Artist>();

        public IObservable<IChangeSet<Artist>> ArtistSource => _artistSource.Connect();

        private readonly ISourceCache<Album, long> _albumSource = new SourceCache<Album, long>(x => x.Id);

        public IObservable<IChangeSet<Album, long>> AlbumSource => _albumSource.Connect();

        private readonly ObservableCollection<StorageFolder> _musicFolders = new ObservableCollection<StorageFolder>();

        public ReadOnlyObservableCollection<StorageFolder> MusicFolders { get; private set; }

        public event EventHandler IndexBegin;
        public event EventHandler IndexProgressChanged;
        public event EventHandler IndexFinished;

        private static readonly string DefaultCoverImage = "ms-appx:///Assets/Square150x150Logo.scale-200.png";
        private static readonly IList<string> AlbumCoverFileNames = new List<string> { ".jpg", ".jpeg", ".png", ".bmp" }.Select(x => $"cover{x}").ToList();

        public int QueueIndexingCount { get; private set; }
        public int QueueIndexedCount { get; private set; }

        private StorageLibrary _musicLibrary;
        private IObservable<EventPattern<IStorageQueryResultBase, object>> _libForlderContentChangedStream;
        internal IndexService()
        {
            
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
                .Throttle(TimeSpan.FromSeconds(5))
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
            _musicLibrary.DefinitionChanged += _musicLibrary_DefinitionChanged;

            await ServiceFacade.Db.Tracks.Include(t => t.Album).Include(t => t.Artist)
                .ToAsyncEnumerable()
                .ForEachAsync(x => _trackSource.Add(x));

            await ServiceFacade.Db.Artists.Include(a => a.Tracks).Include(a => a.Albums)
                .ToAsyncEnumerable()
                .ForEachAsync(x => _artistSource.Add(x));

            await ServiceFacade.Db.Albums.Include(a => a.Tracks).Include(a => a.Artist)
                .ToAsyncEnumerable()
                .ForEachAsync(x => _albumSource.AddOrUpdate(x));

            return this;
        }

        private static void _musicLibrary_DefinitionChanged(StorageLibrary sender, object args)
        {
            
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

            if (artist == default)
            {
                artist = new Artist()
                {
                    Name = artistName,
                };

                await ServiceFacade.Db.Artists.AddAsync(artist);

                _artistSource.Add(artist);
            }

            return artist;
        }

        // search for existed album
        private async Task<Album> FindOrCreateAlbum(Tag tag, string path, Artist artist)
        {
            var albumTitle = string.IsNullOrEmpty(tag.Album) ? "Unknown" : tag.Album;
            var album = artist.Albums.FirstOrDefault(x => x.Title == albumTitle);

            if (album == default)
            {
                album = new Album()
                {
                    Title = albumTitle,
                };

                artist.Albums.Add(album);
                album.Artist = artist;
            }

            if (string.IsNullOrEmpty(album.AlbumCover))
            {
                IBuffer picData = null;
                var folder = Path.GetDirectoryName(path);

                StorageFile folderCover = null;
                foreach(var f in AlbumCoverFileNames)
                {
                    var fn = Path.Combine(folder, f);

                    try
                    {
                        folderCover = await StorageFile.GetFileFromPathAsync(fn);
                        break;
                    }
                    catch { }
                }

                if (folderCover != default(StorageFile))
                {
                    picData = await FileIO.ReadBufferAsync(folderCover);
                }
                else if (tag.Pictures?.Length >= 1)
                {
                    picData = tag.Pictures[0].Data.Data.AsBuffer();
                }

                album.AlbumCover = picData == null ? default : await ServiceFacade.CacheService.CacheAsync(picData);

                _albumSource.AddOrUpdate(album);
            }

            return album;
        }

        private async Task IndexFileAsync(IStorageFile file)
        {
            var path = file.Path;
            var tFile = TagLib.File.Create(await UwpFileAbstraction.CreateAsync(file));

            var prop = tFile.Properties;
            var tag = tFile.Tag;

            var artist = await FindOrCreateArtist(tag);
            var album = await FindOrCreateAlbum(tag, path, artist);

            var fbp = await file.GetBasicPropertiesAsync();

            var track = new Track()
            {
                Path = path,
                FileName = Path.GetFileNameWithoutExtension(path),
                IndexingSuccess = 0,
                DateAdded = DateTime.Now.Ticks,
                TrackTitle = tag.Title,
                Year = tag.Year,
                Duration = prop.Duration,
                Artist = artist,
                Genres = tag.FirstGenre ?? "Unknown",
                FileSize = fbp.Size
            };

            artist.Tracks.Add(track);
            album.Tracks.Add(track);

            await ServiceFacade.Db.Tracks.AddAsync(track);
            _trackSource.Add(track);

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