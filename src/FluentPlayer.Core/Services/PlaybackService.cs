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

namespace Magentaize.FluentPlayer.Core.Services
{
    public class PlaybackService
    {
        public event EventHandler<MediaPlaybackSession> PlayerPositionChanged;
        public event EventHandler<NewTrackPlayedEventArgs> NewTrackPlayed;

        public MediaPlayer Player { get; } = new MediaPlayer();

        private ThreadPoolTimer _positionUpdateTimer;

        internal PlaybackService() { }

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
            return await Task.FromResult(ins);
        }

        public async Task PlayAsync(Track track)
        {
            var file = await StorageFile.GetFileFromPathAsync(track.Path);
            var source = MediaSource.CreateFromStorageFile(file);
            await source.OpenAsync();
            var mpi = new MediaPlaybackItem(source);
            await WriteSmtcThumbnailAsync(mpi, track);

            Player.Source = mpi;
            Player.Play();

            NewTrackPlayed?.Invoke(this, new NewTrackPlayedEventArgs()
            {
                TrackTitle = track.TrackTitle,
                TrackArtist = track.Artist.Name,
                NaturalDuration = source.Duration.Value,
            });

            CreatePositionUpdateTimer();
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