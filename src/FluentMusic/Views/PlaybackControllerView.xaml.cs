using Kasay.DependencyProperty;
using FluentMusic.ViewModels;
using ReactiveUI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FluentMusic.Views
{
    public sealed partial class PlaybackControllerView : UserControl, IViewFor<PlaybackControllerViewModel>
    {
        public PlaybackControllerView()
        {
            ViewModel = new PlaybackControllerViewModel();
            InitializeComponent();
            this.WhenActivated(disposables => { });
        }

        [Bind]
        public PlaybackControllerViewModel ViewModel { get; set; }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (PlaybackControllerViewModel)value;
        }
    }
}
