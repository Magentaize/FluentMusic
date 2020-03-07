using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace FluentMusic.Data
{
    public class Db : DbContext
    {
        public DbSet<Folder> Folders { get; set; }

        public DbSet<Album> Albums { get; set; }

        public DbSet<Artist> Artists { get; set; }

        public DbSet<Track> Tracks { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
            builder.UseSqlite($"Data Source={_dbpath}");
        }

        private static string _dbpath;

        public static async Task InitializeAsync(string dbpath)
        {
            _dbpath = dbpath;

            var db = new Db();
            await db.Database.MigrateAsync();
            await db.SaveChangesAsync();
        }
        public static Db Instance => new Db();

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