using Magentaize.FluentPlayer.Views.FullPlayer;
using Prism.Mvvm;
using Prism.Navigation;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace Magentaize.FluentPlayer.ViewModels
{
    public class ShellViewModel : BindableBase, INavigatedAwareAsync
    {
        private INavigationService _nav;

        public ShellViewModel(INavigationService nav)
        {
            _nav = nav;
        }

        public Task OnNavigatedToAsync(INavigationParameters parameters)
        {
            //_nav = parameters.GetNavigationService();
            return Task.CompletedTask;
        }

        public async void ItemClick(object sender, RoutedEventArgs e)
        {
            await _nav.NavigateAsync(nameof(FullPlayer));
        }
    }
}