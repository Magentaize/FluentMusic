using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

namespace FluentMusic.Controls
{
    public class AutoHideThumbSlider : Slider
    {
        private Thumb _thumb;

        public AutoHideThumbSlider() : base()
        {
        }

        protected override void OnPointerEntered(PointerRoutedEventArgs e)
        {
            base.OnPointerEntered(e);

            VisualStateManager.GoToState(this, "PointerOver", true);
        }

        protected override void OnPointerExited(PointerRoutedEventArgs e)
        {
            base.OnPointerExited(e);

            VisualStateManager.GoToState(this, "PointerExited", true);
        }

        //protected override void OnApplyTemplate()
        //{
        //    base.OnApplyTemplate();

        //    if (Orientation == Orientation.Horizontal)
        //    {
        //        _thumb = GetTemplateChild("HorizontalThumb") as Thumb;
        //    }
        //    else
        //    {
        //        _thumb = GetTemplateChild("HorizontalTemplate") as Thumb;
        //    }
        //}
    }
}
