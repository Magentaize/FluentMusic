using Kasay;
using Magentaize.FluentPlayer.ViewModels;
using ReactiveUI;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Magentaize.FluentPlayer.Views
{
    public sealed partial class FullPlayerPage : Page, IViewFor<FullPlayerPageViewModel>
    {
        public static DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", typeof(FullPlayerPageViewModel), typeof(FullPlayerPage), null);
        public FullPlayerPageViewModel ViewModel {
            get => (FullPlayerPageViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (FullPlayerPageViewModel)value;
        }

        public FullPlayerPage()
        {
            ViewModel = new FullPlayerPageViewModel();

            InitializeComponent();

            this.WhenActivated(disposables => { });
        }

        private void DragNotifySlider_OnSliderDragStarted(object sender, EventArgs e)
        {
            //Vm.ProgressSlider_OnManipulationStarting((Slider)sender);
        }

        private void DragNotifySlider_OnSliderDragCompleted(object sender, EventArgs e)
        {
            //Vm.ProgressSlider_OnManipulationCompleted((Slider)sender);
        }
    }
}
