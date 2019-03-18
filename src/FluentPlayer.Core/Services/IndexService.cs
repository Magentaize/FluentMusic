using Magentaize.FluentPlayer.Core.Extensions;
using Magentaize.FluentPlayer.Core.Storage;
using Magentaize.FluentPlayer.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TagLib;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Magentaize.FluentPlayer.Core.Services
{
    public class IndexService
    {
        public event EventHandler IndexBegin;
        public event EventHandler IndexProgressChanged;
        public event EventHandler IndexFinished;

        public int QueueIndexingCount { get; private set; }
        public int QueueIndexedCount { get; private set; }

        private IndexService() { }

        internal static async Task<IndexService> CreateAsync()
        {
            var index = new IndexService();

            var dbTracks = ServiceFacade.Db.Tracks.Include(t => t.Album).Include(t => t.Artist).AsEnumerable();
            index._tracks = new ObservableCollection<Track>(dbTracks);

            var dbArtists = ServiceFacade.Db.Artists.Include(a => a.Tracks).Include(a => a.Albums).AsEnumerable();
            index._artists = new ObservableCollection<Artist>(dbArtists);

            var dbAlbums = ServiceFacade.Db.Albums.Include(a => a.Tracks).Include(a => a.Artist).AsEnumerable();
            index._albums = new ObservableCollection<Album>(dbAlbums);

            return await Task.FromResult(index);
        }

        private ObservableCollection<Track> _tracks;
        private ObservableCollection<Artist> _artists;
        private ObservableCollection<Album> _albums;

        public async Task<ObservableCollection<Track>> GetAllTracksAsync()
        {
            return await Task.FromResult(_tracks);
        }

        public async Task<ObservableCollection<Artist>> GetAllArtistsAsync()
        {
            return await Task.FromResult(_artists);
        }

        public async Task<ObservableCollection<Album>> GetAllAlbumsAsync()
        {
            return await Task.FromResult(_albums);
        }

        private IDictionary<string, StorageFile> _albumCoverList;

        public async Task BeginIndexAsync()
        {
            var list = new List<StorageFolder> {KnownFolders.MusicLibrary};

            var libAudioFiles = (await list.SelectManyAsync<StorageFolder, StorageFile>(async f =>
                await StorageFolderQuery.Create(f, StorageFolderQuery.AudioExtensions).ExecuteQueryAsync())).ToList();
            var filesPath = libAudioFiles.Select(f => f.Path).ToArray();
            var dbTracks = await ServiceFacade.Db.Tracks.ToListAsync();
            var dbTracksPath = dbTracks.Select(t => t.Path).ToArray();

            var commonPath = filesPath.Intersect(dbTracksPath).ToArray();

            var newFiles = libAudioFiles.Where(f => !commonPath.Contains(f.Path)).ToArray();
            var deletedTracks = dbTracks.Where(t => !commonPath.Contains(t.Path));

            // begin index
            QueueIndexingCount = newFiles.Count();
            QueueIndexedCount = 0;
            IndexBegin?.Invoke(this, null);

            // if there are some files need to be indexed, query cover pictures in library
            if (newFiles.Count() != 0)
            {
                var pics = (await list.SelectManyAsync<StorageFolder, StorageFile>(async f =>
                    await StorageFolderQuery.Create(f, StorageFolderQuery.AlbumCoverExtensions).ExecuteQueryAsync())).ToList();
                _albumCoverList = pics.ToDictionary(p => Path.GetDirectoryName(p.Path));
            }

            foreach (var file in newFiles)
            {
                await IndexFileAsync(file);
                QueueIndexedCount++;
                IndexProgressChanged?.Invoke(this, null);
            }

            await RemoveIndexedFileAsync(deletedTracks);

            IndexFinished?.Invoke(this, null);
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
        private async Task<(bool, Artist)> FindOrCreateArtist(Tag tag)
        {
            var artistName = tag.FirstPerformer ?? "Unknown";
            var artist = await ServiceFacade.Db.Artists.FirstOrDefaultAsync(a => a.Name == artistName);
            if (artist == null)
            {
                artist = new Artist()
                {
                    Name = tag.FirstPerformer ?? "Unknown",
                };

                await ServiceFacade.Db.Artists.AddAsync(artist);
                return (true, artist);
            }

            return (false, artist);
        }

        // search for existed album
        private async Task<(bool, Album)> FindOrCreateAlbum(Tag tag, string path)
        {
            var albumTitle = tag.Album ?? "Unknown";
            var album = await ServiceFacade.Db.Albums.FirstOrDefaultAsync(a => a.Title == albumTitle);
            if (album == null)
            {
                album = new Album()
                {
                    Title = albumTitle,
                    AlbumCover = tag.FirstAlbumArtist ?? "Unknown",
                };

                IBuffer picData = null;
                if (tag.Pictures != null && tag.Pictures.Length >= 1)
                {
                    picData = tag.Pictures[0].Data.Data.AsBuffer();
                }
                else if (_albumCoverList.TryGetValue(Path.GetDirectoryName(path), out var pic))
                {
                    picData = await FileIO.ReadBufferAsync(pic);
                }

                album.AlbumCover = picData == null ? default : await ServiceFacade.CacheService.CacheAsync(picData);

                return (true, album);
            }

            return (false, album);
        }

        private async Task IndexFileAsync(IStorageFile file)
        {
            var path = file.Path;
            var tFile = TagLib.File.Create(await UwpFileAbstraction.CreateAsync(file));

            var prop = tFile.Properties;
            var tag = tFile.Tag;

            (var albumCreated, var album) = await FindOrCreateAlbum(tag, path);
            (var artistCreated, var artist) = await FindOrCreateArtist(tag);

            if (artistCreated)
            {
                _artists.Add(artist);
                artist.Albums.Add(album);
            }

            if (albumCreated)
            {
                _albums.Add(album);
                album.Artist = artist;
            }

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
            _tracks.Add(track);

            await ServiceFacade.Db.SaveChangesAsync();
        }
    }
}