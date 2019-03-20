using Magentaize.FluentPlayer.Core;
using ReactiveUI;
using System.Reactive.Linq;
using System.Windows.Input;

namespace Magentaize.FluentPlayer.ViewModels
{
    public class PlaybackControllerViewModel : ReactiveObject
    {
        private readonly ObservableAsPropertyHelper<bool> _pauseIconVisible;
        public bool PauseIconVisible => _pauseIconVisible.Value;

        private readonly ObservableAsPropertyHelper<bool> _resumeIconVisible;
        public bool ResumeIconVisible => _resumeIconVisible.Value;

        public ICommand Resume { get; }

        public ICommand Pause { get; }

        public ICommand Previous { get; }

        public ICommand Next { get; }

        public PlaybackControllerViewModel()
        {
            _pauseIconVisible = ServiceFacade.PlaybackService.IsPlaying
                .DistinctUntilChanged()
                .ToProperty(this, x => x.PauseIconVisible, false);

            _resumeIconVisible = this
                 .WhenAnyValue(x => x.PauseIconVisible)
                 .Select(x => !x)
                 .ToProperty(this, x => x.ResumeIconVisible, true);

            Resume = ReactiveCommand.Create(ServiceFacade.PlaybackService.Resume);

            Pause = ReactiveCommand.Create(ServiceFacade.PlaybackService.Pause);
        }
    }
}
