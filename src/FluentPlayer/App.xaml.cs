using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using DryIoc;
using FluentPlayer.Data.Repositories;
using Magentaize.FluentPlayer.Service;
using Magentaize.FluentPlayer.ViewModels;
using Magentaize.FluentPlayer.ViewModels.FullPlayer;
using Magentaize.FluentPlayer.ViewModels.Setting;
using Magentaize.FluentPlayer.Views;
using Magentaize.FluentPlayer.Views.FullPlayer;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Magentaize.FluentPlayer.Core.Extensions;

namespace Magentaize.FluentPlayer
{
    sealed partial class App : Application
    {
        private Frame _rootFrame;

        public App()
        {
            InitializeComponent();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            base.OnLaunched(args);

            Static.Container = new Container(CreateContainerRules());
            RegisterTypes(Static.Container);

            if (args.Kind == ActivationKind.Launch)
            {
                if (Window.Current.Content == null)
                {
                    CreateRootFrame();
                }

                if (_rootFrame.Content == null)
                {
                    NavigateStartupPage();
                }

                Window.Current.Activate();
            }
            else
            {
                // TODO
            }
        }

        private void NavigateStartupPage()
        {
            var contains = Static.LocalSettings.ContainsKey(Static.Settings.FirstRun);

            // TODO: disable welcome page while developing
            if (!contains)
            {
                _rootFrame.Navigate(typeof(Shell));
                _rootFrame = _rootFrame.Content.Cast<Page>().FindName("ShellFrame").Cast<Frame>();
                _rootFrame.Navigate(typeof(FullPlayerPage));
            }
            else
            {
                SeedSettings();
                _rootFrame.Navigate(typeof(WelcomePage));
            }
        }

        private void SeedSettings()
        {
            Static.LocalSettings[Static.Settings.Behavior.AutoScroll] = true;
        }

        private void CreateRootFrame()
        {
            _rootFrame = Window.Current.Content as Frame;

            if (_rootFrame == null)
            {
                _rootFrame = new Frame();

                Window.Current.Content = _rootFrame;
            }

            ExtendView();
        }

        private static Rules CreateContainerRules()
        {
            return Rules.Default.WithAutoConcreteTypeResolution()
                .With(Made.Of(FactoryMethod.ConstructorWithResolvableArguments))
                .WithDefaultIfAlreadyRegistered(IfAlreadyRegistered.Replace);
        }

        private void RegisterTypes(Container container)
        {
            RegisterServices(container);
            RegisterViews(container);
            InitializeServices(container);
        }

        private void RegisterServices(Container container)
        {
            container.RegisterSingleton<AlbumArtworkRepository>();
            container.RegisterSingleton<I18NService>();
        }

        private void RegisterViews(Container container)
        {
            //container.RegisterForNavigation<FullPlayer, FullPlayerViewModel>(nameof(FullPlayer));

            container.RegisterSingleton<SettingViewModel>()
                .RegisterSingleton<CollectionSettingViewModel>()
                .RegisterSingleton<BehaviorSettingViewModel>()
                .RegisterSingleton<FullPlayerViewModel>()
                .RegisterSingleton<ArtistsControlViewModel>();
        }

        private void InitializeServices(Container container)
        {
           
        }

        private static void ExtendView()
        {
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        }
    }
}
