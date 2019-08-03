using System.ComponentModel.DataAnnotations;

namespace FluentMusic.Data
{
    public class IndexingTrack
    {
        [Key] public long Id { get; set; }

        public string Path { get; set; }
    }
}