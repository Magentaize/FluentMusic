using DynamicData;
using FluentMusic.Data;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.IO;
using System.Linq;
using Windows.Storage;

namespace FluentMusic.ViewModels.Common
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

        public IObservableCache<TrackViewModel, long> Tracks => _tracks.AsObservableCache();

        [Reactive]
        public string AlbumCoverFsPath { get; set; }

        private ISourceCache<TrackViewModel, long> _tracks = new SourceCache<TrackViewModel, long>(x => x.Id);

        private AlbumViewModel(Album album)
        {
            Id = album.Id;
            ArtworkPath = album.ArtworkPath;
            Title = album.Title;
            AlbumCover = album.Cover;

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
            _tracks.AddOrUpdate(track);

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
            vm._tracks.Edit(x => x.AddOrUpdate(tracks));

            return vm;
        }
    }
}