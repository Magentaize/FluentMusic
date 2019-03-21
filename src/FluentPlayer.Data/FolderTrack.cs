using System.ComponentModel.DataAnnotations;

namespace Magentaize.FluentPlayer.Data
{
    public class FolderTrack
    {
        [Key]
        public long FolderTrackId { get; set; }

        public Folder Folder { get; set; }

        public Track Track { get; set; }
    }
}