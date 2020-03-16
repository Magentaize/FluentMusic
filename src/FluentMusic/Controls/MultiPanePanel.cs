using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Windows.Devices.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace FluentMusic.Controls
{
    [TemplatePart(Name = "PART_LeftPane", Type = typeof(Border))]
    public sealed class MultiPanePanel : Control
    {
        public event RoutedEventHandler WidthPercentChanged;

        private Border leftPane;
        private Border middlePane;
        private Border rightPane;
        private ContentPresenter leftPaneContent;
        private ContentPresenter middlePaneContent;
        private ContentPresenter rightPaneContent;

        private Border separatorLeft;
        private bool isSeparatorLeftMouseButtonDown;

        private Border separatorRight;
        private bool isSeparatorRightMouseButtonDown;

        private ISubject<Unit> resizeSubject = new Subject<Unit>();

        public bool IsRightPaneVisible
        {
            get { return (bool)GetValue(IsRightPaneVisibleProperty); }
            set { SetValue(IsRightPaneVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsRightPaneVisibleProperty =
            DependencyProperty.Register(nameof(IsRightPaneVisible), typeof(bool), typeof(MultiPanePanel), new PropertyMetadata(true));

        public bool CanResize
        {
            get { return (bool)GetValue(CanResizeProperty); }
            set { SetValue(CanResizeProperty, value); }
        }

        public static readonly DependencyProperty CanResizeProperty =
            DependencyProperty.Register(nameof(CanResize), typeof(bool), typeof(MultiPanePanel), new PropertyMetadata(true));

        public int SeparatorMarginTop
        {
            get { return (int)GetValue(SeparatorMarginTopProperty); }
            set { SetValue(SeparatorMarginTopProperty, value); }
        }

        public static readonly DependencyProperty SeparatorMarginTopProperty =
           DependencyProperty.Register(nameof(SeparatorMarginTop), typeof(int), typeof(MultiPanePanel), new PropertyMetadata(0));

        public int SeparatorMarginBottom
        {
            get { return (int)GetValue(SeparatorMarginBottomProperty); }
            set { SetValue(SeparatorMarginBottomProperty, value); }
        }

        public static readonly DependencyProperty SeparatorMarginBottomProperty =
            DependencyProperty.Register(nameof(SeparatorMarginBottom), typeof(int), typeof(MultiPanePanel), new PropertyMetadata(0));

        public int ContentResizeDelay
        {
            get { return (int)GetValue(ContentResizeDelayProperty); }
            set { SetValue(ContentResizeDelayProperty, value); }
        }

        public static readonly DependencyProperty ContentResizeDelayProperty =
           DependencyProperty.Register(nameof(ContentResizeDelay), typeof(int), typeof(MultiPanePanel), new PropertyMetadata(0));

        public double ResizeGripWidth
        {
            get { return (double)GetValue(ResizeGripWidthProperty); }
            set { SetValue(ResizeGripWidthProperty, value); }
        }

        public static readonly DependencyProperty ResizeGripWidthProperty =
            DependencyProperty.Register(nameof(ResizeGripWidth), typeof(double), typeof(MultiPanePanel), new PropertyMetadata(5.0));

        public double LeftPaneWidthPercent
        {
            get { return (double)GetValue(LeftPaneWidthPercentProperty); }
            set { SetValue(LeftPaneWidthPercentProperty, value); }
        }

        public static readonly DependencyProperty LeftPaneWidthPercentProperty =
            DependencyProperty.Register(nameof(LeftPaneWidthPercent), typeof(double), typeof(MultiPanePanel), new PropertyMetadata(33.0));

        public double LeftPaneMinimumWidth
        {
            get { return (double)GetValue(LeftPaneMinimumWidthProperty); }
            set { SetValue(LeftPaneMinimumWidthProperty, value); }
        }

        public static readonly DependencyProperty LeftPaneMinimumWidthProperty =
            DependencyProperty.Register(nameof(LeftPaneMinimumWidth), typeof(double), typeof(MultiPanePanel), new PropertyMetadata(0.0));

        public double RightPaneWidthPercent
        {
            get { return (double)GetValue(RightPaneWidthPercentProperty); }
            set { SetValue(RightPaneWidthPercentProperty, value); }
        }

        public static readonly DependencyProperty RightPaneWidthPercentProperty =
           DependencyProperty.Register(nameof(RightPaneWidthPercent), typeof(double), typeof(MultiPanePanel), new PropertyMetadata(33.0));

        public double RightPaneMinimumWidth
        {
            get { return (double)GetValue(RightPaneMinimumWidthProperty); }
            set { SetValue(RightPaneMinimumWidthProperty, value); }
        }

        public static readonly DependencyProperty RightPaneMinimumWidthProperty =
           DependencyProperty.Register(nameof(RightPaneMinimumWidth), typeof(double), typeof(MultiPanePanel), new PropertyMetadata(0.0));

        public double MiddlePaneMinimumWidth
        {
            get { return (double)GetValue(MiddlePaneMinimumWidthProperty); }
            set { SetValue(MiddlePaneMinimumWidthProperty, value); }
        }

        public static readonly DependencyProperty MiddlePaneMinimumWidthProperty =
        DependencyProperty.Register(nameof(MiddlePaneMinimumWidth), typeof(double), typeof(MultiPanePanel), new PropertyMetadata(0.0));

        public object LeftPaneContent
        {
            get { return GetValue(LeftPaneContentProperty); }
            set { SetValue(LeftPaneContentProperty, value); }
        }

        public static readonly DependencyProperty LeftPaneContentProperty =
           DependencyProperty.Register(nameof(LeftPaneContent), typeof(object), typeof(MultiPanePanel), new PropertyMetadata(null));

        public object MiddlePaneContent
        {
            get { return GetValue(MiddlePaneContentProperty); }
            set { SetValue(MiddlePaneContentProperty, value); }
        }

        public static readonly DependencyProperty MiddlePaneContentProperty =
           DependencyProperty.Register(nameof(MiddlePaneContent), typeof(object), typeof(MultiPanePanel), new PropertyMetadata(null));

        public object RightPaneContent
        {
            get { return GetValue(RightPaneContentProperty); }
            set { SetValue(RightPaneContentProperty, value); }
        }

        public static readonly DependencyProperty RightPaneContentProperty =
           DependencyProperty.Register(nameof(RightPaneContent), typeof(object), typeof(MultiPanePanel), new PropertyMetadata(null));

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            leftPane = (Border)GetTemplateChild("PART_LeftPane");
            middlePane = (Border)GetTemplateChild("PART_MiddlePane");
            rightPane = (Border)GetTemplateChild("PART_RightPane");

            leftPaneContent = (ContentPresenter)GetTemplateChild("PART_LeftPaneContent");
            middlePaneContent = (ContentPresenter)GetTemplateChild("PART_MiddlePaneContent");
            rightPaneContent = (ContentPresenter)GetTemplateChild("PART_RightPaneContent");

            separatorLeft = (Border)GetTemplateChild("PART_SeparatorLeft");
            separatorRight = (Border)GetTemplateChild("PART_SeparatorRight");

            separatorLeft.Margin = new Thickness(0, SeparatorMarginTop, 0, SeparatorMarginBottom);
            separatorRight.Margin = new Thickness(0, SeparatorMarginTop, 0, SeparatorMarginBottom);

            SizeChanged += MultiPanePanel_SizeChanged;

            if (separatorLeft != null)
            {
                separatorLeft.PointerPressed += SeparatorLeft_PointerPressed;
                separatorLeft.PointerReleased += SeparatorLeft_PointerReleased;
                separatorLeft.PointerMoved += MultiPanePanel_PointerMoved;
            }

            if (separatorRight != null)
            {
                separatorRight.PointerPressed += SeparatorRight_PointerPressed;
                separatorRight.PointerReleased += SeparatorRight_PointerReleased;
                separatorRight.PointerMoved += MultiPanePanel_PointerMoved;
            }

            if (ContentResizeDelay <= 0)
            {
                ContentResizeDelay = 1;
            }

            resizeSubject
                .Throttle(TimeSpan.FromMilliseconds(ContentResizeDelay))
                .ObserveOnCoreDispatcher()
                .Subscribe(_ => ResizeContent());
        }

        private void SeparatorRight_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (IsRightPaneVisible) isSeparatorRightMouseButtonDown = false;
        }

        private void SeparatorLeft_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            isSeparatorLeftMouseButtonDown = false;
        }

        private void SeparatorRight_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
            {
                var properties = e.GetCurrentPoint(this).Properties;
                if (properties.IsLeftButtonPressed)
                {
                    if (IsRightPaneVisible)
                    {
                        ((UIElement)sender).CapturePointer(e.Pointer);
                        isSeparatorRightMouseButtonDown = true;
                    }
                }
            }
        }

        private void SeparatorLeft_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
            {
                var properties = e.GetCurrentPoint(this).Properties;
                if (properties.IsLeftButtonPressed)
                {
                    ((UIElement)sender).CapturePointer(e.Pointer);
                    isSeparatorLeftMouseButtonDown = true;
                }
            }
        }

        private double GetTotalWidth()
        {
            return IsRightPaneVisible ? ActualWidth - ResizeGripWidth * 2 : ActualWidth - ResizeGripWidth;
        }

        private void ApplyPercentages()
        {
            if (ActualWidth > 0 && LeftPaneWidthPercent > 0 && RightPaneWidthPercent > 0)
            {
                double totalWidth = GetTotalWidth();

                double newLeftPaneWidth = Convert.ToDouble(totalWidth * LeftPaneWidthPercent / 100);
                leftPane.Width = newLeftPaneWidth > LeftPaneMinimumWidth ? newLeftPaneWidth : LeftPaneMinimumWidth;

                if (IsRightPaneVisible)
                {
                    totalWidth = ActualWidth - ResizeGripWidth * 2;
                    double newRightPaneWidth = Convert.ToDouble(totalWidth * RightPaneWidthPercent / 100);
                    rightPane.Width = newRightPaneWidth > RightPaneMinimumWidth ? newRightPaneWidth : RightPaneMinimumWidth;
                    double proposedWidth = totalWidth - leftPane.Width - rightPane.Width;
                    middlePane.Width = proposedWidth >= 0 ? proposedWidth : 0.0;
                }
                else
                {
                    double newMiddlePaneWidth = totalWidth - leftPane.Width;
                    middlePane.Width = newMiddlePaneWidth > MiddlePaneMinimumWidth ? newMiddlePaneWidth : MiddlePaneMinimumWidth;
                }

                WidthPercentChanged?.Invoke(this, default);
            }
        }

        private void RecalculatePercentages()
        {
            double totalWidth = GetTotalWidth();

            LeftPaneWidthPercent = Math.Round(leftPane.Width * 100 / totalWidth);
            if (IsRightPaneVisible) RightPaneWidthPercent = Math.Round(rightPane.Width * 100 / totalWidth);

            ApplyPercentages();
        }

        private void ResizeContent()
        {
            leftPaneContent.Width = leftPane.ActualWidth;
            middlePaneContent.Width = middlePane.ActualWidth;
            if (IsRightPaneVisible) rightPaneContent.Width = rightPane.ActualWidth;
        }

        private void MultiPanePanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ApplyPercentages();
        }

        private void MultiPanePanel_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!CanResize) { return; }

            if (isSeparatorLeftMouseButtonDown)
            {
                var p = e.GetCurrentPoint(separatorLeft);

                double newLeftWidth = leftPane.Width + p.Position.X;
                double newMiddleWidth = middlePane.Width - p.Position.X;

                if (newLeftWidth > LeftPaneMinimumWidth && newMiddleWidth > MiddlePaneMinimumWidth)
                {
                    leftPane.Width = newLeftWidth;
                    middlePane.Width = newMiddleWidth;
                    RecalculatePercentages();
                }
            }

            if (isSeparatorRightMouseButtonDown && IsRightPaneVisible)
            {
                var p = e.GetCurrentPoint(separatorRight);

                double newRightWidth = rightPane.Width - p.Position.X;
                double newMiddleWidth = middlePane.Width + p.Position.X;

                if (newRightWidth > RightPaneMinimumWidth && newMiddleWidth > MiddlePaneMinimumWidth)
                {
                    rightPane.Width = newRightWidth;
                    middlePane.Width = newMiddleWidth;
                    RecalculatePercentages();
                }
            }
        }

        public MultiPanePanel()
        {
            DefaultStyleKey = typeof(MultiPanePanel);
            //InitializeComponent();
        }
    }
}
