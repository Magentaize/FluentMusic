using FluentMusic.ViewModels.Common;
using Kasay.DependencyProperty;
using Windows.UI.Xaml.Controls;

namespace FluentMusic.Views.Common
{
    public sealed partial class VolumeSlider : UserControl
    {
        public VolumeSlider()
        {
            InitializeComponent();
            ViewModel = new VolumeSliderViewModel();

            Slider.Events()
                .ManipulationStarted
                .Subscribe(ViewModel.SliderManipulationStarted);

            Slider.Events()
                .ManipulationCompleted
                .Subscribe(ViewModel.SliderManipulationCompleted);
        }

        [Bind]
        public VolumeSliderViewModel ViewModel { get; set; }
    }
}
