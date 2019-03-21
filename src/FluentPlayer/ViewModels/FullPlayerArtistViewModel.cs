using DynamicData;
using DynamicData.Binding;
using Magentaize.FluentPlayer.Collections;
using Magentaize.FluentPlayer.Core;
using Magentaize.FluentPlayer.ViewModels.DataViewModel;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;

namespace Magentaize.FluentPlayer.ViewModels
{
    public sealed class FullPlayerArtistViewModel : ReactiveObject, ISupportsActivation
    {
        private ReadOnlyObservableCollection<Grouping<char, TrackViewModel>> _trackCvsSource;
        public ReadOnlyObservableCollection<Grouping<char, TrackViewModel>> TrackCvsSource => _trackCvsSource;

        private ReadOnlyObservableCollection<Grouping<char, ArtistViewModel>> _artistCvsSource;
        public ReadOnlyObservableCollection<Grouping<char, ArtistViewModel>> ArtistCvsSource => _artistCvsSource;

        private ReadOnlyObservableCollection<AlbumViewModel> _albumCvsSource;
        public ReadOnlyObservableCollection<AlbumViewModel> AlbumCvsSource => _albumCvsSource;

        [Reactive]
        public TrackViewModel TrackListSelected { get; set; }
        [Reactive]
        public ArtistViewModel ArtistListSelectedItem { get; set; }
        [Reactive]
        public AlbumViewModel AlbumGridViewSelectedItem { get; set; }

        public ReactiveCommand<Unit, Unit> RestoreArtistsCommand { get; }
        public ICommand ArtistListTapped { get; }
        public ICommand AlbumGridViewTapped { get; }
        public ICommand PlayTrack { get; }
        public ICommand PlayArtist => PlayTrack;
        public ICommand PlayAlbum => PlayTrack;

        public FullPlayerArtistViewModel()
        {
            var artistFilter = new Subject<Func<ArtistViewModel, bool>>();
            var albumFilter = new Subject<Func<AlbumViewModel, bool>>();
            var trackFilter = new Subject<Func<TrackViewModel, bool>>();

            // ---------------- Artist ----------------

            var artistList = ServiceFacade.IndexService.ArtistSource
                .Transform(x => new ArtistViewModel(x));

            artistList
                .GroupOn(x => x.Artist.Name[0])
                .Transform(x => new Grouping<char, ArtistViewModel>(x))
                .Sort(SortExpressionComparer<Grouping<char, ArtistViewModel>>.Ascending(x => x.Key))
                .Bind(out _artistCvsSource)
                .Subscribe();

            // ---------------- Album ----------------

            ViewModelAccessor.AlbumVmSource
                .ToObservableChangeSet<IObservableCollection<AlbumViewModel>, AlbumViewModel>()
                .Filter(albumFilter)
                .Bind(out var filteredAlbumVm).Subscribe();

            filteredAlbumVm
                .ToObservableChangeSet()
                .Bind(out _albumCvsSource)
                .Subscribe();

            // ---------------- Track ----------------

            ServiceFacade.IndexService.TrackSource
                .Transform(x => new TrackViewModel(x))
                .Bind(out var trackVmList)
                .Subscribe();

            trackVmList.ToObservableChangeSet()
                .Filter(trackFilter)
                .Bind(out var _filteredTrackVm)
                .Subscribe();

            _filteredTrackVm.ToObservableChangeSet()
                .GroupOn(x => x.Track.TrackTitle[0])
                .Transform(x => new Grouping<char, TrackViewModel>(x))
                .Sort(SortExpressionComparer<Grouping<char, TrackViewModel>>.Ascending(x => x.Key))
                .Bind(out _trackCvsSource)
                .Subscribe();

            ArtistListTapped = ReactiveCommand.Create<object>(_ =>
              {
                  albumFilter.OnNext(vm => vm.Album.Artist == ArtistListSelectedItem.Artist);
                  trackFilter.OnNext(vm => vm.Track.Artist == ArtistListSelectedItem.Artist);
              });

            AlbumGridViewTapped = ReactiveCommand.Create<object>(_ =>
            {
                trackFilter.OnNext(vm => vm.Track.Album == AlbumGridViewSelectedItem.Album);
            });

            PlayTrack = ReactiveCommand.Create<object>(async _ =>
            {
                var playlist = _filteredTrackVm.Select(x => x.Track);
                await ServiceFacade.PlaybackService.PlayAsync(playlist, TrackListSelected?.Track);
            });

            TrackViewModel lastPlayedTrack = null;

            ServiceFacade.PlaybackService.CurrentTrack
                .Subscribe(x =>
                {
                    if (lastPlayedTrack != null) lastPlayedTrack.IsPlaying = false;
                    var xvm = _filteredTrackVm.First(vm => vm.Track == x.Track);
                    xvm.IsPlaying = true;
                    lastPlayedTrack = xvm;
                });

            RestoreArtistsCommand = ReactiveCommand.Create(() =>
            {
                albumFilter.OnNext(_ => true);
                trackFilter.OnNext(_ => true);

                ArtistListSelectedItem = null;
                AlbumGridViewSelectedItem = null;
            });

            Activator = new ViewModelActivator();
            this.WhenActivated(async (CompositeDisposable d) =>
            {
                await RestoreArtistsCommand.Execute();
            });
        }

        public ViewModelActivator Activator { get; }
    }
}