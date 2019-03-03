using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Magentaize.FluentPlayer.ViewModels;

namespace Magentaize.FluentPlayer.Views
{
    public sealed partial class FullPlayerPageArtistsControl : UserControl
    {
        public FullPlayerPageArtistsControlViewModel Vm { get; private set; }

        public FullPlayerPageArtistsControl()
        {
            this.InitializeComponent();

            Vm = new FullPlayerPageArtistsControlViewModel();

            Loaded += FullPlayerPageArtistsControl_Loaded;
        }

        private async void FullPlayerPageArtistsControl_Loaded(object sender, RoutedEventArgs args)
        {
            await Vm.CreateAsync();
        }
    }
}
