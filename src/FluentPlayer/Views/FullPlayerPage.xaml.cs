using Windows.UI.Xaml.Controls;
using Magentaize.FluentPlayer.ViewModels;

namespace Magentaize.FluentPlayer.Views
{
    public sealed partial class FullPlayerPage : Page
    {
        public FullPlayerPageViewModel Vm = new FullPlayerPageViewModel();

        public FullPlayerPage()
        {
            this.InitializeComponent();

            Loaded += FullPlayerPage_Loaded;
        }

        private async void FullPlayerPage_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            await Vm.ShowAllTracksAsync();
        }
    }
}
