using FluentMusic.ViewModels.Common;
using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;

namespace FluentMusic.Core.Services
{
    public class CacheService
    {
        private const string AlbumCacheFolderName = "AlbumCache";
        private const uint MaxPixel = 500;

        private static IStorageFolder albumCacheFolder;

        private CacheService() { }

        internal static async Task InitializeAsync()
        {
            albumCacheFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(AlbumCacheFolderName,
                CreationCollisionOption.OpenIfExists);
        }

        public static string GetCachePath(string token)
        {
            return Path.Combine(ApplicationData.Current.LocalFolder.Path, AlbumCacheFolderName, token);
        }

        public static async Task DeleteCacheAsync(string token)
        {
            var file = await StorageFile.GetFileFromPathAsync(GetCachePath(token));
            await file.DeleteAsync();
        }

        public static async Task<string> CacheAsync(IBuffer data)
        {
            var token = $"{Guid.NewGuid():N}.jpg";

            var ras = data.AsStream().AsRandomAccessStream();
            var decoder = await BitmapDecoder.CreateAsync(ras);

            var max = Math.Max(decoder.PixelHeight, decoder.PixelHeight);

            var file = await albumCacheFolder.CreateFileAsync(token);

            if (max <= MaxPixel)
            {
                await FileIO.WriteBytesAsync(file, data.ToArray());
            }
            else
            {
                var ratio = (double) MaxPixel / max;

                using (var fs = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    var encoder = await BitmapEncoder.CreateForTranscodingAsync(fs, decoder);
                    encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Linear;
                    encoder.BitmapTransform.ScaledWidth = Convert.ToUInt32(decoder.PixelWidth * ratio);
                    encoder.BitmapTransform.ScaledHeight = Convert.ToUInt32(decoder.PixelHeight * ratio);
                    await encoder.FlushAsync();
                }
            }

            return token;
        }
    }
}