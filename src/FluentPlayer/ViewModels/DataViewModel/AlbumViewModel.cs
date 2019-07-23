using DynamicData;
using Magentaize.FluentPlayer.Data;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.IO;
using System.Linq;
using Windows.Storage;

namespace Magentaize.FluentPlayer.ViewModels.DataViewModel
{
    public class AlbumViewModel : ReactiveObject
    {
        private static readonly string DefaultCoverImage = "ms-appx:///Assets/Square150x150Logo.scale-200.png";

        public long Id { get; }

        public ArtistViewModel Artist { get; set; }

        public string ArtworkPath { get; set; }

        [Reactive]
        public string Title { get; set; }

        [Reactive]
        public string AlbumCover { get; set; }

        public IObservableList<TrackViewModel> Tracks => _tracks.AsObservableList();

        [Reactive]
        public string AlbumCoverFsPath { get; set; }

        private ISourceList<TrackViewModel> _tracks;

        private AlbumViewModel(Album album)
        {
            Id = album.Id;
            ArtworkPath = album.ArtworkPath;
            Title = album.Title;
            AlbumCover = album.AlbumCover;

            if (string.IsNullOrEmpty(AlbumCover))
            {
                AlbumCoverFsPath = DefaultCoverImage;
            }
            else
            {
                AlbumCoverFsPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, AlbumCover);
            }
        }

        public AlbumViewModel AddTrack(TrackViewModel track)
        {
            track.Album = this;
            _tracks.Add(track);

            return this;
        }

        public static AlbumViewModel Create(Album album)
        {
            var vm = new AlbumViewModel(album);
            var tracks = album.Tracks
                .Select(x =>
                {
                    var v = TrackViewModel.Create(x);
                    v.Album = vm;

                    return v;
                });
            vm._tracks = SourceList.CreateFromEnumerable(tracks);

            return vm;
        }
    }
}