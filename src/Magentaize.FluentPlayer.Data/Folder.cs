using System.ComponentModel.DataAnnotations;

namespace Magentaize.FluentPlayer.Data
{
    public class Folder
    {
        [Key]
        public long FolderId { get; set; }

        public string Path { get; set; }

        public string SafePath { get; set; }

        public bool ShowInCollection { get; set; }
    }
}