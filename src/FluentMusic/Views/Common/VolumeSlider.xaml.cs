using FluentMusic.ViewModels.Common;
using Kasay.DependencyProperty;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FluentMusic.Views.Common
{
    public sealed partial class VolumeSlider : UserControl
    {
        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(VolumeSlider), default);

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
