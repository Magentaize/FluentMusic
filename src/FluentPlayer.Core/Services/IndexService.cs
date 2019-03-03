using Magentaize.FluentPlayer.Core.Extensions;
using Magentaize.FluentPlayer.Core.Storage;
using Magentaize.FluentPlayer.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Media.Core;
using TagLib;
using Windows.Storage;
using Windows.Storage.Streams;
using File = TagLib.File;
using Track = Magentaize.FluentPlayer.Data.Track;

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
            var ins = new IndexService();
            return await Task.FromResult(ins);
        }

        public async Task<List<Track>> GetAllTracksAsync()
        {
            var tracks = await ServiceFacade.Db.Tracks.Include(t=>t.Album).ToListAsync();

            return tracks;
        }

        public async Task<List<Artist>> GetAllArtistsAsync()
        {
            return await ServiceFacade.Db.Artists.ToListAsync();
        }

        private string SubFirstCharacter(string str)
        {
            return str.Substring(0, 1);
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

            // remove deleted tracks in database
            ServiceFacade.Db.Tracks.RemoveRange(deletedTracks);
            await ServiceFacade.Db.SaveChangesAsync();

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

            IndexFinished?.Invoke(this, null);
        }

        private async Task IndexFileAsync(IStorageFile file)
        {
            var path = file.Path;
            var tFile = File.Create(await UwpFileAbstraction.CreateAsync(file));

            var prop = tFile.Properties;
            var tag = tFile.Tag;

            // search for existed artist
            var artistName = tag.FirstPerformer ?? "Unknown";
            var artist = await ServiceFacade.Db.Artists.FirstOrDefaultAsync(a=>a.Name== artistName);
            if (artist == null)
            {
                artist = new Artist()
                {
                    Name = tag.FirstPerformer ?? "Unknown",
                };
                await ServiceFacade.Db.Artists.AddAsync(artist);
            }

            // search for existed album
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
                else if(_albumCoverList.TryGetValue(Path.GetDirectoryName(path), out var pic))
                {
                    picData = await FileIO.ReadBufferAsync(pic);
                }

                album.AlbumCover = picData == null ? default : await ServiceFacade.CacheService.CacheAsync(picData);
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

            artist.Albums.Add(album);

            await ServiceFacade.Db.Tracks.AddAsync(track);

            await ServiceFacade.Db.SaveChangesAsync();
        }
    }
}