﻿using FluentMusic.Core;
using FluentMusic.ViewModels.Common;
using FluentMusic.Views;
using Microsoft.UI.Xaml.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Windows.Input;

namespace FluentMusic.ViewModels
{
    public class FullPlayerPageViewModel : ReactiveObject
    {
        public ObservableCollection<NavigationViewItemViewModel> Navigations { get; }
        [Reactive]
        public object NavigationViewSelectedItem { get; set; }

        [ObservableAsProperty]
        public bool IsPlaying { get; }

        public FullPlayerPageViewModel()
        {
            Navigations = new ObservableCollection<NavigationViewItemViewModel>()
            {
                new NavigationViewItemViewModel { Name = "Artist", PageType = typeof(FullPlayerArtistPage) },
                new NavigationViewItemViewModel { Name = "Album", PageType = typeof(WelcomePage) },
                new NavigationViewItemViewModel { Name = "Genre", PageType = typeof(WelcomePage) },
                new NavigationViewItemViewModel { Name = "Setting", PageType = typeof(SettingsPage) },
            };
            NavigationViewSelectedItem = Navigations[0];

            var pbs = Service.PlaybackService;
            pbs.IsPlaying
                .ObservableOnCoreDispatcher()
                .ToPropertyEx(this, x => x.IsPlaying);
        }
    }
}