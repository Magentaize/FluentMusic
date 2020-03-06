using FluentMusic.Core.Services;
using SQLite;
using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace FluentMusic.Core.Storage
{
    public class Artist
    {
        [PrimaryKey, AutoIncrement]
        public long Id { get; set; }

        public string Name { get; set; }

        public long AlbumId { get; set; }

        public long TrackId { get; set; }
    }

    public class Folder
    {
        [PrimaryKey, AutoIncrement]
        public long Id { get; set; }

        [Unique]
        public string Path { get; set; }

        public DateTimeOffset DateModified { get; set; }

        public string Token { get; set; }

        public Folder() { }

        public Folder(StorageFolder f)
        {
            Token = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(f);
        }

        public async Task<StorageFolder> GetStorageFolderAsync()
        {
            var f = await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFolderAsync(Token);
            if (f.Path != Path)
            {
                Path = f.Path;
                await Db.UpdateFolderAsync(this);
            }
            return f;
        }
    }

    public class Track
    {
        [PrimaryKey, AutoIncrement]
        public long Id { get; set; }

        public long ArtistId { get; set; }

        public long AlbumId { get; set; }

        public string Genres { get; set; }

        public string Artist { get; set; }

        public string Path { get; set; }

        public string SafePath { get; set; }

        public string FileName { get; set; }

        public string MimeType { get; set; }

        public ulong? FileSize { get; set; }

        public long? BitRate { get; set; }

        public long? SampleRate { get; set; }

        public string TrackTitle { get; set; }

        public long? TrackNumber { get; set; }

        public long? TrackCount { get; set; }

        public long? DiscNumber { get; set; }

        public long? DiscCount { get; set; }

        public TimeSpan? Duration { get; set; }

        public long? Year { get; set; }

        public long? HasLyrics { get; set; }

        public long DateAdded { get; set; }

        public long DateFileCreated { get; set; }

        public long DateLastSynced { get; set; }

        public long DateFileModified { get; set; }

        public long? NeedsIndexing { get; set; }

        public long? NeedsAlbumArtworkIndexing { get; set; }

        public long? IndexingSuccess { get; set; }

        public string IndexingFailureReason { get; set; }

        public long? Rating { get; set; }

        public long? Love { get; set; }

        public long? PlayCount { get; set; }

        public long? SkipCount { get; set; }

        public long? DateLastPlayed { get; set; }

    }

    public class TrackFolder
    {
        [PrimaryKey, AutoIncrement]
        public long Id { get; set; }
        
        public long TrackID { get; set; }

        public long FolderID { get; set; }
    }

    public class Album
    {
        [PrimaryKey, AutoIncrement]
        public long Id { get; set; }

        public string Name { get; set; }

        public string Cover { get; set; }

        public long ArtistId { get; set; }
    }
}
