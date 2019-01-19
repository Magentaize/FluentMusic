using Windows.UI.Xaml.Controls;
using Magentaize.FluentPlayer.ViewModels;

namespace Magentaize.FluentPlayer.Views
{
    public sealed partial class Shell : Page
    {
        public Shell()
        {
            this.InitializeComponent();
        }

        public ShellViewModel ViewModel => DataContext as ShellViewModel;
    }
}
