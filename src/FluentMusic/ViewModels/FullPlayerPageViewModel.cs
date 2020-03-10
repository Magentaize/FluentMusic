using FluentMusic.Core;
using FluentMusic.ViewModels.Common;
using FluentMusic.Views;
using Microsoft.UI.Xaml.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Windows.Input;

namespace FluentMusic.ViewModels
{
    public class FullPlayerPageViewModel : ReactiveObject
    {
        public IList<NavigationViewItemViewModel> Navigations { get; }

        [ObservableAsProperty]
        public bool IsPlaying { get; }

        public FullPlayerPageViewModel()
        {
            Navigations = new List<NavigationViewItemViewModel>()
            {
                new NavigationViewItemViewModel { Name = "Artist", PageType = typeof(FullPlayerArtistPage) },
                new NavigationViewItemViewModel { Name = "Album", PageType = typeof(WelcomePage) },
                new NavigationViewItemViewModel { Name = "Genre", PageType = typeof(WelcomePage) },
                new NavigationViewItemViewModel { Name = "Setting", PageType = typeof(SettingsPage) },
            };

            var pbs = Service.PlaybackService;
            pbs.IsPlaying
                .ObservableOnCoreDispatcher()
                .ToPropertyEx(this, x => x.IsPlaying);
        }
    }
}