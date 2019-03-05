using Magentaize.FluentPlayer.Data;
using Prism.Mvvm;
using System;
using System.IO;
using Windows.Storage;

namespace Magentaize.FluentPlayer.ViewModels
{
    public class AlbumViewModel : BindableBase
    {
        public Album Album { get; }

        public Lazy<string> AlbumCoverFsPath =>
            new Lazy<string>(() => Path.Combine(ApplicationData.Current.LocalFolder.Path, Album.AlbumCover));

        public AlbumViewModel(Album album)
        {
            Album = album;
        }
    }
}