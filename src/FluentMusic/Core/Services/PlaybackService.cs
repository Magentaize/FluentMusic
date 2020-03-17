using FluentMusic.ViewModels.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Streams;

namespace FluentMusic.Core.Services
{
    public class PlaybackService
    {
        public class TrackMixed
        {
            public bool IsPlayingPreviousTrack { get; internal set; }
            public TrackViewModel Track { get; internal set; }
            public MediaPlaybackItem PlaybackItem { get; internal set; }
        }

        public static ISubject<bool> IsPlaying { get; } = new Subject<bool>();
        public static ISubject<MediaPlaybackSession> PlaybackPosition { get; } = new Subject<MediaPlaybackSession>();
        public static ISubject<TrackMixed> NewTrackPlayed { get; } = new Subject<TrackMixed>();

        public ISubject<int> VolumeChanged => new ReplaySubject<int>();

        public static MediaPlayer Player { get; private set; }

        public static IList<TrackViewModel> _trackPlaybackList { get; private set; }
        private MediaPlaybackItem _currentPlaybackItem;
        private static PlaylistContainer _playlistContainer;
        private TrackViewModel _previousTrack;

        internal PlaybackService()
        {
        }

        public async Task<PlaybackService> InitializeAsync()
        {
            // Initialize Setting
            //Setting.InitializeSettingBinary(RepeatMode, nameof(RepeatMode), MediaRepeatMode.None);
            //Setting.InitializeSetting(EnableShuffle, nameof(EnableShuffle), false);
            
            Player = new MediaPlayer();
            _playlistContainer = new PlaylistContainer();

            Setting.Playback.Volume
                .Subscribe(x => Player.Volume = x / 100d);

            Observable.FromEventPattern<TypedEventHandler<MediaPlayer, object>, object>(
            h => Player.MediaEnded += h, h => Player.MediaEnded -= h)
                            .Subscribe(async _ =>
                            {
                                IsPlaying.OnNext(false);

                                if(_playlistContainer.RepeatMode == MediaRepeatMode.Track)
                                {
                                    await PlayAsyncInner(_playlistContainer.CurrentTrack);
                                    return;
                                }

                                if (_playlistContainer.HasNext())
                                {
                                    await PlayAsyncInner(_playlistContainer.Next());
                                }
                            });

            Observable.Timer(TimeSpan.Zero, TimeSpan.FromMilliseconds(500))
                .SkipUntil(IsPlaying.DistinctUntilChanged().Where(x => x))
                .TakeUntil(IsPlaying.DistinctUntilChanged().Where(x => !x))
                .Repeat()
                .Subscribe(_ =>
                {
                    PlaybackPosition.OnNext(Player.PlaybackSession);
                });

            NewTrackPlayed
                .Do(x => WriteSmtcThumbnail(x.PlaybackItem, x.Track))
                .ObserveOnCoreDispatcher()
                .Subscribe(x =>
                {
                    if (_previousTrack != null) _previousTrack.IsPlaying = false;
                    x.Track.IsPlaying = true;
                    _previousTrack = x.Track;
                });

            return await Task.FromResult(this);
        }

        public static async Task PlayAsync(IList<TrackViewModel> tracks, TrackViewModel selected = default)
        {
            _trackPlaybackList = tracks;

            if (selected != default)
            {
                _playlistContainer.Reset(tracks);
                await PlayAsyncInner(selected);
                //_requestPlayNext.OnNext(Unit.Default);
            }
        }

        public void ChangeVolume(int volume)
        {
            Player.Volume = volume;
        }

        public static void Pause()
        {
            Player.Pause();

            IsPlaying.OnNext(false);
        }

        public static void Resume()
        {
            Player.Play();

            IsPlaying.OnNext(true);
        }

        public static async Task PreviousAsync()
        {
            Pause();
            await PlayAsyncInner(_playlistContainer.Previous());
        }

        public static async Task NextAsync()
        {
            Pause();
            await PlayAsyncInner(_playlistContainer.Next());
        }

        private static async Task PlayAsyncInner(TrackViewModel track)
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

        public static void Seek(TimeSpan position)
        {
            if (Player.PlaybackSession.CanSeek)
            {
                Player.PlaybackSession.Position = position;
            }
        }

        private static async Task<MediaPlaybackItem> CreateMediaPlaybackItemAsync(TrackViewModel track)
        {
            var file = await StorageFile.GetFileFromPathAsync(track.Path);
            var source = MediaSource.CreateFromStorageFile(file);
            await source.OpenAsync();
            var mpi = new MediaPlaybackItem(source);

            return mpi;
        }

        private void WriteSmtcThumbnail(MediaPlaybackItem item, TrackViewModel track)
        {
            var album = track.Album;

            Observable
            .FromAsync(_ => StorageFile.GetFileFromPathAsync(album.CoverPath).AsTask())
            .Catch((Exception ex) => Observable.FromAsync(_ => StorageFile.GetFileFromApplicationUriAsync(new Uri(album.CoverPath)).AsTask()))
            .FirstAsync()
            .Subscribe(x =>
            {
                var prop = item.GetDisplayProperties();
                var rasf = RandomAccessStreamReference.CreateFromFile(x);
                prop.Thumbnail = rasf;
                prop.Type = MediaPlaybackType.Music;
                prop.MusicProperties.Title = track.Title;
                prop.MusicProperties.Artist = track.Album.Artist.Name;
                prop.MusicProperties.AlbumTitle = track.Album.Title;

                item.ApplyDisplayProperties(prop);
            },
            ex => { Debugger.Break(); });
        }
    }
    public enum MediaRepeatMode
    {
        None = 0,
        Track = 1,
        List = 2,
    }
}