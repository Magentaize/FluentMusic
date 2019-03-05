using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Streams;
using Magentaize.FluentPlayer.Data;

namespace Magentaize.FluentPlayer.Core.Services
{
    public class PlaybackService
    {
        private readonly MediaPlayer _player = new MediaPlayer();

        internal PlaybackService() { }

        internal static async Task<PlaybackService> CreateAsync()
        {
            var ins = new PlaybackService();
            return await Task.FromResult(ins);
        }

        public async Task PlayAsync(Track track)
        {
            var file = await StorageFile.GetFileFromPathAsync(track.Path);
            var source = MediaSource.CreateFromStorageFile(file);
            var item = new MediaPlaybackItem(source);
            await WriteSmtcThumbnailAsync(item, track);

            _player.Source = item;
            _player.Play();
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