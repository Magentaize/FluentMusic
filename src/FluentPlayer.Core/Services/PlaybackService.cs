using Magentaize.FluentPlayer.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
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
            public Track Track { get; internal set; }
            public TimeSpan NaturalDuration { get; internal set; }
        }

        public BehaviorSubject<bool> IsPlaying { get; } = new BehaviorSubject<bool>(false);
        public SubjectBase<MediaPlaybackSession> PlaybackPosition { get; } = new Subject<MediaPlaybackSession>();
        public SubjectBase<TrackMixed> CurrentTrack { get; } = new Subject<TrackMixed>(); 

        public MediaPlayer Player { get; private set; }

        //private ThreadPoolTimer _positionUpdateTimer;
        private IList<Track> _trackPlaybackList;
        private MediaPlaybackItem _currentPlaybackItem;

        internal PlaybackService()
        {
            ThreadPoolTimer _positionUpdateTimer = null;
            IsPlaying.DistinctUntilChanged()
                .Subscribe(x =>
                {
                    if (x)
                    {
                        _positionUpdateTimer = ThreadPoolTimer.CreatePeriodicTimer(async _ =>
                        {
                            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                                () =>
                                {
                                    PlaybackPosition.OnNext(Player.PlaybackSession);
                                });
                        }, TimeSpan.FromMilliseconds(500));
                    }
                    else
                    {
                        _positionUpdateTimer?.Cancel();
                    }
                });
        }

        internal static async Task<PlaybackService> CreateAsync()
        {
            var ins = new PlaybackService();
            ins.Player = new MediaPlayer();
            ins.Player.MediaOpened += ins.Player_MediaOpened;
            ins.Player.MediaEnded += ins.Player_MediaEnded;

            return await Task.FromResult(ins);
        }

        private void Player_MediaEnded(MediaPlayer sender, object args)
        {

        }

        private async void Player_MediaOpened(MediaPlayer sender, object args)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                () =>
                {
                    IsPlaying.OnNext(true);
                });

            await WriteSmtcThumbnailAsync(_currentPlaybackItem, (await CurrentTrack.LastAsync()).Track);
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

        public async Task PlayAsync(IEnumerable<Track> tracks, Track selected)
        {
            _trackPlaybackList = new List<Track>(tracks);

            if (selected == null) selected = _trackPlaybackList[0];
            var mpi = await CreateMediaPlaybackItemAsync(selected);
            CurrentTrack.OnNext(new TrackMixed { Track = selected, NaturalDuration = mpi.Source.Duration.Value });
            _currentPlaybackItem = mpi;

            Player.Source = mpi;
            Player.Play();
        }

        public void Seek(TimeSpan position)
        {
            if (Player.PlaybackSession.CanSeek)
            {
                Player.PlaybackSession.Position = position;
            }
        }

        private async Task<MediaPlaybackItem> CreateMediaPlaybackItemAsync(Track track)
        {
            var file = await StorageFile.GetFileFromPathAsync(track.Path);
            var source = MediaSource.CreateFromStorageFile(file);
            await source.OpenAsync();
            var mpi = new MediaPlaybackItem(source);

            return mpi;
        }

        private async Task WriteSmtcThumbnailAsync(MediaPlaybackItem item, Track track)
        {
            var prop = item.GetDisplayProperties();

            var thumbF = await StorageFile.GetFileFromPathAsync(Path.Combine(ApplicationData.Current.LocalFolder.Path, track.Album.AlbumCover));
            var rasf = RandomAccessStreamReference.CreateFromFile(thumbF);
            prop.Thumbnail = rasf;
            prop.Type = MediaPlaybackType.Music;
            prop.MusicProperties.Title = track.TrackTitle;
            prop.MusicProperties.Artist = track.Artist.Name;
            prop.MusicProperties.AlbumTitle = track.Album.Title;

            item.ApplyDisplayProperties(prop);
        } 
    }
}