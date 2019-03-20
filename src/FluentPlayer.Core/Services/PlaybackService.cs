using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System.Threading;
using Magentaize.FluentPlayer.Data;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Core;
using Windows.ApplicationModel.Core;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using ReactiveUI;
using System.Diagnostics;

namespace Magentaize.FluentPlayer.Core.Services
{
    public class PlaybackService
    {
        public event EventHandler<MediaPlaybackSession> PlayerPositionChanged;
        public event EventHandler<NewTrackPlayedEventArgs> NewTrackPlayed;

        public SubjectBase<bool> IsPlaying { get; } = new Subject<bool>();
        public IObservable<MediaPlaybackSession> MediaPlaybackSession => new Subject<MediaPlaybackSession>();
        public SubjectBase<Track> CurrentTrack { get; } = new Subject<Track>(); 

        //public Track CurrentTrack { get; private set; }
        public MediaPlayer Player { get; private set; }

        private ThreadPoolTimer _positionUpdateTimer;
        private IList<Track> _trackPlaybackList;
        private MediaPlaybackItem _currentPlaybackItem;

        internal PlaybackService()
        {
        }

        private void CreatePositionUpdateTimer()
        {
            _positionUpdateTimer = ThreadPoolTimer.CreatePeriodicTimer(PositionUpdateTimer_TimerElapsedHandler, TimeSpan.FromMilliseconds(500));
        }

        private void PositionUpdateTimer_TimerElapsedHandler(ThreadPoolTimer timer)
        {
            PlayerPositionChanged?.Invoke(this, Player.PlaybackSession);
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
            //await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
            //    () =>
            //    {
            //        NewTrackPlayed?.Invoke(this, new NewTrackPlayedEventArgs()
            //        {
            //            TrackTitle = CurrentTrack.TrackTitle,
            //            TrackArtist = CurrentTrack.Artist.Name,
            //            NaturalDuration = _currentPlaybackItem.Source.Duration.Value,
            //        });
            //    });

            await WriteSmtcThumbnailAsync(_currentPlaybackItem, await CurrentTrack.LastAsync());

            CreatePositionUpdateTimer();
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
            CurrentTrack.OnNext(selected);
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