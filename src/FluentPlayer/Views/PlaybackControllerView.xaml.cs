using Magentaize.FluentPlayer.ViewModels;
using ReactiveUI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Magentaize.FluentPlayer.Views
{
    public sealed partial class PlaybackControllerView : UserControl, IViewFor<PlaybackControllerViewModel>
    {
        public PlaybackControllerView()
        {
            ViewModel = new PlaybackControllerViewModel();
            InitializeComponent();
            this.WhenActivated(disposables => { });
        }

        public PlaybackControllerViewModel ViewModel
        {
            get => (PlaybackControllerViewModel)GetValue(ViewModelProperty);

            set => SetValue(ViewModelProperty, value);
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", typeof(PlaybackControllerViewModel), typeof(PlaybackControllerView), null);

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (PlaybackControllerViewModel)value;
        }
    }
}
