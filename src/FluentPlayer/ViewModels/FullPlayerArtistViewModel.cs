using DynamicData;
using DynamicData.Binding;
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

        public ISubject<object> RestoreArtistsTapped { get; } = new Subject<object>();

        public ISubject<object> RestoreAlbumTapped { get; } = new Subject<object>();
        public ICommand ArtistListTapped { get; }
        public ICommand AlbumGridViewTapped { get; }
        public ICommand PlayTrack { get; }
        public ICommand PlayArtist => PlayTrack;
        public ICommand PlayAlbum => PlayTrack;
        public ICommand ArtistListSelectionChanged { get; }

        public ISourceList<ArtistViewModel> ArtistListSelectedItems { get; } = new SourceList<ArtistViewModel>();

        public ISourceList<AlbumViewModel> AlbumGridViewSelectedItems { get; } = new SourceList<AlbumViewModel>();

        private IObservable<bool> CreateUseSelectedItemObservable<TEvent, TViewModel>(ISubject<TEvent> restoreSubject, Action restoreAction, ISourceList<TViewModel> sourceList)
        {
            return 
                Observable.Create<bool>(observer =>
                {
                    // When (catalog)list has selected items, if cancel selection
                    // an item of (catalog)SelectedItems will be emitted,
                    // so this variable is a workaround of that.
                    var skipFirstChange = true;
                    var flag = false;

                    restoreSubject
                        .Where(_=> flag)
                        .Subscribe(_ =>
                        {
                            flag = false;
                            skipFirstChange = false;
                            observer.OnNext(false);
                            restoreAction();
                        });

                    sourceList
                        .Connect()
                        .Where(_ =>
                        {
                            var old = skipFirstChange;
                            skipFirstChange = true;
                            return old;
                        })
                        .Subscribe(_ => 
                        {
                            flag = true;
                            observer.OnNext(true);
                        });

                    return Disposable.Empty;
                })
                .StartWith(false);
        }

        private ISourceList<TDest> FlatMapViewModels<TSource, TDest>(
            IObservable<bool> useSelectedItemObservable,
            ReadOnlyObservableCollection<TSource> originalSource, 
            ISourceList<TSource> selectedSource, 
            Func<TSource, IObservableList<TDest>> selector)
        {
            var waitForFlatMapLastSubscription = Disposable.Empty;
            var waitForFlatMap = new SourceList<TSource>();

            useSelectedItemObservable
                .DistinctUntilChanged()
                .Subscribe(use =>
                {
                    waitForFlatMapLastSubscription.Dispose();
                    waitForFlatMap.Clear();

                    if (use)
                    {
                        waitForFlatMapLastSubscription =
                        selectedSource
                        .Connect()
                        .Subscribe(x => waitForFlatMap.Edit(x));
                    }
                    else
                    {
                        waitForFlatMapLastSubscription =
                        originalSource
                        .ToObservableChangeSet()
                        .Subscribe(x => waitForFlatMap.Edit(x));
                    }
                });

            var vmList = new SourceList<TDest>();
            var connectedAlbumSource = new Dictionary<TSource, IDisposable>();
            Action<TSource> connectAlbumSource = i =>
            {
                connectedAlbumSource.Add(i, selector(i).Connect().Subscribe(y =>
                {
                    vmList.Edit(y);
                }));
            };
            Action<TSource> disconnectAlbumSource = a =>
            {
                if (connectedAlbumSource.TryGetValue(a, out var d))
                {
                    d.Dispose();
                    connectedAlbumSource.Remove(a);
                    vmList.RemoveMany(selector(a).Items);
                }
            };
            waitForFlatMap
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
            vmList.Connect().Subscribe(x => { });
            return vmList;
        }

        public FullPlayerArtistViewModel()
        {
            var artistFilter = new Subject<Func<ArtistViewModel, bool>>();
            var albumFilter = new Subject<Func<AlbumViewModel, bool>>();
            var trackFilter = new Subject<Func<TrackViewModel, bool>>();

            // ---------------- Artist ----------------

            ServiceFacade.IndexService.ArtistSource
                .Connect()
                //.SubscribeOn(ThreadPoolScheduler.Instance)
                .RemoveKey()
                .Filter(artistFilter.StartWith(_ => true))
                .Bind(out var filteredArtists)
                .Subscribe();

            filteredArtists
                .ToObservableChangeSet()
                .GroupOn(x => x.Name.Substring(0, 1))
                .Transform(x => new GroupArtistViewModel(x))
                .Sort(SortExpressionComparer<GroupArtistViewModel>.Ascending(x => x.Key))
                .ObserveOnDispatcher()
                .Bind(out _artistCvsSource)
                .Subscribe();

            var useSelectedArtists = CreateUseSelectedItemObservable(
                                        RestoreArtistsTapped,
                                        () => ArtistListSelectedItem = null,
                                        ArtistListSelectedItems);

            // ---------------- Album ----------------

            var albumVm = FlatMapViewModels(
                useSelectedArtists,
                filteredArtists,
                ArtistListSelectedItems,
                x => x.Albums);

            albumVm.Connect()
                .ObserveOnDispatcher()
                .Bind(out _albumCvsSource)
                .Subscribe(x=> { }, ex => { Debugger.Break(); });

            var useSelectedAlbum = CreateUseSelectedItemObservable(
                                        RestoreAlbumTapped,
                                        () => AlbumGridViewSelectedItem = null,
                                        AlbumGridViewSelectedItems);

            // ---------------- Track ----------------

            var trackVm = FlatMapViewModels(
                useSelectedAlbum,
                _albumCvsSource,
                AlbumGridViewSelectedItems,
                x => x.Tracks);

            trackVm.Connect()
                //.SubscribeOn(ThreadPoolScheduler.Instance)
                .GroupOn(x => x.Title.Substring(0, 1))
                .Transform(x => new GroupTrackViewModel(x))
                .Sort(SortExpressionComparer<GroupTrackViewModel>.Ascending(x => x.Key))
                .ObserveOnDispatcher()
                .Bind(out _trackCvsSource)
                .Subscribe(x => { Console.WriteLine(_trackCvsSource); }, ex => { Debugger.Break(); });

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
            });
        }
        public ViewModelActivator Activator { get; }
    }
}