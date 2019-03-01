using DryIoc;
using FluentPlayer.Data.Repositories;
using Magentaize.FluentPlayer.Views;
using Prism;
using Prism.Ioc;
using Prism.Navigation;
using Prism.Services;
using Windows.UI.Xaml;
using Magentaize.FluentPlayer.Service;
using Magentaize.FluentPlayer.ViewModels.FullPlayer;
using Magentaize.FluentPlayer.ViewModels.Setting;
using Magentaize.FluentPlayer.Views.FullPlayer;
using Prism.DryIoc;
using Windows.ApplicationModel.Core;
using Windows.UI.ViewManagement;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Microsoft.Toolkit.Uwp.Helpers;
using Prism.Logging;
using Magentaize.FluentPlayer.ViewModels;

namespace Magentaize.FluentPlayer
{
    sealed partial class App : PrismApplication
    {
        private new IPlatformNavigationService NavigationService { get; set; }

        public App()
        {
            InitializeComponent();
        }

        protected override Rules CreateContainerRules()
        {
            return Rules.Default.WithAutoConcreteTypeResolution()
                .With(Made.Of(FactoryMethod.ConstructorWithResolvableArguments))
                .WithDefaultIfAlreadyRegistered(IfAlreadyRegistered.Replace);
        }

        protected override void RegisterTypes(IContainerRegistry container)
        {
            RegisterServices(container);
            RegisterViews(container);
            InitializeServices(container);
        }

        private void RegisterServices(IContainerRegistry container)
        {
            container.RegisterSingleton<AlbumArtworkRepository>();
            container.RegisterSingleton<I18NService>();
        }

        private void RegisterViews(IContainerRegistry container)
        {
            container.RegisterForNavigation<FullPlayer>(nameof(FullPlayer));

            container.RegisterSingleton<SettingViewModel>()
                .RegisterSingleton<CollectionSettingViewModel>()
                .RegisterSingleton<BehaviorSettingViewModel>()
                .RegisterSingleton<FullPlayerViewModel>();
        }

        private void InitializeServices(IContainerRegistry container)
        {
            var ioc = container.GetContainer();
        }

        protected override void OnStart(StartArgs args)
        {
            base.OnStart(args);

            ReplaceNavigationService();

            if (args.StartKind == StartKinds.Launch)
            {
                NavigationService.NavigateAsync(nameof(FullPlayer));
            }
            else
            {
                // TODO
            }
        }

        private void ReplaceNavigationService()
        {
            var _shell = new Shell();
            var frame = _shell.FindName("ShellFrame") as Frame;
            var logger = Container.Resolve<ILoggerFacade>();
            var frameFacade = new FrameFacade(frame, logger);
            Container.GetContainer().UseInstance(typeof(IFrameFacade), frameFacade, IfAlreadyRegistered.Replace);
            var nav = new NavigationService(logger, frameFacade);
            Container.GetContainer().UseInstance(typeof(IPlatformNavigationService), nav, IfAlreadyRegistered.Replace);
            NavigationService = nav;
            _shell.DataContext = new ShellViewModel();
            Window.Current.Content = _shell;
        }
    }
}
