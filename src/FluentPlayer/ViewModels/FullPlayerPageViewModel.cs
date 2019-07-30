using Magentaize.FluentPlayer.Core;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Reactive;

namespace Magentaize.FluentPlayer.ViewModels
{
    public class FullPlayerPageViewModel : ReactiveObject
    {
        [ObservableAsProperty]
        public bool IsPlaying { get; }

        public FullPlayerPageViewModel()
        {
            var pbs = ServiceFacade.PlaybackService;
            pbs.IsPlaying
                .ObservableOnCoreDispatcher()
                .ToPropertyEx(this, x => x.IsPlaying);
        }
    }
}