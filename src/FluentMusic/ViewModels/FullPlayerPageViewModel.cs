using FluentMusic.Core;
using FluentMusic.Core.Services;
using FluentMusic.ViewModels.Common;
using FluentMusic.Views;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace FluentMusic.ViewModels
{
    public class FullPlayerPageViewModel : ReactiveObject
    {
        public IList<NavigationViewItemViewModel> Navigations { get; }
        [ObservableAsProperty]
        public bool IsIndexing { get; }

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

            IndexService.IsIndexing
                .ObserveOnCoreDispatcher()
                .ToPropertyEx(this, x => x.IsIndexing);

            PlaybackService.IsPlaying
                .ObserveOnCoreDispatcher()
                .ToPropertyEx(this, x => x.IsPlaying);
        }
    }
}