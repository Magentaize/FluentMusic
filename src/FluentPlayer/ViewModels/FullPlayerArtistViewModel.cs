using DynamicData;
using DynamicData.Binding;
using Magentaize.FluentPlayer.Core;
using Magentaize.FluentPlayer.ViewModels.Common;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;
using Windows.UI.Xaml.Input;
using Z.Linq;

namespace Magentaize.FluentPlayer.ViewModels
{
    public sealed class FullPlayerArtistViewModel : ReactiveObject, ISupportsActivation
    {
        public ObservableCollectionExtended<GroupArtistViewModel> ArtistCvsSource { get; } = new ObservableCollectionExtended<GroupArtistViewModel>();

        public ObservableCollectionExtended<AlbumViewModel> AlbumCvsSource { get; } = new ObservableCollectionExtended<AlbumViewModel>();

        public ObservableCollectionExtended<GroupTrackViewModel> TrackCvsSource { get; } = new ObservableCollectionExtended<GroupTrackViewModel>();

        [Reactive]
        public TrackViewModel TrackListSelected { get; set; }
        [Reactive]
        public ArtistViewModel ArtistListSelectedItem { get; set; }
        [Reactive]
        public AlbumViewModel AlbumGridViewSelectedItem { get; set; }

        public ISubject<object> RestoreArtistsTapped { get; } = new Subject<object>();
        public ISubject<object> RestoreAlbumTapped { get; } = new Subject<object>();
        public ISubject<object> ArtistListTapped { get; } = new Subject<object>();
        public ISubject<object> AlbumGridViewTapped { get; } = new Subject<object>();
        public ISubject<object> PlayArtistCommand { get; } = new Subject<object>();
        public ISubject<object> PlayAlbumCommand { get; } = new Subject<object>();
        public ISubject<object> PlayTrackCommand { get; } = new Subject<object>();
        public ICommand ArtistListSelectionChanged { get; }

        public ISourceList<ArtistViewModel> ArtistListSelectedItems { get; } = new SourceList<ArtistViewModel>();

        public ISourceList<AlbumViewModel> AlbumGridViewSelectedItems { get; } = new SourceList<AlbumViewModel>();

        private IObservable<bool> CreateUseSelectedItemObservable<TViewModel>(
            IObservable<object> restoreSubject, 
            Action restoreAction, 
            IObservableList<TViewModel> selectedList,
            ISubject<object> selectedTapped)
        {
            return 
                Observable.Create<bool>(observer =>
                {
                    IDisposable dispose = null;

                    restoreSubject
                        .Subscribe(_ =>
                        {
                            dispose?.Dispose();
                            dispose = null;

                            observer.OnNext(false);
                            restoreAction();
                        });

                    selectedTapped
                        .Where(_ => dispose == null)
                        .Subscribe(_ =>
                        {
                            dispose = selectedList
                                        .Connect()
                                        .Subscribe(_ => observer.OnNext(true));
                        });

                    return Disposable.Empty;
                })
                .StartWith(false);
        }

        private ISourceList<TDest> FlatMapViewModels<TSource, TDest>(
            IObservable<bool> useSelectedItemObservable,
            ObservableCollection<TSource> originalSource, 
            IObservableList<TSource> selectedSource, 
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

        private void InitializeReactive()
        {
            var artistFilter = new Subject<Func<ArtistViewModel, bool>>();
            var albumFilter = new Subject<Func<AlbumViewModel, bool>>();
            var trackFilter = new Subject<Func<TrackViewModel, bool>>();

            // ---------------- Artist ----------------

            var filteredArtists = new ObservableCollectionExtended<ArtistViewModel>();

            ServiceFacade.IndexService.ArtistSource
                .Connect()
                .SubscribeOnThreadPool()
                .RemoveKey()
                .Filter(artistFilter.StartWith(_ => true))
                .Bind(filteredArtists)
                .Subscribe();

            filteredArtists
                .ToObservableChangeSet()
                .SubscribeOnThreadPool()
                .GroupOn(x => x.Name.Substring(0, 1))
                .Transform(x => new GroupArtistViewModel(x))
                .Sort(SortExpressionComparer<GroupArtistViewModel>.Ascending(x => x.Key))
                .ObservableOnCoreDispatcher()
                .Bind(ArtistCvsSource)
                .Subscribe();

            var useSelectedArtists = CreateUseSelectedItemObservable(
                                        RestoreArtistsTapped,
                                        () => ArtistListSelectedItem = null,
                                        ArtistListSelectedItems,
                                        ArtistListTapped)
                                     .CacheReplay(1);
            useSelectedArtists.Connect();

            // ---------------- Album ----------------

            var albumVm = FlatMapViewModels(
                useSelectedArtists,
                filteredArtists,
                ArtistListSelectedItems,
                x => x.Albums);

            albumVm.Connect()
                .ObservableOnCoreDispatcher()
                .Bind(AlbumCvsSource)
                .Subscribe(x => { }, ex => { Debugger.Break(); });

            var restoreSubjectAlbumEx = ArtistListTapped
                                            .Where(_ => ArtistListSelectedItems.Count <= 1)
                                            .Select(_ => default(object))
                                            .Merge(RestoreAlbumTapped);

            var useSelectedAlbum = CreateUseSelectedItemObservable(
                                        restoreSubjectAlbumEx,
                                        () => AlbumGridViewSelectedItem = null,
                                        AlbumGridViewSelectedItems,
                                        AlbumGridViewTapped);

            // ---------------- Track ----------------

            var trackVm = FlatMapViewModels(
                useSelectedAlbum,
                AlbumCvsSource,
                AlbumGridViewSelectedItems,
                x => x.Tracks);

            trackVm.Connect()
                .SubscribeOnThreadPool()
                .Bind(out var ungroupTrackCvsSource)
                .GroupOn(x => x.Title.Substring(0, 1))
                .Transform(x => new GroupTrackViewModel(x))
                .Sort(SortExpressionComparer<GroupTrackViewModel>.Ascending(x => x.Key))
                .ObservableOnCoreDispatcher()
                .Bind(TrackCvsSource)
                .Subscribe(x => { }, ex => { Debugger.Break(); });

            // ---------------- Commands ----------------

            PlayArtistCommand
                .Merge(PlayAlbumCommand)
                .Merge(PlayTrackCommand)
                .Subscribe(async _ =>
                {
                    var list = ungroupTrackCvsSource;
                    await ServiceFacade.PlaybackService.PlayAsync(list);
                });

            TrackViewModel lastPlayedTrack = null;

            //ServiceFacade.PlaybackService.CurrentTrack
            //    .Subscribe(x =>
            //    {
            //        if (lastPlayedTrack != null) lastPlayedTrack.IsPlaying = false;
            //        var xvm = _filteredTrackVm.First(vm => vm.Track == x.Track);
            //        xvm.IsPlaying = true;
            //        lastPlayedTrack = xvm;
            //    });
        }

        public FullPlayerArtistViewModel()
        { 
            Activator = new ViewModelActivator();
            this.WhenActivated((CompositeDisposable d) =>
            {
                InitializeReactive();
            });
        }
        public ViewModelActivator Activator { get; }
    }
}