using Kasay.DependencyProperty;
using FluentMusic.ViewModels;
using ReactiveUI;
using System.Reactive.Linq;
using Windows.UI.Xaml.Controls;

namespace FluentMusic.Views
{
    public sealed partial class PlaybackSlider : UserControl
    {
        public PlaybackSlider()
        {
            ViewModel = new PlaybackSliderViewModel();
            InitializeComponent();

            ProgressSlider
                .Events()
                .ManipulationStarted
                .Subscribe(ViewModel.ProgressSliderManipulationStarted);

            ProgressSlider
                .Events()
                .ManipulationCompleted
                .Subscribe(ViewModel.ProgressSliderManipulationCompleted);
        }

        [Bind]
        public PlaybackSliderViewModel ViewModel { get; set; }
    }
}
