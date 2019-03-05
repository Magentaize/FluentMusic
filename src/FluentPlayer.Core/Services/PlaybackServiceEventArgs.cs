using System;

namespace Magentaize.FluentPlayer.Core.Services
{
    public class NewTrackPlayedEventArgs : EventArgs
    {
        public string TrackTitle { get; internal set; }
        public string TrackArtist { get; internal set; }
        public TimeSpan NaturalDuration { get; internal set; }

        internal NewTrackPlayedEventArgs() { }
    }
}