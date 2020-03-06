using ReactiveUI;
using System;

namespace FluentMusic.ViewModels.Common
{
    public sealed class NavigationViewItemViewModel : ReactiveObject
    {
        public string Name { get; set; }

        public Type PageType { get; set; }
    }
}
