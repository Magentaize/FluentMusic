using Magentaize.FluentPlayer.Core;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Reactive.Linq;
using System.Windows.Input;

namespace Magentaize.FluentPlayer.ViewModels
{
    public class PlaybackControllerViewModel : ReactiveObject
    {
        [ObservableAsProperty]
        public bool PauseIconVisible { get; set; }

        [ObservableAsProperty]
        public bool ResumeIconVisible { get; set; }

        public ICommand Resume { get; }

        public ICommand Pause { get; }

        public ICommand Previous { get; }

        public ICommand Next { get; }

        public PlaybackControllerViewModel()
        {
            ServiceFacade.PlaybackService.IsPlaying
                .DistinctUntilChanged()
                .ToPropertyEx(this, x => x.PauseIconVisible, false);

            this.WhenAnyValue(x => x.PauseIconVisible)
                 .Select(x => !x)
                 .ToPropertyEx(this, x => x.ResumeIconVisible, true);

            Resume = ReactiveCommand.Create(ServiceFacade.PlaybackService.Resume);

            Pause = ReactiveCommand.Create(ServiceFacade.PlaybackService.Pause);
        }
    }
}
