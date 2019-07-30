using Magentaize.FluentPlayer.ViewModels.Common;
using System;

namespace Magentaize.FluentPlayer.Core.Services
{
    public class NextTrackGenerator
    {
        public PlaylistMode PlaylistMode { get; private set; }
        public TrackViewModel CurrentTrack { get; private set; }
        private PlaybackService _playback;
        private int _playlistIndex = 0;

        public NextTrackGenerator(PlaybackService playback)
        {
            _playback = playback;
            playback.NewTrackPlayed
                .Subscribe(x => CurrentTrack = x.Track);
        }

        public TrackViewModel Next()
        {
            switch (PlaylistMode)
            {
                case PlaylistMode.RepeatOne:
                    return CurrentTrack;
                case PlaylistMode.Normal:
                    return _playback._trackPlaybackList[++_playlistIndex];
                default: throw new InvalidOperationException();
            }
        }

        public bool HasNext()
        {
            switch (PlaylistMode)
            {
                default:
                case PlaylistMode.Normal:
                    return _playlistIndex < _playback._trackPlaybackList.Count;
                case PlaylistMode.RepeatOne:
                case PlaylistMode.RepeatAll:
                case PlaylistMode.Shuffle:
                    return true;
            }
        }
    }
}
