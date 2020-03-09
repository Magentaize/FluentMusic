using FluentMusic.Data.Extensions;
using System;
using System.ComponentModel.DataAnnotations;

namespace FluentMusic.Data
{
    public class Track : LazyEntity, IIdEntity
    {
        private Track(Action<object, string> lazyLoader) : base(lazyLoader) { }

        public Track()
        {
        }

        [Key]
        public long Id { get; set; }

        private Folder _folder;
        public Folder Folder
        {
            get => LazyLoader.Load(this, ref _folder);
            set => _folder = value;
        }

        private Album _album;
        public Album Album
        {
            get => LazyLoader.Load(this, ref _album);
            set => _album = value;
        }

        public string Genres { get; set; }

        public DateTimeOffset DateModified { get; set; }

        public string Path { get; set; }

        public string FileName { get; set; }

        public string MimeType { get; set; }

        public ulong? FileSize { get; set; }

        public long? BitRate { get; set; }

        public long? SampleRate { get; set; }

        public string Title { get; set; }

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

        public Track ShallowCopy()
        {
            return (Track)MemberwiseClone();
        }
    }
}