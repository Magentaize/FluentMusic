using System;
using System.IO;
using Windows.Storage;
using Magentaize.FluentPlayer.Data;
using Prism.Mvvm;

namespace Magentaize.FluentPlayer.ViewModels.DataViewModel
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