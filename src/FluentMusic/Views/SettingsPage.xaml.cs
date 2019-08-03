using Kasay.DependencyProperty;
using FluentMusic.ViewModels;
using ReactiveUI;
using Windows.UI.Xaml.Controls;

namespace FluentMusic.Views
{
    public sealed partial class SettingsPage : Page
    {
        [Bind]
        internal SettingsPageViewModel ViewModel { get; set; }

        public SettingsPage()
        {
            ViewModel = new SettingsPageViewModel();
            this.InitializeComponent();
        }

        private void TextBlock_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            e.Handled = true;
        }
    }
}
