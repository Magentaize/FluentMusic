using FluentMusic.ViewModels.Common;
using Kasay.DependencyProperty;
using System;
using System.Reactive.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace FluentMusic.Views.Common
{
    public sealed partial class VolumeSlider : UserControl
    {
        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(double), typeof(VolumeSlider), default);

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

            Observable.FromEventPattern<RangeBaseValueChangedEventHandler, RangeBaseValueChangedEventArgs>(
                h => Slider.ValueChanged += h, h => Slider.ValueChanged -= h)
                .Subscribe(x => Value = x.EventArgs.NewValue);
        }

        [Bind]
        public VolumeSliderViewModel ViewModel { get; set; }
    }
}
