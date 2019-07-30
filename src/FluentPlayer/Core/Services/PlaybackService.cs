﻿using Magentaize.FluentPlayer.ViewModels.Common;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System.Threading;
using Windows.UI.Core;

namespace Magentaize.FluentPlayer.Core.Services
{
    public class PlaybackService
    {
        public class TrackMixed
        {
            public bool IsPlayingPreviousTrack { get; internal set; }
            public TrackViewModel Track { get; internal set; }
            public MediaPlaybackItem PlaybackItem { get; internal set; }
        }

        public ISubject<bool> IsPlaying { get; } = new Subject<bool>();
        public ISubject<MediaPlaybackSession> PlaybackPosition { get; } = new Subject<MediaPlaybackSession>();
        public ISubject<TrackMixed> NewTrackPlayed { get; } = new Subject<TrackMixed>(); 

        public MediaPlayer Player { get; private set; }
        public ReplaySubject<PlaylistMode> PlaylistMode { get; private set; }

        public IList<TrackViewModel> _trackPlaybackList { get; private set; }
        private MediaPlaybackItem _currentPlaybackItem;
        private NextTrackGenerator _nextTrackGenerator;
        private ISubject<Unit> _requestPlayNext = new Subject<Unit>();
        private TrackViewModel _previousTrack;

        internal PlaybackService()
        {
        }

        public async Task<PlaybackService> InitializeAsync()
        {
            Player = new MediaPlayer();
            _nextTrackGenerator = new NextTrackGenerator(this);
            _requestPlayNext
                .Merge(Observable.FromEventPattern<TypedEventHandler<MediaPlayer, object>, object>(
                h => Player.MediaEnded += h, h => Player.MediaEnded -= h).Select(_ => Unit.Default))
                .Subscribe(async _ =>
                {
                    if (_nextTrackGenerator.HasNext())
                    {
                        await PlayAsync(_nextTrackGenerator.Next());
                    }
                });

            var _positionUpdateTimer = Observable.Timer(TimeSpan.Zero, TimeSpan.FromMilliseconds(500));
            IDisposable _positionUpdateTimerSubscription = default;
            IsPlaying
                .DistinctUntilChanged()
                .Subscribe(x =>
                {
                    if (x)
                    {
                        _positionUpdateTimerSubscription = _positionUpdateTimer.Subscribe(_ =>
                        {
                            PlaybackPosition.OnNext(Player.PlaybackSession);
                        });
                    }
                    else
                    {
                        _positionUpdateTimerSubscription.Dispose();
                    }
                });

            NewTrackPlayed
                .Do(async x=> await WriteSmtcThumbnailAsync(x.PlaybackItem, x.Track))
                .ObservableOnCoreDispatcher()
                .Subscribe(x =>
                {
                    if (_previousTrack != null) _previousTrack.IsPlaying = false;
                    x.Track.IsPlaying = true;
                    _previousTrack = x.Track;
                });

            return await Task.FromResult(this);
        }

        public void Pause()
        {
            Player.Pause();

            IsPlaying.OnNext(false);
        }

        public void Resume()
        {
            Player.Play();

            IsPlaying.OnNext(true);
        }

        public async Task PlayAsync(IList<TrackViewModel> tracks, TrackViewModel selected = null)
        {
            _trackPlaybackList = tracks;

            if (selected == null) selected = _trackPlaybackList[0];

            await PlayAsync(selected);
        }

        private async Task PlayAsync(TrackViewModel track)
        {
            var mpi = await CreateMediaPlaybackItemAsync(track);

            Player.Source = mpi;
            Player.Play();

            NewTrackPlayed.OnNext(new TrackMixed
            {
                Track = track,
                PlaybackItem = mpi,
            });
            IsPlaying.OnNext(true);
        }

        public void Seek(TimeSpan position)
        {
            if (Player.PlaybackSession.CanSeek)
            {
                Player.PlaybackSession.Position = position;
            }
        }

        private async Task<MediaPlaybackItem> CreateMediaPlaybackItemAsync(TrackViewModel track)
        {
            var file = await StorageFile.GetFileFromPathAsync(track.Path);
            var source = MediaSource.CreateFromStorageFile(file);
            await source.OpenAsync();
            var mpi = new MediaPlaybackItem(source);

            return mpi;
        }

        private async Task WriteSmtcThumbnailAsync(MediaPlaybackItem item, TrackViewModel track)
        {
            var prop = item.GetDisplayProperties();

            var album = track.Album;
            IStorageFile thumbFile;
            if (string.IsNullOrEmpty(album.AlbumCover))
            {
                thumbFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri(album.AlbumCoverFsPath));
            }
            else
            {
                thumbFile = await StorageFile.GetFileFromPathAsync(album.AlbumCoverFsPath);
            }
            var rasf = RandomAccessStreamReference.CreateFromFile(thumbFile);
            prop.Thumbnail = rasf;
            prop.Type = MediaPlaybackType.Music;
            prop.MusicProperties.Title = track.Title;
            prop.MusicProperties.Artist = track.Album.Artist.Name;
            prop.MusicProperties.AlbumTitle = track.Album.Title;

            item.ApplyDisplayProperties(prop);
        } 
    }

    public enum PlaylistMode
    {
        Normal,
        Shuffle,
        RepeatAll,
        RepeatOne,
    }
}