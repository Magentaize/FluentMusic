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

namespace FluentMusic.Views
{
    public sealed partial class FullPlayerArtistPage : Page, IViewFor<FullPlayerArtistViewModel>
    {
        public ObservableCollectionExtended<GroupArtistViewModel> ArtistCvsSource { get; } = new ObservableCollectionExtended<GroupArtistViewModel>();
        public ObservableCollectionExtended<GroupTrackViewModel> TrackCvsSource { get; } = new ObservableCollectionExtended<GroupTrackViewModel>();

        public FullPlayerArtistPage()
        {
            InitializeComponent();
            ViewModel = new FullPlayerArtistViewModel();

            TrackList.Events().DoubleTapped
                .InvokeCommand(ViewModel.PlayTrackCommand);

            ViewModel.ArtistSource.Connect()
                .ObservableOnThreadPool()
                .RemoveKey()
                .GroupOn(x => x.Name.Substring(0, 1))
                .Transform(x => new GroupArtistViewModel(x))
                .Sort(SortExpressionComparer<GroupArtistViewModel>.Ascending(x => x.Key))
                .ObservableOnCoreDispatcher()
                .Bind(ArtistCvsSource)
                .Subscribe();

            ViewModel.TrackSource.Connect()
                .ObservableOnThreadPool()
                .RemoveKey()
                .GroupOn(x => x.Title.Substring(0, 1))
                .Transform(x => new GroupTrackViewModel(x))
                .Sort(SortExpressionComparer<GroupTrackViewModel>.Ascending(x => x.Key))
                .DisposeMany()
                .ObservableOnCoreDispatcher()
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

            //AlbumGridView.Events().SelectionChanged
            //    .Subscribe(x =>
            //    {
            //        ViewModel.AlbumGridViewSelectedItems.Edit(a =>
            //        {
            //            a.RemoveMany(x.RemovedItems.Cast<AlbumViewModel>());
            //            a.AddRange(x.AddedItems.Cast<AlbumViewModel>());
            //        });
            //    });

            //RestoreArtistButton.Events().Tapped
            //    .Subscribe(ViewModel.RestoreArtistsTapped);

            //RestoreAlbumButton.Events().Tapped
            //    .Subscribe(ViewModel.RestoreAlbumTapped);

            //ArtistList.Events().Tapped
            //    .Subscribe(ViewModel.ArtistListTapped);

            //ArtistList.Events().DoubleTapped
            //    .Subscribe(ViewModel.PlayArtistCommand);

            //AlbumGridView.Events().Tapped
            //    .Subscribe(ViewModel.AlbumGridViewTapped);

            //AlbumGridView.Events().DoubleTapped
            //    .Subscribe(ViewModel.PlayAlbumCommand);

            //TrackList.Events().DoubleTapped
            //    .Subscribe(ViewModel.PlayTrackCommand);

            this.WhenActivated(d =>
            {
                return;
            });
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
