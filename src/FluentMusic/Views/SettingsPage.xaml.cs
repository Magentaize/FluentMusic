using Kasay.DependencyProperty;
using FluentMusic.ViewModels;
using ReactiveUI;
using Windows.UI.Xaml.Controls;
using System.Reactive.Linq;
using FluentMusic.ViewModels.Common;
using Windows.UI.Xaml.Media.Animation;
using System;
using Windows.UI.Xaml.Navigation;

namespace FluentMusic.Views
{
    public sealed partial class SettingsPage : Page
    {
        [Bind]
        internal SettingsPageViewModel ViewModel { get; set; }

        public SettingsPage()
        {
            ViewModel = new SettingsPageViewModel();
            this.InitializeComponent();

            NavigationView.Events().ItemInvoked
                    .Select(x => x.args)
    .Where(x => x.InvokedItemContainer.DataContext is NavigationViewItemViewModel)
    .Select(x => (tr: x.RecommendedNavigationTransitionInfo, ((NavigationViewItemViewModel)x.InvokedItemContainer.DataContext).PageType))
    .StartWith((new EntranceNavigationTransitionInfo(), typeof(SettingsFolderPage)))
    .ObserveOnDispatcher()
    .Subscribe(x =>
    {
        var opt = new FrameNavigationOptions() { TransitionInfoOverride = x.tr };
        NavigationContentFrame.NavigateToType(x.PageType, null, opt);
    });
        }
    }
}
