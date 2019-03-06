using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

namespace Magentaize.FluentPlayer.Controls
{
    public class DragNotifySlider : Slider
    {
        public event EventHandler SliderDragStarted;
        public event EventHandler SliderDragCompleted;

        private Thumb _thumb;

        public DragNotifySlider() : base()
        {
            ManipulationStarting += DragNotifySlider_ManipulationStarting;
            ManipulationCompleted += DragNotifySlider_ManipulationCompleted;
            Tapped += DragNotifySlider_Tapped;
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (Orientation == Orientation.Horizontal)
            {
                _thumb = GetTemplateChild("HorizontalThumb") as Thumb;
            }
            else
            {
                _thumb = GetTemplateChild("HorizontalTemplate") as Thumb;
            }
        }

        private void DragNotifySlider_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            SliderDragCompleted?.Invoke(this, null);
        }

        private void DragNotifySlider_ManipulationCompleted(object sender, Windows.UI.Xaml.Input.ManipulationCompletedRoutedEventArgs e)
        {
            SliderDragCompleted?.Invoke(this, null);
        }

        private void DragNotifySlider_ManipulationStarting(object sender, Windows.UI.Xaml.Input.ManipulationStartingRoutedEventArgs e)
        {
            SliderDragStarted?.Invoke(this, null);
        }

        protected override void OnPointerEntered(PointerRoutedEventArgs e)
        {
            base.OnPointerEntered(e);

            VisualStateManager.GoToState(_thumb, "PointerOver", true);
        }

        protected override void OnPointerExited(PointerRoutedEventArgs e)
        {
            base.OnPointerExited(e);

            VisualStateManager.GoToState(_thumb, "PointerExited", true);
        }
    }
}
