using Microsoft.EntityFrameworkCore;

namespace FluentMusic.Data
{
    public class FluentMusicDbContext : DbContext
    {
        public DbSet<Folder> Folders { get; set; }

        public DbSet<FolderTrack> FolderTracks { get; set; }

        public DbSet<Album> Albums { get; set; }

        public DbSet<Artist> Artists { get; set; }

        public DbSet<Track> Tracks { get; set; }

        public DbSet<IndexingTrack> IndexingTracks { get; set; }

        public DbSet<RemovingTrack> RemovingTracks { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
            builder.UseSqlite("Data Source=FluentMusic.db");
        }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    base.OnModelCreating(modelBuilder);

        //    modelBuilder.Entity<AlbumTrack>()
        //        .HasKey(at => new { at.AlbumId, at.TrackId });

        //    modelBuilder.Entity<AlbumTrack>()
        //        .HasOne(at => at.Track)
        //        .WithMany(t => t.Artist)
        //}
    }
}