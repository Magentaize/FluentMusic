using DynamicData;
using Magentaize.FluentPlayer.Data;
using ReactiveUI;
using System;
using System.Linq;

namespace Magentaize.FluentPlayer.ViewModels.DataViewModel
{
    public class ArtistViewModel : ReactiveObject
    {
        public Artist Artist { get; }

        public ISourceList<AlbumViewModel> AlbumViewModels { get; }

        public IObservable<IChangeSet<AlbumViewModel>> AlbumViewModelsConnector { get; } 

        public ArtistViewModel(Artist artist)
        {
            AlbumViewModels = new SourceList<AlbumViewModel>();
            AlbumViewModelsConnector = AlbumViewModels.Connect();
            Artist = artist;
            AlbumViewModels.Edit(a =>
            {
                a.AddRange(artist.Albums.Select(x => new AlbumViewModel(x)));
            });
        }
    }
}