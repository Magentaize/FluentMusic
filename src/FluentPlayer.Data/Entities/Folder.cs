using SQLite;

namespace FluentPlayer.Data.Entities
{
    public class Folder
    {
        [PrimaryKey, AutoIncrement]
        public long FolderId { get; set; }

        public string Path { get; set; }

        public string SafePath { get; set; }

        public long ShowInCollection { get; set; }
    }
}