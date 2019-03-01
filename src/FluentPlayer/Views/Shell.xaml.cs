using Magentaize.FluentPlayer.ViewModels;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;

namespace Magentaize.FluentPlayer.Views
{
    public sealed partial class Shell : Page
    {
        public Shell()
        {
            this.InitializeComponent();

            ExtendView();
        }

        public ShellViewModel ViewModel => DataContext as ShellViewModel;

        private void ExtendView()
        {
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        }
    }
}
