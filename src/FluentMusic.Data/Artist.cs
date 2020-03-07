using FluentMusic.Data.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FluentMusic.Data
{
    public class Artist : LazyEntity
    {
        [Key]
        public long Id { get; set; }

        public string Name { get; set; }

        private ICollection<Album> _albums;
        public ICollection<Album> Albums
        {
            get => LazyLoader.Load(this, ref _albums);
            set => _albums = value;
        }

        private Artist(Action<object, string> lazyLoader) : base(lazyLoader) { }

        public Artist()
        {
        }
    }
}