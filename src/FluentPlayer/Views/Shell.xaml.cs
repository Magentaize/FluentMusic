using Magentaize.FluentPlayer.ViewModels;
using Windows.UI.Xaml.Controls;
using Magentaize.FluentPlayer.Core.Services;

namespace Magentaize.FluentPlayer.Views
{
    public sealed partial class Shell : Page
    {
        public Shell()
        {
            this.InitializeComponent();

            var i = new IndexService();
            i.BeginIndex();
        }

        public ShellViewModel ViewModel => DataContext as ShellViewModel;
    }
}
