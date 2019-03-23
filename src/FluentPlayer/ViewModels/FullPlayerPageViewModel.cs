using Magentaize.FluentPlayer.Core;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Magentaize.FluentPlayer.ViewModels
{
    public class FullPlayerPageViewModel : ReactiveObject
    {
        [ObservableAsProperty]
        public bool IsPlaying { get; }

        public FullPlayerPageViewModel()
        {
            var pbs = ServiceFacade.PlaybackService;
            pbs.IsPlaying.ToPropertyEx(this, x => x.IsPlaying);
        }
    }
}