﻿using FluentMusic.ViewModels.Common;
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

        private static IStorageFolder _albumCacheFolder;

        private CacheService() { }

        internal static async Task InitializeAsync()
        {
            _albumCacheFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(AlbumCacheFolderName,
                CreationCollisionOption.OpenIfExists);
        }

        public static async Task<string> CacheAsync(IBuffer data)
        {
            var fileName = $"{Guid.NewGuid():N}.jpg";

            var ras = data.AsStream().AsRandomAccessStream();
            var decoder = await BitmapDecoder.CreateAsync(ras);

            var max = Math.Max(decoder.PixelHeight, decoder.PixelHeight);

            var file = await _albumCacheFolder.CreateFileAsync(fileName);

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

            return Path.Combine(AlbumCacheFolderName, fileName);
        }

        public async Task RemoveCacheAsync(string path)
        {
            var file = await StorageFile.GetFileFromPathAsync(path);
            await file.DeleteAsync();
        }
    }
}