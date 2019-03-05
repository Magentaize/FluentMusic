using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace Magentaize.FluentPlayer.Controls
{
    public class DragNotifySlider : Slider
    {
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (GetTemplateChild("HorizontalThumb") is Thumb thumb)
            {
                thumb.DragStarted += ThumbOnDragStarted;
                thumb.DragCompleted += ThumbOnDragCompleted;
            }

            thumb = GetTemplateChild("VerticalThumb") as Thumb;
            if (thumb != null)
            {
                thumb.DragStarted += ThumbOnDragStarted;
                thumb.DragCompleted += ThumbOnDragCompleted;
            }
        }

        private void ThumbOnDragCompleted(object sender, DragCompletedEventArgs e)
        {
        }

        private void ThumbOnDragStarted(object sender, DragStartedEventArgs e)
        {
        }
    }
}