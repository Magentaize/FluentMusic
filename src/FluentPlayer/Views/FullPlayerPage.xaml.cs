using Magentaize.FluentPlayer.ViewModels;
using System;
using Windows.UI.Xaml.Controls;

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

        private void DragNotifySlider_OnSliderDragStarted(object sender, EventArgs e)
        {
            Vm.ProgressSlider_OnManipulationStarting((Slider)sender);
        }

        private void DragNotifySlider_OnSliderDragCompleted(object sender, EventArgs e)
        {
            Vm.ProgressSlider_OnManipulationCompleted((Slider)sender);
        }
    }
}
