using Magentaize.FluentPlayer.Data;
using Prism.Commands;
using Prism.Mvvm;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;

namespace Magentaize.FluentPlayer.Controls
{
    public sealed partial class TrackItemViewer : UserControl
    {
        public TrackItemViewerViewModel Vm = new TrackItemViewerViewModel();

        public TrackItemViewer()
        {
            this.InitializeComponent();

            Loaded += TrackItemViewer_Loaded;
        }

        private void TrackItemViewer_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Vm.Track = (Track) DataContext;
        }
    }

    public sealed class TrackItemViewerViewModel : BindableBase
    {
        private Track _track;

        public Track Track
        {
            get => _track;
            set => SetProperty(ref _track, value);
        }

        public ICommand PlayTrackCommand { get; }

        public TrackItemViewerViewModel()
        {
            PlayTrackCommand = new DelegateCommand(ExecutePlayTrack);
        }

        public void ExecutePlayTrack()
        {

        }
    }
}
