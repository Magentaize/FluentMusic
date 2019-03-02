using Magentaize.FluentPlayer.ViewModels;
using Windows.UI.Xaml.Controls;

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
