using DynamicData;
using DynamicData.Binding;
using Magentaize.FluentPlayer.Collections;
using Magentaize.FluentPlayer.Core;
using Magentaize.FluentPlayer.Data;
using Magentaize.FluentPlayer.ViewModels.DataViewModel;
using Microsoft.Toolkit.Uwp.UI.Controls.TextToolbarSymbols;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;

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

        public ICommand RestoreArtistsCommand { get; }
        public ICommand RestoreAlbumCommand { get; }
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

            var filteredArtists = 
                ServiceFacade.IndexService.ArtistSource
                .Connect()
                .RemoveKey()
                .Filter(Observable.Return<Func<ArtistViewModel, bool>>(_ => true).Concat(artistFilter))
                .ToSourceList()
                .Connect();

            filteredArtists
                .GroupOn(x => x.Name.Substring(0, 1))
                .Transform(x => new GroupArtistViewModel(x))
                .Sort(SortExpressionComparer<Grouping<ArtistViewModel>>.Ascending(x => x.Key))
                .Bind(out _artistCvsSource)
                .Subscribe();

            var artistsWaitForFlatMapAlbum = new SourceList<ArtistViewModel>();

            IDisposable artistsWaitForFlatMapAlbumLastSubscription = Disposable.Empty;
            var useSelectedArtists = new Subject<bool>();
            useSelectedArtists.Merge(Observable.Return(false))
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
                        .Subscribe(x => artistsWaitForFlatMapAlbum.Edit(x));
                    }
                });

            var artistFilterChangeTriggerArtistListSelectedItemsChangeWorkaround = true;

            RestoreArtistsCommand = ReactiveCommand.Create(() =>
            {
                artistFilterChangeTriggerArtistListSelectedItemsChangeWorkaround = false;
                useSelectedArtists.OnNext(false);
                //albumFilter.OnNext(_ => true);
                //trackFilter.OnNext(_ => true);

                ArtistListSelectedItem = null;
                //AlbumGridViewSelectedItem = null;
            });

            ArtistListSelectedItems.Connect()
                .Where(_ =>
                {
                    var old = artistFilterChangeTriggerArtistListSelectedItemsChangeWorkaround;
                    artistFilterChangeTriggerArtistListSelectedItemsChangeWorkaround = true;
                    return old;
                })
                .Subscribe(_ =>
                {
                    useSelectedArtists.OnNext(true);
                });

            // ---------------- Album ----------------

            RestoreAlbumCommand = ReactiveCommand.Create(() =>
            {
                AlbumGridViewSelectedItem = null;
            });

            var albumVmList = new SourceList<AlbumViewModel>();
            var connectedAlbumSource = new Dictionary<ArtistViewModel, IDisposable>();

            Action<IExtendedList<AlbumViewModel>, ArtistViewModel> connectAlbumSource = (a, i) =>
            {
                a.Add(i.Albums.Items);
                connectedAlbumSource.Add(i, i.Albums.Connect().Subscribe(y =>
                {
                    y.ForEach(albumChange =>
                    {
                        switch (albumChange.Reason)
                        {
                            case ListChangeReason.Add:
                                albumVmList.Add(albumChange.Item.Current);
                                break;
                            case ListChangeReason.Remove:
                                albumVmList.Remove(albumChange.Item.Current);
                                break;
                        }
                    });
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
                    albumVmList.Edit(a =>
                    {
                        x.ForEach(change =>
                        {
                            switch (change.Reason)
                            {
                                case ListChangeReason.Add:
                                    connectAlbumSource(a, change.Item.Current);
                                    break;
                                case ListChangeReason.AddRange:
                                    change.Range.ForEach(i =>
                                    {
                                        connectAlbumSource(a, i);
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
                });

            albumVmList.Connect()
                .ObserveOnDispatcher()
                .Bind(out _albumCvsSource)
                .Subscribe(x => { }, ex => { Debugger.Break(); });

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