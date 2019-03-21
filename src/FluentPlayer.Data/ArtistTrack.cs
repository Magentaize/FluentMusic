namespace Magentaize.FluentPlayer.Data
{
    public class ArtistTrack
    {
        public long ArtistId { get; set; }

        public Artist Artist { get; set; }

        public long TrackId { get; set; }

        public Track Track { get; set; }
    }
}