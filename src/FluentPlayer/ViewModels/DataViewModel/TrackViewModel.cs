using Magentaize.FluentPlayer.Data;
using Prism.Mvvm;

namespace Magentaize.FluentPlayer.ViewModels.DataViewModel
{
    internal class TrackViewModel : BindableBase
    {
        public Track Track { get; }

        private bool _isPlaying = false;

        public bool IsPlaying
        {
            get => _isPlaying;
            set => SetProperty(ref _isPlaying, value);
        }

        public TrackViewModel(Track track)
        {
            Track = track;
        }
    }
}