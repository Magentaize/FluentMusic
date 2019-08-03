using System.ComponentModel.DataAnnotations;

namespace FluentMusic.Data
{
    public class QueuedTrack
    {
        [Key]
        public long QueuedTrackId { get; set; }

        public string Path { get; set; }

        public string SafePath { get; set; }

        public bool IsPlaying { get; set; }

        public long ProgressSeconds { get; set; }

        public long OrderId { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return QueuedTrackId.Equals(((QueuedTrack)obj).QueuedTrackId);
        }

        public override int GetHashCode()
        {
            return new { QueuedTrackID = QueuedTrackId }.GetHashCode();
        }
    }
}