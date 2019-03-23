using Kasay.DependencyProperty;
using Magentaize.FluentPlayer.ViewModels.Common;
using ReactiveUI;
using Windows.UI.Xaml.Controls;

namespace Magentaize.FluentPlayer.Views.Common
{
    public sealed partial class PlaybackInfoText : UserControl, IViewFor<PlaybackInfoTextViewModel>
    {
        public PlaybackInfoText()
        {
            ViewModel = new PlaybackInfoTextViewModel();
            InitializeComponent();
        }

        [Bind]
        public PlaybackInfoTextViewModel ViewModel { get; set; }
        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (PlaybackInfoTextViewModel)value;
        }
    }
}
