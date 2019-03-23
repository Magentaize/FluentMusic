using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;

namespace Magentaize.FluentPlayer.ViewModels.Common
{
    public class PlaybackInfoTextPropertyViewModel : ReactiveObject
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        [Reactive]
        public string CurrentPosition { get; set; }
        public string NaturalPosition { get; set; }
    }
}
