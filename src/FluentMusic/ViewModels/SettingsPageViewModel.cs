using FluentMusic.ViewModels.Common;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.ObjectModel;

namespace FluentMusic.ViewModels
{
    public class SettingsPageViewModel : ReactiveObject
    {
        public ObservableCollection<NavigationViewItemViewModel> Navigations { get; }
        [Reactive]
        public object NavigationViewSelectedItem { get; set; }

        public SettingsPageViewModel()
        {
            Navigations = new ObservableCollection<NavigationViewItemViewModel>()
            {
                new NavigationViewItemViewModel { Name = "Folder", PageType = typeof(SettingsFolderPageViewModel) },
            };
            NavigationViewSelectedItem = Navigations[0];
        }
    }
}
