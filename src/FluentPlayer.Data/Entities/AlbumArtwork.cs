using SQLite;

namespace FluentPlayer.Data.Entities
{
    public class AlbumArtwork
    {
        [PrimaryKey(), AutoIncrement()]
        public long AlbumArtworkId { get; set; }

        public string AlbumKey { get; }

        public string ArtworkId { get; set; }

        public AlbumArtwork()
        {

        }

        public AlbumArtwork(string albumKey)
        {
            AlbumKey = albumKey;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return this.AlbumKey.Equals(((AlbumArtwork)obj).AlbumKey);
        }

        public override int GetHashCode()
        {
            return new { AlbumKey }.GetHashCode();
        }
    }
}