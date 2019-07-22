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
        public ReadOnlyObservableCollection<GroupArtistViewModel> ArtistCvsSource => _artistCvsSource;

        private ReadOnlyObservableCollection<AlbumViewModel> _albumCvsSource;
        public IEnumerable<AlbumViewModel> AlbumCvsSource => _albumCvsSource;

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
        public ICommand ArtistListSelectionChanged { get; }

        public ISourceList<ArtistViewModel> ArtistListSelectedItems = new SourceList<ArtistViewModel>();

        public FullPlayerArtistViewModel()
        {
            //ReadOnlyObservableCollection<ArtistViewModel> ungroupedFilteredArtistList;
            var artistFilter = new Subject<Func<ArtistViewModel, bool>>();
            var albumFilter = new Subject<Func<AlbumViewModel, bool>>();
            var trackFilter = new Subject<Func<TrackViewModel, bool>>();

            // ---------------- Artist ----------------

            var ungroupedFilteredArtistListSubject = new Subject<IChangeSet<ArtistViewModel>>();

            ServiceFacade.IndexService.ArtistSource
                .Transform(x => new ArtistViewModel(x))
                .Filter(artistFilter)
                .Subscribe(ungroupedFilteredArtistListSubject.OnNext);


            ungroupedFilteredArtistListSubject.Subscribe(x =>
            {

            });

            ungroupedFilteredArtistListSubject
                .GroupOn(x => x.Artist.Name.Substring(0, 1))
                .Transform(x => new GroupArtistViewModel(x))
                .Sort(SortExpressionComparer<Grouping<ArtistViewModel>>.Ascending(x => x.Key))
                .Bind(out _artistCvsSource)
                .Subscribe();

            var unprocessedArtists = new SourceList<ArtistViewModel>();
            var usingSelectedItems = false;
            var artistFilterChangeTriggerArtistListSelectedItemsChangeWorkaround = true;

            Func<IChangeSet<ArtistViewModel>, bool> originalArtistMux = _ => !usingSelectedItems;
            Func<IChangeSet<ArtistViewModel>, bool> selectedArtistMux = _ => usingSelectedItems;
            ungroupedFilteredArtistListSubject.Where(originalArtistMux)
                .Merge(ArtistListSelectedItems.Connect().Where(selectedArtistMux))
                .Subscribe(x =>
                {
                    unprocessedArtists.Edit(a =>
                    {
                        x.ForEach(a.Edit);
                    });
                });

            ungroupedFilteredArtistListSubject.Bind(out var ungroupedFilteredArtistListCollection).Subscribe();
            artistFilter
                .Where(_ => usingSelectedItems)
                .Subscribe(x =>
                {
                    artistFilterChangeTriggerArtistListSelectedItemsChangeWorkaround = false;
                    usingSelectedItems = false;
                    unprocessedArtists.Edit(a =>
                    {
                        a.Clear();
                        a.AddRange(ungroupedFilteredArtistListCollection);
                    });
                });

            ArtistListSelectedItems.Connect()
                .Where(_ =>
                {
                    var old = artistFilterChangeTriggerArtistListSelectedItemsChangeWorkaround;
                    artistFilterChangeTriggerArtistListSelectedItemsChangeWorkaround = true;
                    return old;
                })
                .Where(_ => !usingSelectedItems)
                .Subscribe(_ =>
                {
                    usingSelectedItems = true;
                    unprocessedArtists.Edit(a =>
                    {
                        a.Clear();
                        a.AddRange(ArtistListSelectedItems.Items);
                    });
                });

            // ---------------- Album ----------------

            var albumVmList = new SourceList<AlbumViewModel>();
            var connectedAlbumSource = new Dictionary<ArtistViewModel, IDisposable>();
            unprocessedArtists.Connect().Subscribe(x =>
            {
                Action<ArtistViewModel> disconnectAlbumSource = a =>
                {
                    if (connectedAlbumSource.TryGetValue(a, out var d))
                    {
                        d.Dispose();
                        connectedAlbumSource.Remove(a);
                        albumVmList.RemoveMany(a.AlbumViewModels.Items);
                    }
                };

                Action<IExtendedList<AlbumViewModel>, ArtistViewModel> connectAlbumSource = (a, i) =>
                {
                    a.Add(i.AlbumViewModels.Items);
                    connectedAlbumSource.Add(i, i.AlbumViewModels.Connect().Subscribe(y =>
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
                .Bind(out _albumCvsSource)
                .Subscribe(x=> { Debug.WriteLine(_albumCvsSource); });

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
                  albumFilter.OnNext(vm => vm.Album.Artist == ArtistListSelectedItem.Artist);
                  trackFilter.OnNext(vm => vm.Track.Artist == ArtistListSelectedItem.Artist);
              });

            AlbumGridViewTapped = ReactiveCommand.Create<object>(_ =>
            {
                trackFilter.OnNext(vm => vm.Track.Album == AlbumGridViewSelectedItem.Album);
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

            RestoreArtistsCommand = ReactiveCommand.Create(() =>
            {
                artistFilter.OnNext(_ => true);
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