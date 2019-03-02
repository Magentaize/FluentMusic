using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Aurora.Shared.Helpers;
using Magentaize.FluentPlayer.Core.Extensions;
using Magentaize.FluentPlayer.Core.Storage;
using Magentaize.FluentPlayer.Data;
using Microsoft.EntityFrameworkCore;

namespace Magentaize.FluentPlayer.Core.Services
{
    public class IndexService
    {
        public event EventHandler IndexBegin;

        public int IndexingCount { get; private set; }

        public void BeginIndex()
        {
            var list = new List<StorageFolder>();
            list.Add(KnownFolders.MusicLibrary);

            var fs = list.SelectMany(f => AsyncHelper.RunSync(async()=> await StorageFolderQuery.Create(f).ExecuteQueryAsync())).ToList();
        }

        public async Task BeginIndexAsync()
        {
            var list = new List<StorageFolder>();
            list.Add(KnownFolders.MusicLibrary);

            var files = (await list.SelectManyAsync<StorageFolder, StorageFile>(async f =>
                await StorageFolderQuery.Create(f).ExecuteQueryAsync())).ToList();
            var filesPath = files.Select(f => f.Path).ToArray();
            var dbTracks = await Singleton.Db.Tracks.ToListAsync();
            var dbTracksPath = dbTracks.Select(t => t.Path).ToArray();

            var commomPath = filesPath.Intersect(dbTracksPath).ToArray();

            var newFiles = files.Where(f => !commomPath.Contains(f.Path));
            var deletedTracks = dbTracks.Where(t => !commomPath.Contains(t.Path));

            // remove deleted tracks in database
            Singleton.Db.Tracks.RemoveRange(deletedTracks);
            await Singleton.Db.SaveChangesAsync();

            // begin index
            IndexingCount = newFiles.Count();
            IndexBegin?.Invoke(this, null);

            foreach (var file in newFiles)
            {
                
            }
        }

        private async Task IndexFileAsync(IStorageFile file)
        {
            var fileInfo = TagLibUWP.TagManager.ReadFile(file);
            var tag = fileInfo.Tag;
            var track = new Track
            {
                AlbumArtists =  tag.Album
            }
            Singleton.Db.Tracks
        }
    }
}