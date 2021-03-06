﻿using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;

namespace FluentMusic.ViewModels.Common
{
    public class PlaybackInfoTextPropertyViewModel : ReactiveObject
    {
        public string Title { get; set; }
        public string Artist { get; set; }
    }
}
