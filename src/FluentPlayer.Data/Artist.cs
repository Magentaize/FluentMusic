using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Magentaize.FluentPlayer.Data
{
    public class Artist
    {
        [Key]
        public long Id { get; set; }

        public string Name { get; set; }

        public IList<Album> Albums { get; set; } = new List<Album>();

        public IList<Track> Tracks { get; set; } = new List<Track>();
    }
}