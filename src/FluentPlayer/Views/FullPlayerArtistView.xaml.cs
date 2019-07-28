using DynamicData;
using Kasay.DependencyProperty;
using Magentaize.FluentPlayer.Core.Extensions;
using Magentaize.FluentPlayer.ViewModels;
using Magentaize.FluentPlayer.ViewModels.DataViewModel;
using ReactiveUI;
using System;
using System.Linq;
using System.Reactive.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Magentaize.FluentPlayer.Views
{
    public sealed partial class FullPlayerArtistView : UserControl, IViewFor<FullPlayerArtistViewModel>
    {
        public FullPlayerArtistView()
        {
            ViewModel = new FullPlayerArtistViewModel();

            InitializeComponent();

            ArtistList.Events().SelectionChanged
                .Subscribe(x =>
                {
                    ViewModel.ArtistListSelectedItems.Edit(a =>
                    {
                        a.RemoveMany(x.RemovedItems.Cast<ArtistViewModel>());
                        a.AddRange(x.AddedItems.Cast<ArtistViewModel>());
                    });
                });

            AlbumGridView.Events().SelectionChanged
                .Subscribe(x =>
                {
                    ViewModel.AlbumGridViewSelectedItems.Edit(a =>
                    {
                        a.RemoveMany(x.RemovedItems.Cast<AlbumViewModel>());
                        a.AddRange(x.AddedItems.Cast<AlbumViewModel>());
                    });
                });

            RestoreArtistButton.Events().Tapped
                .Subscribe(ViewModel.RestoreArtistsTapped);

            RestoreAlbumButton.Events().Tapped
                .Subscribe(ViewModel.RestoreAlbumTapped);

            ArtistList.Events().Tapped
                .Subscribe(ViewModel.ArtistListTapped);

            ArtistList.Events().DoubleTapped
                .InvokeCommand(ViewModel.PlayArtist);
            
            //TrackList.Events().DoubleTapped
            //    .InvokeCommand(ViewModel.PlayTrack);

            AlbumGridView.Events().Tapped
                .Subscribe(ViewModel.AlbumGridViewTapped);

            AlbumGridView.Events().DoubleTapped
                .InvokeCommand(ViewModel.PlayAlbum);

            this.WhenActivated(d => { });
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
