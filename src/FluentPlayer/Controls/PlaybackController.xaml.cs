using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Magentaize.FluentPlayer.Controls
{
    public sealed partial class PlaybackController : UserControl
    {
        public PlaybackController()
        {
            this.InitializeComponent();
        }

        public static readonly DependencyProperty IsPlayingProperty = DependencyProperty.Register("IsPlaying", typeof(bool), typeof(PlaybackController), new PropertyMetadata(false));

        public bool IsPlaying
        {
            get => (bool)GetValue(IsPlayingProperty);
            set => SetValue(IsPlayingProperty, value);
        }

        public static readonly DependencyProperty ResumePauseCommandProperty = DependencyProperty.Register("ResumePauseCommand", typeof(ICommand), typeof(PlaybackController), new PropertyMetadata(null));

        public ICommand ResumePauseCommand
        {
            get => (ICommand)GetValue(ResumePauseCommandProperty);
            set => SetValue(ResumePauseCommandProperty, value);
        }

        public static readonly DependencyProperty NextCommandProperty = DependencyProperty.Register("NextCommand", typeof(ICommand), typeof(PlaybackController), new PropertyMetadata(null));

        public ICommand NextCommand
        {
            get => (ICommand)GetValue(NextCommandProperty);
            set => SetValue(NextCommandProperty, value);
        }

        public static readonly DependencyProperty PreviousCommandProperty = DependencyProperty.Register("PreviousCommand", typeof(ICommand), typeof(PlaybackController), new PropertyMetadata(null));

        public ICommand PreviousCommand
        {
            get => (ICommand)GetValue(PreviousCommandProperty);
            set => SetValue(PreviousCommandProperty, value);
        }
    }
}
