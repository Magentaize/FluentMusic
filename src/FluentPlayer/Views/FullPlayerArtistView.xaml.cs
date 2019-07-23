﻿using DynamicData;
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

            Observable.FromEventPattern<SelectionChangedEventHandler, SelectionChangedEventArgs>
                (x => ArtistList.SelectionChanged += x, x => ArtistList.SelectionChanged -= x)
                .Subscribe(x =>
                {
                    ViewModel.ArtistListSelectedItems.Edit(a =>
                    {
                        a.RemoveMany(x.EventArgs.RemovedItems.Cast<ArtistViewModel>());
                        a.AddRange(x.EventArgs.AddedItems.Cast<ArtistViewModel>());
                    });
                });
    
            ArtistList.Events().Tapped
                .InvokeCommand(ViewModel.ArtistListTapped);

            ArtistList.Events().DoubleTapped
                .InvokeCommand(ViewModel.PlayArtist);
            
            //TrackList.Events().DoubleTapped
            //    .InvokeCommand(ViewModel.PlayTrack);

            AlbumGridView.Events().Tapped
                .InvokeCommand(ViewModel.AlbumGridViewTapped);

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
