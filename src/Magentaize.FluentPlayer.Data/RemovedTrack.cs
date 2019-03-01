using System.ComponentModel.DataAnnotations;

namespace Magentaize.FluentPlayer.Data
{
    public class RemovedTrack
    {
        [Key]
        public long TrackId { get; set; }

        public string Path { get; set; }

        public string SafePath { get; set; }

        public long DateRemoved { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return SafePath.Equals(((RemovedTrack)obj).SafePath);
        }

        public override int GetHashCode()
        {
            return new { SafePath }.GetHashCode();
        }
    }
}