using System.Collections.Generic;
using System.IO;
using Windows.Storage;
using Magentaize.FluentPlayer.Data;
using Prism.Mvvm;

namespace Magentaize.FluentPlayer.ViewModels
{
    public class AlbumViewModel : BindableBase
    {
        public string Title { get; set; }

        public string AlbumCover { get; set; }

        public IList<Track> Tracks { get; set; } = new List<Track>();

        public string AlbumCoverFsPath => Path.Combine(ApplicationData.Current.LocalFolder.Path, AlbumCover);
    }
}