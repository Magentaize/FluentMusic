using Kasay.DependencyProperty;
using FluentMusic.ViewModels.Common;
using ReactiveUI;
using Windows.UI.Xaml.Controls;

namespace FluentMusic.Views.Common
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
