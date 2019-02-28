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

namespace Magentaize.FluentPlayer
{
    sealed partial class App : PrismApplication
    {
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

            container.RegisterSingleton<SettingViewModel>();
            container.RegisterSingleton<CollectionSettingViewModel>();
            container.RegisterSingleton<BehaviorSettingViewModel>();

            container.RegisterSingleton<FullPlayerViewModel>();
        }

        private void InitializeServices(IContainerRegistry container)
        {
            var ioc = container.GetContainer();
        }

        protected override void OnStart(StartArgs args)
        {
            base.OnStart(args);

            // extend title bar
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = false;

            if (args.StartKind == StartKinds.Launch)
            {
                NavigationService.NavigateAsync(nameof(FullPlayer));
            }
            else
            {
                // TODO
            }
        }
    }
}
