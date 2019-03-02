using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace TagLib
{
    public sealed class UwpFileAbstraction : File.IFileAbstraction
    {
        private readonly IStorageFile _file;

        private UwpFileAbstraction() {}

        public static async Task<File.IFileAbstraction> CreateAsync(IStorageFile file)
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