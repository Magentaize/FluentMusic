﻿using FluentMusic.Core;
using FluentMusic.Core.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive;
using System.Reactive.Linq;
using static FluentMusic.Controls.SlideTile;

namespace FluentMusic.ViewModels.Common
{
    public class PlaybackInfoTextViewModel : ReactiveObject
    { 
        [ObservableAsProperty]
        public PlaybackInfoTextPropertyViewModel Property { get; set; }

        [ObservableAsProperty]
        public SlideDirection Direction { get; set; }

        public PlaybackInfoTextViewModel()
        {
            PlaybackService.NewTrackPlayed
                .Select(x => x.IsPlayingPreviousTrack ? SlideDirection.Down : SlideDirection.Up)
                .ObserveOnCoreDispatcher()
                .ToPropertyEx(this, x => x.Direction, SlideDirection.Up);
            PlaybackService.NewTrackPlayed
                .DistinctUntilChanged(x => x.Track)
                .Select(x => new PlaybackInfoTextPropertyViewModel
                {
                    Title = x.Track.Title,
                    Artist = x.Track.Album.Artist.Name,
                })
                .ObserveOnCoreDispatcher()
                .ToPropertyEx(this, x => x.Property);
        }
    }
}
