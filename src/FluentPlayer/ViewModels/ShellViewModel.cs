using System.Threading.Tasks;
using Windows.UI.Xaml;
using Magentaize.FluentPlayer.Views;
using Prism.Mvvm;
using Prism.Navigation;

namespace Magentaize.FluentPlayer.ViewModels
{
    public class ShellViewModel : BindableBase, INavigatedAwareAsync
    {
        private INavigationService _nav;

        public Task OnNavigatedToAsync(INavigationParameters parameters)
        {
            _nav = parameters.GetNavigationService();
            return Task.CompletedTask;
        }

        public async void ItemClick(object sender, RoutedEventArgs e)
        {
            await _nav.NavigateAsync(nameof(FullPlayer));
        }
    }
}