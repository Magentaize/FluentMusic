using System.ComponentModel.DataAnnotations;

namespace Magentaize.FluentPlayer.Data
{
    public class IndexingTrack
    {
        [Key] public long Id { get; set; }

        public string Path { get; set; }
    }
}