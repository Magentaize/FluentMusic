using System;

namespace Magentaize.FluentPlayer.ViewModels.Common
{
    public class PlaybackInfoCoverThumbnailViewModel
    {
        public string Uri { get; set; }

        public override string ToString() => Uri;
    }
}
