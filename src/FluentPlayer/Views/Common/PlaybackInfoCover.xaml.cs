using Kasay.DependencyProperty;
using Magentaize.FluentPlayer.ViewModels.Common;
using ReactiveUI;
using Windows.UI.Xaml.Controls;

namespace Magentaize.FluentPlayer.Views.Common
{
    public sealed partial class PlaybackInfoCover : UserControl, IViewFor<PlaybackInfoCoverViewModel>
    {
        public PlaybackInfoCover()
        {
            ViewModel = new PlaybackInfoCoverViewModel();
            InitializeComponent();
        }

        [Bind]
        public PlaybackInfoCoverViewModel ViewModel { get; set; }
        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (PlaybackInfoCoverViewModel)value;
        }
    }
}
