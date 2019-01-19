using Windows.UI.Xaml.Controls;
using Magentaize.FluentPlayer.ViewModels.FullPlayer;

namespace Magentaize.FluentPlayer.Views.FullPlayer
{
    public sealed partial class FullPlayer : Page
    {
        public FullPlayerViewModel ViewModel => DataContext as FullPlayerViewModel;

        public FullPlayer()
        {
            this.InitializeComponent();
        }
    }
}
