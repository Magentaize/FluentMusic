using Kasay.DependencyProperty;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;

namespace Magentaize.FluentPlayer.Controls
{
    public class SlideTile : ContentControl
    {
        private Grid _outerGrid;
        private ContentPresenter _previous;
        private ContentPresenter _current;
        private EasingFunctionBase _ease;
        private readonly TranslateTransform _newCt = new TranslateTransform();
        private readonly TranslateTransform _oldCt = new TranslateTransform();

        public SlideTile()
        {
            _ease = new SineEase
            {
                EasingMode = EasingMode.EaseOut,
            };

            SizeChanged += SlideTile_SizeChanged;
        }

        private void SlideTile_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _outerGrid.Clip = new RectangleGeometry() { Rect = new Rect(0, 0, ActualWidth, ActualHeight) };
        }

        protected override void OnApplyTemplate()
        {
            _outerGrid = (Grid)GetTemplateChild("OuterGird");
            _previous = (ContentPresenter)GetTemplateChild("Previous");
            _current = (ContentPresenter)GetTemplateChild("Current");
            if (_previous.Content == null) _previous.Content = DefaultContent;
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            SlideToNewContent(oldContent, newContent);
        }

        private DoubleAnimation CreateAnimation(double from, double to)
        {
            return new DoubleAnimation()
            {
                From = from,
                To = to,
                Duration = Duration,
                EasingFunction = _ease,
            };
        }

        private void SlideToNewContent(object oldContent, object newContent)
        {
            if (ActualWidth > 0 && ActualHeight > 0)
            {
                DoubleAnimation newAn = null;
                DoubleAnimation oldAn = null;

                _previous.RenderTransform = _oldCt;
                _current.RenderTransform = _newCt;

                switch (Direction)
                {
                    case SlideDirection.Left:
                        newAn = CreateAnimation(ActualWidth, 0);
                        oldAn = CreateAnimation(0, -ActualWidth);
                        Storyboard.SetTargetProperty(newAn, nameof(TranslateTransform.X));
                        Storyboard.SetTargetProperty(oldAn, nameof(TranslateTransform.X));
                        break;
                    case SlideDirection.Right:
                        newAn = CreateAnimation(-ActualWidth, 0);
                        oldAn = CreateAnimation(0, ActualWidth);
                        Storyboard.SetTargetProperty(newAn, nameof(TranslateTransform.X));
                        Storyboard.SetTargetProperty(oldAn, nameof(TranslateTransform.X));
                        break;
                    case SlideDirection.Down:
                        newAn = CreateAnimation(-ActualHeight, 0);
                        oldAn = CreateAnimation(0, ActualHeight);
                        Storyboard.SetTargetProperty(newAn, nameof(TranslateTransform.Y));
                        Storyboard.SetTargetProperty(oldAn, nameof(TranslateTransform.Y));
                        break;
                    case SlideDirection.Up:
                        newAn = CreateAnimation(ActualHeight, 0);
                        oldAn = CreateAnimation(0, -ActualHeight);
                        Storyboard.SetTargetProperty(newAn, nameof(TranslateTransform.Y));
                        Storyboard.SetTargetProperty(oldAn, nameof(TranslateTransform.Y));
                        break;
                }

                oldAn.Completed += (_, __) =>
                {
                    _previous.Content = newContent;
                };
                Storyboard.SetTarget(newAn, _newCt);
                Storyboard.SetTarget(oldAn, _oldCt);

                var sb = new Storyboard();
                sb.Children.Add(newAn);
                sb.Children.Add(oldAn);

                _current.Content = newContent;

                sb.Begin();
            }
        }

        [Bind]
        public SlideDirection Direction { get; set; }
        [Bind]
        public Duration Duration { get; set; }
        [Bind]
        public object DefaultContent { get; set; }

        public enum SlideDirection
        {
            Left = 1,
            Right = 2,
            Up = 3,
            Down = 4
        }
    }
}
