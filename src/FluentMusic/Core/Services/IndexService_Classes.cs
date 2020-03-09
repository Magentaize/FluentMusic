using FluentMusic.Data;
using FluentMusic.ViewModels.Common;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using System;

namespace FluentMusic.Core.Services
{
    public sealed partial class IndexService
    {
        struct IndexTrackTransactionResult
        {
            public bool Successful { get; set; }
            public bool ArtistCreated { get; set; }
            public Artist Artist { get; set; }
            public bool AlbumCreated { get; set; }
            public Album Album { get; set; }
            public Track Track { get; set; }
        }

        struct GetFolderFilesResult
        {
            public FolderViewModel ViewModel { get; set; }

            public IEnumerable<StorageFile> Files { get; set; }
        }

        public sealed class UwpFileAbstraction : TagLib.File.IFileAbstraction
        {
            private readonly IStorageFile _file;

            private UwpFileAbstraction() { }

            public static async Task<TagLib.File.IFileAbstraction> CreateAsync(IStorageFile file)
            {
                var fAbs = new UwpFileAbstraction();
                fAbs.Name = file.Path;
                var ras = await file.OpenAsync(FileAccessMode.Read);
                fAbs.ReadStream = ras.AsStream();

                return fAbs;
            }

            public string Name { get; private set; }

            public Stream ReadStream { get; private set; }

            public Stream WriteStream { get; }

            public void CloseStream(Stream stream)
            {
                stream.Close();
            }
        }
    }
}
