using Kasay.DependencyProperty;
using Magentaize.FluentPlayer.ViewModels;
using ReactiveUI;
using System.Reactive.Linq;
using Windows.UI.Xaml.Controls;

namespace Magentaize.FluentPlayer.Views
{
    public sealed partial class PlaybackSlider : UserControl, IViewFor<PlaybackSliderViewModel>
    {
        public PlaybackSlider()
        {
            ViewModel = new PlaybackSliderViewModel();
            InitializeComponent();

            Observable.FromEventPattern(ProgressSlider, nameof(ProgressSlider.SliderDragStarted))
                .InvokeCommand(ViewModel.ProgressSliderOnManipulationStarting);
            Observable.FromEventPattern(ProgressSlider, nameof(ProgressSlider.SliderDragCompleted))
                .InvokeCommand(ViewModel.ProgressSliderOnManipulationCompleted);
        }

        [Bind]
        public PlaybackSliderViewModel ViewModel { get; set; }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (PlaybackSliderViewModel)value;
        }
    }
}
