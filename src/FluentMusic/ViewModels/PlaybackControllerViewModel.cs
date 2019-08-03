using FluentMusic.Core;
using FluentMusic.Core.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using Windows.Media;

namespace FluentMusic.ViewModels
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
        public bool EnableRepeatList { get; set; }
        [Reactive]
        public bool RepeatListVisible { get; set; }
        [Reactive]
        public bool RepeatTrackVisible { get; set; }

        private MediaRepeatMode _repeatMode;

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
                        case MediaRepeatMode.None:
                            EnableRepeatList = false;
                            RepeatListVisible = true;
                            RepeatTrackVisible = false;
                            break;
                        case MediaRepeatMode.List:
                            RepeatListVisible = true;
                            EnableRepeatList = true;
                            RepeatTrackVisible = false;
                            break;
                        case MediaRepeatMode.Track:
                            RepeatListVisible = false;
                            RepeatTrackVisible = true;
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
                    case MediaRepeatMode.None:
                        pb.RepeatMode.OnNext(MediaRepeatMode.List);
                        break;
                    case MediaRepeatMode.List:
                        pb.RepeatMode.OnNext(MediaRepeatMode.Track);
                        break;
                    case MediaRepeatMode.Track:
                        pb.RepeatMode.OnNext(MediaRepeatMode.None);
                        break;
                    default: throw new InvalidOperationException(nameof(_repeatMode));
                }
            });
        }
    }
}
