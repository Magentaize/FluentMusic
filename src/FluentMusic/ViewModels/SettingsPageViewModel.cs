using FluentMusic.ViewModels.Common;
using FluentMusic.Views;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FluentMusic.ViewModels
{
    public class SettingsPageViewModel : ReactiveObject
    {
        public IList<NavigationViewItemViewModel> Navigations { get; }

        public SettingsPageViewModel()
        {
            Navigations = new List<NavigationViewItemViewModel>()
            {
                new NavigationViewItemViewModel { Name = "Folder", PageType = typeof(SettingsFolderPage) },
                new NavigationViewItemViewModel{ Name="Interface", PageType = typeof(SettingsInterfacePage) },
            };
        }
    }
}
