using FluentMusic.ViewModels;
using FluentMusic.ViewModels.Common;
using Kasay.DependencyProperty;
using ReactiveUI;
using System;
using System.Reactive.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace FluentMusic.Views
{
    public sealed partial class FullPlayerPage : Page, IViewFor<FullPlayerPageViewModel>
    {
        [Bind]
        public FullPlayerPageViewModel ViewModel { get; set; }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (FullPlayerPageViewModel)value;
        }

        public FullPlayerPage()
        {
            ViewModel = new FullPlayerPageViewModel();
            InitializeComponent();

            NavigationView.Events().ItemInvoked
                .Select(x => x.args)
                .Where(x => x.InvokedItemContainer.DataContext is NavigationViewItemViewModel)
                .Select(x => (tr: x.RecommendedNavigationTransitionInfo, ((NavigationViewItemViewModel)x.InvokedItemContainer.DataContext).PageType))
                .StartWith((new EntranceNavigationTransitionInfo(), typeof(FullPlayerArtistPage)))
                .ObserveOnDispatcher()
                .Subscribe(x =>
                {
                    var opt = new FrameNavigationOptions() { TransitionInfoOverride = x.tr };
                    NavigationContentFrame.NavigateToType(x.PageType, null, opt);
                });


            this.WhenActivated(disposables =>
            {
            });
        }
    }
}
