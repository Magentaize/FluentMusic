﻿using DynamicData;
using FluentMusic.Core.Extensions;
using FluentMusic.ViewModels.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Windows.Media;

namespace FluentMusic.Core.Services
{
    public class PlaylistContainer
    {
        public MediaRepeatMode RepeatMode { get; private set; }
        public bool EnableShuffle { get; private set; }
        public TrackViewModel CurrentTrack { get; private set; }
        private IList<TrackViewModel> _playlist;
        private IList<TrackViewModel> _shuffledPlaylist;
        private ThresholdInt _playlistIndex = new ThresholdInt();

        public PlaylistContainer()
        {
            Setting.Playback.RepeatMode
                .DistinctUntilChanged()
                .Subscribe(x =>
                {
                    RepeatMode = x;
                });

            Setting.Playback.EnableShuffle
                .DistinctUntilChanged()
                .Subscribe(x =>
                {
                    EnableShuffle = x;
                });

            PlaybackService.NewTrackPlayed
                .Subscribe(x => CurrentTrack = x.Track);

            //Service.IndexService.TrackSource
            //    .SelectMany(x => x.Unified())
            //    .Where(x => x.Reason == ListChangeReason.Remove)
            //    .Subscribe(x =>
            //    {
            //        //TODO: remove vm
            //    });
        }

        public TrackViewModel Previous()
        {
            switch (RepeatMode)
            {
                case MediaRepeatMode.Track:
                //break;
                case MediaRepeatMode.None:
                case MediaRepeatMode.List:
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
            switch (RepeatMode)
            {
                case MediaRepeatMode.Track:
                //break;
                case MediaRepeatMode.None:
                case MediaRepeatMode.List:
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
            switch (RepeatMode)
            {
                case MediaRepeatMode.None:
                    return _playlistIndex.Value < _playlistIndex.Threshold;
                case MediaRepeatMode.Track:
                    return true;
                case MediaRepeatMode.List:
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
