using Magentaize.FluentPlayer.ViewModels;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Magentaize.FluentPlayer.Views
{
    public sealed partial class FullPlayerPage : Page
    {
        private readonly FullPlayerPageViewModel Vm = new FullPlayerPageViewModel();

        public FullPlayerPage()
        {
            InitializeComponent();

            Loaded += FullPlayerPage_Loaded;
        }

        private async void FullPlayerPage_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            await Vm.ShowAllTracksAsync();
        }

        private void UIElement_OnManipulationStarting(object sender, ManipulationStartingRoutedEventArgs e)
        {
            Vm.ProgressSlider_OnManipulationStarting((Slider)sender);
        }

        private void UIElement_OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
           Vm.ProgressSlider_OnManipulationCompleted((Slider)sender);
        }
    }
}
