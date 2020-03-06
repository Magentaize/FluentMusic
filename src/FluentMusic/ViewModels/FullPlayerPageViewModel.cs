using FluentMusic.Core;
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
        public ObservableCollection<FullPlayerPageNavigationViewModel> Navigations { get; }
        [Reactive]
        public object NavigationViewSelectedItem { get; set; }

        [ObservableAsProperty]
        public bool IsPlaying { get; }

        public FullPlayerPageViewModel()
        {
            Navigations = new ObservableCollection<FullPlayerPageNavigationViewModel>()
            {
                new FullPlayerPageNavigationViewModel { Name = "Artist", PageType = typeof(SettingsPage) },
                new FullPlayerPageNavigationViewModel { Name = "Album", PageType = typeof(WelcomePage) },
                new FullPlayerPageNavigationViewModel { Name = "Genre", PageType = typeof(WelcomePage) },
                new FullPlayerPageNavigationViewModel { Name = "Setting", PageType = typeof(SettingsPage) },
            };
            NavigationViewSelectedItem = Navigations[0];

            var pbs = Service.PlaybackService;
            pbs.IsPlaying
                .ObservableOnCoreDispatcher()
                .ToPropertyEx(this, x => x.IsPlaying);
        }
    }

    public sealed class FullPlayerPageNavigationViewModel : ReactiveObject
    {
        public string Name { get; set; }

        public Type PageType { get; set; }
    }
}