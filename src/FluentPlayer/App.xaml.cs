using DryIoc;
using FluentPlayer.Data.Repositories;
using Magentaize.FluentPlayer.Views;
using Prism;
using Prism.Ioc;
using Prism.Navigation;
using Prism.Services;
using Windows.UI.Xaml;

namespace Magentaize.FluentPlayer
{
    sealed partial class App
    {
        public static INavigationService NavigationService { get; private set; }

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

        public override void RegisterTypes(IContainerRegistry container)
        {
            RegisterServices(container);

            RegisterViews(container);
        }

        private void RegisterServices(IContainerRegistry container)
        {
            container.RegisterSingleton<AlbumArtworkRepository>();
        }

        private void RegisterViews(IContainerRegistry container)
        {
            container.RegisterForNavigation<Shell>(nameof(Shell));
            container.RegisterForNavigation<FullPlayer>(nameof(FullPlayer));
        }

        public override void OnInitialized()
        {
            NavigationService = Prism.Navigation.NavigationService.Create(Gesture.Back, Gesture.Forward, Gesture.Refresh);
            NavigationService.SetAsWindowContent(Window.Current, true);
        }

        public override void OnStart(StartArgs args)
        {
            if (args.StartKind == StartKinds.Launch)
            {
                NavigationService.NavigateAsync(nameof(Shell));
            }
            else
            {
                // TODO
            }
        }
    }
}
