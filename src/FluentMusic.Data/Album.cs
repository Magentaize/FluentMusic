using FluentMusic.Data.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FluentMusic.Data
{
    public class Album : LazyEntity
    {
        [Key]
        public long Id { get; set; }

        public string CoverCacheToken { get; set; }

        public string Title { get; set; }

        private ICollection<Track> _tracks;
        public ICollection<Track> Tracks
        {
            get => LazyLoader.Load(this, ref _tracks);
            set => _tracks = value;
        }

        private Artist _artist;
        public Artist Artist
        {
            get => LazyLoader.Load(this, ref _artist);
            set => _artist = value;
        }

        private Album(Action<object, string> lazyLoader) : base(lazyLoader) { }

        public Album()
        {
        }
    }
}