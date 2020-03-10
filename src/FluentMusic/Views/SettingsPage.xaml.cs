using FluentMusic.ViewModels;
using FluentMusic.ViewModels.Common;
using Kasay.DependencyProperty;
using ReactiveUI;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace FluentMusic.Views
{
    public sealed partial class SettingsPage : Page, IViewFor<SettingsPageViewModel>
    {
        public SettingsPage()
        {
            InitializeComponent();
            ViewModel = new SettingsPageViewModel();

            NavigationView.Events().SelectionChanged
                .Where(x => x.args.IsSettingsSelected == false)
                .Select(x => x.args)
                .Where(x => x.SelectedItem is NavigationViewItemViewModel)
                .ObserveOnDispatcher()
                .Subscribe(x =>
                {
                    var opt = new FrameNavigationOptions() { TransitionInfoOverride = x.RecommendedNavigationTransitionInfo };
                    NavigationContentFrame.NavigateToType(((NavigationViewItemViewModel)x.SelectedItem).PageType, null, opt);
                });

            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.Navigations, v => v.NavigationView.MenuItemsSource)
                    .DisposeWith(d);
                NavigationView.SelectedItem = ViewModel.Navigations[0];
            });
        }

        [Bind]
        public SettingsPageViewModel ViewModel { get; set; }
        object IViewFor.ViewModel
        { 
            get => ViewModel; 
            set => ViewModel = (SettingsPageViewModel)value; 
        }
    }
}
