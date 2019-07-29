using Magentaize.FluentPlayer.ViewModels.DataViewModel;
using System;
using System.Collections.Generic;
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
            public TrackViewModel Track { get; internal set; }
            public MediaPlaybackItem PlaybackItem { get; internal set; }
        }

        public ISubject<bool> IsPlaying { get; } = new Subject<bool>();
        public ISubject<MediaPlaybackSession> PlaybackPosition { get; } = new Subject<MediaPlaybackSession>();
        public ISubject<TrackMixed> CurrentTrack { get; } = new Subject<TrackMixed>(); 

        public MediaPlayer Player { get; private set; }

        private IList<TrackViewModel> _trackPlaybackList;
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

            CurrentTrack
                .Subscribe(async x =>
                {
                    await WriteSmtcThumbnailAsync(x.PlaybackItem, x.Track);
                });
        }

        internal static async Task<PlaybackService> CreateAsync()
        {
            var ins = new PlaybackService();
            ins.Player = new MediaPlayer();
            ins.Player.MediaEnded += ins.Player_MediaEnded;

            return await Task.FromResult(ins);
        }

        private void Player_MediaEnded(MediaPlayer sender, object args)
        {

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

        public async Task PlayAsync(IEnumerable<TrackViewModel> tracks, TrackViewModel selected = null)
        {
            _trackPlaybackList = new List<TrackViewModel>(tracks);

            if (selected == null) selected = _trackPlaybackList[0];
            var mpi = await CreateMediaPlaybackItemAsync(selected);

            Player.Source = mpi;
            Player.Play();

            CurrentTrack.OnNext(new TrackMixed
            {
                Track = selected,
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
}