using FluentMusic.ViewModels;
using Kasay.DependencyProperty;
using ReactiveUI;
using System.Reactive.Disposables;
using Windows.UI.Xaml.Controls;

namespace FluentMusic.Views
{
    public sealed partial class SettingsInterfacePage : Page, IViewFor<SettingsInterfacePageViewModel>
    {
        public SettingsInterfacePage() : base()
        {          
            InitializeComponent();
            ViewModel = new SettingsInterfacePageViewModel();

            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.ElementThemes, v => v.ThemeComboBox.ItemsSource)
                    .DisposeWith(d);
                this.Bind(ViewModel, vm => vm.SelectedElementTheme, v => v.ThemeComboBox.SelectedItem)
                    .DisposeWith(d);
            });
        }

        [Bind]
        public SettingsInterfacePageViewModel ViewModel { get; set; }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (SettingsInterfacePageViewModel)value;
        }

    }
}
