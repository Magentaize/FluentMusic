using DynamicData;
using Magentaize.FluentPlayer.Core.Extensions;
using Magentaize.FluentPlayer.ViewModels.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Magentaize.FluentPlayer.Core.Services
{
    public class NextTrackGenerator
    {
        public RepeatMode PlaylistMode { get; private set; }
        public bool EnableShuffle { get; private set; }
        public TrackViewModel CurrentTrack { get; private set; }
        private IList<TrackViewModel> _playlist;
        private IList<TrackViewModel> _shuffledPlaylist;
        private ThresholdInt _playlistIndex = new ThresholdInt();

        public NextTrackGenerator()
        {
            var playback = ServiceFacade.PlaybackService;
            playback.RepeatMode
                .DistinctUntilChanged()
                .Subscribe(x =>
                {
                    PlaylistMode = x;
                });

            playback.EnableShuffle
                .DistinctUntilChanged()
                .Subscribe(x =>
                {
                    EnableShuffle = x;
                });

            playback.NewTrackPlayed
                .Subscribe(x => CurrentTrack = x.Track);

            ServiceFacade.IndexService.TrackSource
                .SelectMany(x => x.Unified())
                .Where(x => x.Reason == ListChangeReason.Remove)
                .Subscribe(x =>
                {
                    //TODO: remove vm
                });
        }

        public TrackViewModel Previous()
        {
            switch (PlaylistMode)
            {
                case RepeatMode.RepeatOne:
                    //break;
                case RepeatMode.NotRepeat:
                case RepeatMode.RepeatAll:
                    _playlistIndex.Decrease();
                    if (EnableShuffle)
                    {
                        CurrentTrack = _shuffledPlaylist[_playlistIndex.Value];
                    }
                    else
                    {
                        CurrentTrack = _playlist[_playlistIndex.Value];
                    }
                    break;
                default: throw new InvalidOperationException();
            }

            return CurrentTrack;
        }

        public TrackViewModel Next()
        {
            switch (PlaylistMode)
            {
                case RepeatMode.RepeatOne:
                    //break;
                case RepeatMode.NotRepeat:
                case RepeatMode.RepeatAll:
                    if (EnableShuffle)
                    {
                        CurrentTrack = _shuffledPlaylist[_playlistIndex.Value];
                    }
                    else
                    {
                        CurrentTrack = _playlist[_playlistIndex.Value];
                    }
                    _playlistIndex.Increase();
                    break;
                default: throw new InvalidOperationException();
            }

            return CurrentTrack;
        }

        public bool HasNext()
        {
            switch (PlaylistMode)
            {
                case RepeatMode.NotRepeat:
                    return _playlistIndex.Value < _playlistIndex.Threshold;
                case RepeatMode.RepeatOne:
                    return true;
                case RepeatMode.RepeatAll:
                    return true;
                default:throw new InvalidCastException();
            }
        }

        public void Reset(IEnumerable<TrackViewModel> list)
        {
            _playlist = new List<TrackViewModel>(list);
            _shuffledPlaylist = list.Shuffle();
            _playlistIndex.Reset();
            _playlistIndex.Threshold = _playlist.Count - 1;
        }

        private struct ThresholdInt
        {
            public int Threshold { get; set; }

            public int Value { get; private set; }

            public void Reset()
            {
                Threshold = 0;
                Value = 0;
            }

            public void Decrease()
            {
                if (Value == 0)
                {
                    Value = Threshold;
                }
                else
                {
                    Value -= 1;
                }
            }

            public void Increase()
            {
                if (Value == Threshold)
                {
                    Value = 0;
                }
                else
                {
                    Value += 1;
                }
            }
        }
    }
}
