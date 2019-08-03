using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FluentMusic.Data
{
    public class Album
    {
        [Key]
        public long Id { get; set; }

        public string ArtworkPath { get; set; }

        public string Title { get; set; }

        public string AlbumCover { get; set; }

        public IList<Track> Tracks { get; set; } = new List<Track>();

        public Artist Artist { get; set; }
    }
}