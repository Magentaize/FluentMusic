using Kasay.DependencyProperty;
using FluentMusic.ViewModels.Common;
using ReactiveUI;
using Windows.UI.Xaml.Controls;

namespace FluentMusic.Views.Common
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
