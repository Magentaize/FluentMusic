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

namespace FluentMusic.Views
{
    public sealed partial class FullPlayerArtistPage : Page, IViewFor<FullPlayerArtistViewModel>
    {
        public FullPlayerArtistPage()
        {
            ViewModel = new FullPlayerArtistViewModel();

            InitializeComponent();

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

            //this.WhenActivated(d => { });
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
