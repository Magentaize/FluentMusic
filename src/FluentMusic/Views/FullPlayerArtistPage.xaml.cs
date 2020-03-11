using DynamicData;
using Kasay.DependencyProperty;
using FluentMusic.Core.Extensions;
using FluentMusic.ViewModels;
using FluentMusic.ViewModels.Common;
using ReactiveUI;
using System;
using System.Linq;
using System.Reactive.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Reactive.Disposables;
using DynamicData.Binding;
using System.Reactive;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Data;
using FluentMusic.Core.Services;
using System.Reactive.Subjects;
using System.Diagnostics;

namespace FluentMusic.Views
{
    public sealed partial class FullPlayerArtistPage : Page, IViewFor<FullPlayerArtistViewModel>
    {
        public ObservableCollectionExtended<GroupArtistViewModel> ArtistCvsSource { get; } = new ObservableCollectionExtended<GroupArtistViewModel>();
        public ObservableCollectionExtended<AlbumViewModel> AlbumCvsSource { get; } = new ObservableCollectionExtended<AlbumViewModel>();
        public ObservableCollectionExtended<GroupTrackViewModel> TrackCvsSource { get; } = new ObservableCollectionExtended<GroupTrackViewModel>();
        public IObservableCache<ArtistViewModel, long> ArtistSource { get; }
        public IObservableCache<AlbumViewModel, long> AlbumSource { get; }
        public IObservableCache<TrackViewModel, long> TrackSource { get; }

        private TrackStatus _trackStatus = TrackStatus.Normal;

        public FullPlayerArtistPage()
        {
            InitializeComponent();
            NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Required;
            ViewModel = new FullPlayerArtistViewModel();

            var _selectedAlbums = AlbumGridView.SelectedItemsAsObservableList<GridView, AlbumViewModel>();

            TrackList.Events().DoubleTapped
                .InvokeCommand(ViewModel.PlayTrackCommand);

            ArtistSource = IndexService.ArtistSource.Connect().ObservableOnThreadPool().AsObservableCache();
            ArtistSource.Connect()
                .ObservableOnThreadPool()
                .RemoveKey()
                .GroupOn(x => x.Name.Substring(0, 1))
                .Transform(x => new GroupArtistViewModel(x))
                .Sort(SortExpressionComparer<GroupArtistViewModel>.Ascending(x => x.Key))
                .ObserveOnCoreDispatcher()
                .Bind(ArtistCvsSource)
                .Subscribe();

            AlbumSource = IndexService.AlbumSource.Connect().ObservableOnThreadPool().AsObservableCache();
            AlbumSource.Connect()
                .ObservableOnThreadPool()
                .RemoveKey()
                .ObserveOnCoreDispatcher()
                .Bind(AlbumCvsSource)
                .Subscribe();

            var trackOrigin = IndexService.TrackSource.Connect().RemoveKey().AsObservableList();
            var trackSourceFactory = new Subject<IObservableList<TrackViewModel>>();
            trackSourceFactory.ObservableOnThreadPool()
                .Switch()
                .GroupOn(x => x.Title.Substring(0, 1))
                .Transform(x => new GroupTrackViewModel(x))
                .Sort(SortExpressionComparer<GroupTrackViewModel>.Ascending(x => x.Key))
                .DisposeMany()
                .ObserveOnCoreDispatcher()
                .Bind(TrackCvsSource)
                .Subscribe();

            //ArtistList.Events().SelectionChanged
            //    .Subscribe(x =>
            //    {
            //        ViewModel.ArtistListSelectedItems.Edit(a =>
            //        {
            //            a.RemoveMany(x.RemovedItems.Cast<ArtistViewModel>());
            //            a.AddRange(x.AddedItems.Cast<ArtistViewModel>());
            //        });
            //    });

            AlbumGridView.Events().SelectionChanged
                .Subscribe(x =>
                {
                    ViewModel.AlbumGridViewSelectedItems.Edit(a =>
                    {
                        a.RemoveMany(x.RemovedItems.Cast<AlbumViewModel>());
                        a.AddRange(x.AddedItems.Cast<AlbumViewModel>());
                    });
                });

            //RestoreArtistButton.Events().Tapped
            //    .Subscribe(ViewModel.RestoreArtistsTapped);

            //RestoreAlbumButton.Events().Tapped
            //    .Subscribe(ViewModel.RestoreAlbumTapped);

            RestoreTrackButton.Events()
                .Tapped
                .Where(_ => _trackStatus != TrackStatus.Normal)
                .Subscribe(_ =>
                {
                    _trackStatus = TrackStatus.Normal;
                    trackSourceFactory.OnNext(trackOrigin);
                });

            //ArtistList.Events().Tapped
            //    .Subscribe(ViewModel.ArtistListTapped);

            //ArtistList.Events().DoubleTapped
            //    .Subscribe(ViewModel.PlayArtistCommand);

            AlbumGridView.Events()
                .Tapped
                .Where(_ => _trackStatus == TrackStatus.Normal)
                .Do(_ => _trackStatus = TrackStatus.AlbumTapped)
                .Subscribe(_ =>
                {
                    var @new = _selectedAlbums.Connect().MergeManyEx(x => x.Tracks.Connect().RemoveKey()).AsObservableList();
                    trackSourceFactory.OnNext(@new);
                });

            //AlbumGridView.Events().DoubleTapped
            //    .Subscribe(ViewModel.PlayAlbumCommand);

            //TrackList.Events().DoubleTapped
            //    .Subscribe(ViewModel.PlayTrackCommand);

            trackSourceFactory.OnNext(trackOrigin);

            this.WhenActivated(d =>
            {
                return;
            });
        }

        enum TrackStatus
        {
            Normal,
            AlbumTapped,
        }

        [Bind]
        public FullPlayerArtistViewModel ViewModel { get; set; }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (FullPlayerArtistViewModel)value;
        }
    }
}
