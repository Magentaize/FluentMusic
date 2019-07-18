using Kasay.DependencyProperty;
using Magentaize.FluentPlayer.ViewModels;
using ReactiveUI;
using Windows.UI.Xaml.Controls;

namespace Magentaize.FluentPlayer.Views
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
