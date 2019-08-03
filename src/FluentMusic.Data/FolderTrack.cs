using System.ComponentModel.DataAnnotations;

namespace FluentMusic.Data
{
    public class FolderTrack
    {
        [Key]
        public long FolderTrackId { get; set; }

        public Folder Folder { get; set; }

        public Track Track { get; set; }
    }
}