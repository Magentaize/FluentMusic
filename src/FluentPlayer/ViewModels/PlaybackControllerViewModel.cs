using Magentaize.FluentPlayer.Core;
using Magentaize.FluentPlayer.Core.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive;
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

        [ObservableAsProperty]
        public bool EnableShuffle { get; set; }

        [Reactive]
        public bool EnableRepeatAll { get; set; }
        [Reactive]
        public bool RepeatAllVisible { get; set; }
        [Reactive]
        public bool RepeatOneVisible { get; set; }

        private RepeatMode _repeatMode;

        public ICommand Resume { get; }

        public ICommand Pause { get; }

        public ICommand Previous { get; }

        public ICommand Next { get; }

        public ICommand SwitchShuffle { get; }
        public ICommand SwitchRepeatMode { get; }

        public PlaybackControllerViewModel()
        {
            var pb = ServiceFacade.PlaybackService;

            pb.RepeatMode
                .DistinctUntilChanged()
                .ObservableOnCoreDispatcher()
                .Subscribe(x =>
                {
                    _repeatMode = x;
                    switch (x)
                    {
                        case RepeatMode.NotRepeat:
                            EnableRepeatAll = false;
                            RepeatAllVisible = true;
                            RepeatOneVisible = false;
                            break;
                        case RepeatMode.RepeatAll:
                            RepeatAllVisible = true;
                            EnableRepeatAll = true;
                            RepeatOneVisible = false;
                            break;
                        case RepeatMode.RepeatOne:
                            RepeatAllVisible = false;
                            RepeatOneVisible = true;
                            break;
                        default: throw new InvalidOperationException();
                    }
                });

            pb.EnableShuffle
                .DistinctUntilChanged()
                .ObservableOnCoreDispatcher()
                .ToPropertyEx(this, x => x.EnableShuffle, false);

            pb.IsPlaying
                .DistinctUntilChanged()
                .ObservableOnCoreDispatcher()
                .ToPropertyEx(this, x => x.PauseIconVisible, false);

            this.WhenAnyValue(x => x.PauseIconVisible)
                 .Select(x => !x)
                 .ToPropertyEx(this, x => x.ResumeIconVisible, true);

            Resume = ReactiveCommand.Create(ServiceFacade.PlaybackService.Resume);

            Pause = ReactiveCommand.Create(ServiceFacade.PlaybackService.Pause);

            Previous = ReactiveCommand.Create(() =>
            {
                pb.Previous();
            });

            Next = ReactiveCommand.Create(() =>
            {
                pb.Next();
            });

            SwitchShuffle = ReactiveCommand.Create(() =>
            {
                pb.EnableShuffle.OnNext(!EnableShuffle);
            });

            SwitchRepeatMode = ReactiveCommand.Create(() =>
            {
                switch (_repeatMode)
                {
                    case RepeatMode.NotRepeat:
                        pb.RepeatMode.OnNext(RepeatMode.RepeatAll);
                        break;
                    case RepeatMode.RepeatAll:
                        pb.RepeatMode.OnNext(RepeatMode.RepeatOne);
                        break;
                    case RepeatMode.RepeatOne:
                        pb.RepeatMode.OnNext(RepeatMode.NotRepeat);
                        break;
                    default: throw new InvalidOperationException(nameof(_repeatMode));
                }
            });
        }
    }
}
