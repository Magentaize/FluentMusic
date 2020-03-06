using FluentMusic.ViewModels;
using Kasay.DependencyProperty;
using Windows.UI.Xaml.Controls;

namespace FluentMusic.Views
{
    public sealed partial class SettingsFolderPage : Page
    {
        [Bind]
        internal SettingsFolderPageViewModel ViewModel { get; set; }

        public SettingsFolderPage()
        {
            ViewModel = new SettingsFolderPageViewModel();
            this.InitializeComponent();
        }
    }
}
