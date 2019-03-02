using Microsoft.EntityFrameworkCore;

namespace Magentaize.FluentPlayer.Data
{
    public class FluentPlayerDbContext : DbContext
    {
        public DbSet<Folder> Folders { get; set; }

        public DbSet<FolderTrack> FolderTracks { get; set; }

        public DbSet<AlbumArtwork> AlbumArtworks { get; set; }

        public DbSet<Track> Tracks { get; set; }

        public DbSet<IndexingTrack> IndexingTracks { get; set; }

        public DbSet<RemovingTrack> RemovingTracks { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
            builder.UseSqlite("Data Source=FluentPlayer.db");
        }
    }
}