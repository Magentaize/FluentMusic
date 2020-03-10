using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive.Disposables;
using Windows.UI.Xaml;
using System.Reactive;
using System;

namespace FluentMusic.ViewModels
{
    public sealed class SettingsInterfacePageViewModel : ReactiveObject, IActivatableViewModel
    {
        public ICollection<ElementTheme> ElementThemes { get; } = new Collection<ElementTheme>
        {
            ElementTheme.Default,
            ElementTheme.Light,
            ElementTheme.Dark,
        };

        [Reactive]
        public ElementTheme SelectedElementTheme { get; set; }

        public SettingsInterfacePageViewModel()
        {
            this.WhenActivated(d =>
            {
                this.WhenAnyValue(x => x.SelectedElementTheme)
                    .Subscribe(x => Debug.WriteLine(x))
                    .DisposeWith(d);
            });
        }

        public ViewModelActivator Activator { get; } = new ViewModelActivator();
    }
}
