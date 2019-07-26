using DynamicData;
using DynamicData.Binding;
using Magentaize.FluentPlayer.Collections;
using Magentaize.FluentPlayer.Core;
using Magentaize.FluentPlayer.ViewModels.DataViewModel;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;
using Windows.UI.Xaml.Input;

namespace Magentaize.FluentPlayer.ViewModels
{
    public sealed class FullPlayerArtistViewModel : ReactiveObject, ISupportsActivation
    {
        private ReadOnlyObservableCollection<GroupArtistViewModel> _artistCvsSource;
        public IEnumerable<GroupArtistViewModel> ArtistCvsSource => _artistCvsSource;

        private ReadOnlyObservableCollection<AlbumViewModel> _albumCvsSource;
        public IEnumerable<AlbumViewModel> AlbumCvsSource => _albumCvsSource;

        private ReadOnlyObservableCollection<GroupTrackViewModel> _trackCvsSource;

        public IEnumerable<GroupTrackViewModel> TrackCvsSource => _trackCvsSource;

        [Reactive]
        public TrackViewModel TrackListSelected { get; set; }
        [Reactive]
        public ArtistViewModel ArtistListSelectedItem { get; set; }
        [Reactive]
        public AlbumViewModel AlbumGridViewSelectedItem { get; set; }

        public ISubject<TappedRoutedEventArgs> RestoreArtistsTapped { get; } = new Subject<TappedRoutedEventArgs>();

        public ISubject<TappedRoutedEventArgs> RestoreAlbumTapped { get; } = new Subject<TappedRoutedEventArgs>();
        public ICommand ArtistListTapped { get; }
        public ICommand AlbumGridViewTapped { get; }
        public ICommand PlayTrack { get; }
        public ICommand PlayArtist => PlayTrack;
        public ICommand PlayAlbum => PlayTrack;
        public ICommand ArtistListSelectionChanged { get; }

        public ISourceList<ArtistViewModel> ArtistListSelectedItems = new SourceList<ArtistViewModel>();

        public FullPlayerArtistViewModel()
        {
            var artistFilter = new Subject<Func<ArtistViewModel, bool>>();
            var albumFilter = new Subject<Func<AlbumViewModel, bool>>();
            var trackFilter = new Subject<Func<TrackViewModel, bool>>();

            // ---------------- Artist ----------------

            ServiceFacade.IndexService.ArtistSource
                .Connect()
                .RemoveKey()
                .Filter(artistFilter.StartWith(_ => true))
                .Bind(out var filteredArtists)
                .Subscribe();

            filteredArtists
                .ToObservableChangeSet()
                .GroupOn(x => x.Name.Substring(0, 1))
                .Transform(x => new GroupArtistViewModel(x))
                .Sort(SortExpressionComparer<Grouping<ArtistViewModel>>.Ascending(x => x.Key))
                .Bind(out _artistCvsSource)
                .Subscribe(); 

            var useSelectedArtists = Observable.Create<bool>(observer =>
            {
                // When artlist list has selected items, if cancel selection
                // an item of ArtistListSelectedItems will be emitted,
                // so this variable is a workaround of that.
                var skipFirstChange = true;

                RestoreArtistsTapped.Subscribe(_ =>
                {
                    skipFirstChange = false;
                    observer.OnNext(false);

                    ArtistListSelectedItem = null;
                });

                ArtistListSelectedItems
                    .Connect()
                    .Where(_ =>
                    {
                        var old = skipFirstChange;
                        skipFirstChange = true;
                        return old;
                    })
                    .Subscribe(_ =>
                    {
                        observer.OnNext(true);
                    });

                return Disposable.Empty;
            });

            // ---------------- Album ----------------

            var artistsWaitForFlatMapAlbumLastSubscription = Disposable.Empty;
            var artistsWaitForFlatMapAlbum = new SourceList<ArtistViewModel>();
            useSelectedArtists
                .StartWith(false)
                .DistinctUntilChanged()
                .Subscribe(use =>
                {
                    artistsWaitForFlatMapAlbumLastSubscription.Dispose();
                    artistsWaitForFlatMapAlbum.Clear();

                    if (use)
                    {
                        artistsWaitForFlatMapAlbumLastSubscription =
                        ArtistListSelectedItems
                        .Connect()
                        .Subscribe(x => artistsWaitForFlatMapAlbum.Edit(x));
                    }
                    else
                    {
                        artistsWaitForFlatMapAlbumLastSubscription =
                        filteredArtists
                        .ToObservableChangeSet()
                        .Subscribe(x => artistsWaitForFlatMapAlbum.Edit(x));
                    }
                });

            var albumVmList = new SourceList<AlbumViewModel>();
            var connectedAlbumSource = new Dictionary<ArtistViewModel, IDisposable>();
            Action<ArtistViewModel> connectAlbumSource = i =>
            {
                connectedAlbumSource.Add(i, i.Albums.Connect().Subscribe(y =>
                {
                    albumVmList.Edit(y);
                }));
            };
            Action<ArtistViewModel> disconnectAlbumSource = a =>
            {
                if (connectedAlbumSource.TryGetValue(a, out var d))
                {
                    d.Dispose();
                    connectedAlbumSource.Remove(a);
                    albumVmList.RemoveMany(a.Albums.Items);
                }
            };          
            artistsWaitForFlatMapAlbum
                .Connect()
                .Subscribe(x =>
                {
                    x.ForEach(change =>
                    {
                        switch (change.Reason)
                        {
                            case ListChangeReason.Add:
                                connectAlbumSource(change.Item.Current);
                                break;
                            case ListChangeReason.AddRange:
                                change.Range.ForEach(i =>
                                {
                                    connectAlbumSource(i);
                                });
                                break;
                            case ListChangeReason.Remove:
                                disconnectAlbumSource(change.Item.Current);
                                break;
                            case ListChangeReason.Clear:
                            case ListChangeReason.RemoveRange:
                                change.Range.ForEach(disconnectAlbumSource);
                                break;
                        }
                    });
                });

            albumVmList.Connect()
                .ObserveOnDispatcher()
                .Bind(out _albumCvsSource)
                .Subscribe(x => { }, ex => { Debugger.Break(); });

            var useSelectedAlbum = Observable.Create<bool>(observer =>
            {
                AlbumGridViewSelectedItem = null;

                return Disposable.Empty;
            });

            // ---------------- Track ----------------

            //ServiceFacade.IndexService.TrackSource
            //    .Transform(x => new TrackViewModel(x))
            //    .Bind(out var trackVmList)
            //    .Subscribe();

            //trackVmList.ToObservableChangeSet()
            //    .Filter(trackFilter)
            //    .Bind(out var _filteredTrackVm)
            //    .Subscribe();

            //_filteredTrackVm.ToObservableChangeSet()
            //    .GroupOn(x => x.Track.TrackTitle[0])
            //    .Transform(x => new Grouping<char, TrackViewModel>(x))
            //    .Sort(SortExpressionComparer<Grouping<char, TrackViewModel>>.Ascending(x => x.Key))
            //    .Bind(out _trackCvsSource)
            //    .Subscribe();

            ArtistListTapped = ReactiveCommand.Create<object>(_ =>
              {
                  //albumFilter.OnNext(vm => vm.Album.Artist == ArtistListSelectedItem.Artist);
                  //trackFilter.OnNext(vm => vm.Track.Artist == ArtistListSelectedItem.Artist);
              });

            AlbumGridViewTapped = ReactiveCommand.Create<object>(_ =>
            {
                //trackFilter.OnNext(vm => vm.Track.Album == AlbumGridViewSelectedItem.Album);
            });

            //PlayTrack = ReactiveCommand.Create<object>(async _ =>
            //{
            //    var playlist = _filteredTrackVm.Select(x => x.Track);
            //    await ServiceFacade.PlaybackService.PlayAsync(playlist, TrackListSelected?.Track);
            //});

            TrackViewModel lastPlayedTrack = null;

            //ServiceFacade.PlaybackService.CurrentTrack
            //    .Subscribe(x =>
            //    {
            //        if (lastPlayedTrack != null) lastPlayedTrack.IsPlaying = false;
            //        var xvm = _filteredTrackVm.First(vm => vm.Track == x.Track);
            //        xvm.IsPlaying = true;
            //        lastPlayedTrack = xvm;
            //    });

            Activator = new ViewModelActivator();
            this.WhenActivated(async (CompositeDisposable d) =>
            {
                //await RestoreArtistsCommand.Execute();
            });
        }
        public ViewModelActivator Activator { get; }
    }
}