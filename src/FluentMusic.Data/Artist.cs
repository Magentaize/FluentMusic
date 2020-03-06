using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FluentMusic.Data
{
    public class Artist
    {
        [Key]
        public long Id { get; set; }

        public string Name { get; set; }

        public IList<Album> Albums { get; set; } = new List<Album>();
    }
}