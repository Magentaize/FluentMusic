using Prism.Mvvm;
using Prism.Navigation;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace Magentaize.FluentPlayer.ViewModels
{
    public class ShellViewModel : BindableBase, INavigatedAwareAsync
    {
        private double _titleBarHeight;

        public double TitleBarHeight
        {
            get => _titleBarHeight;
            set => SetProperty(ref _titleBarHeight, value);
        }

        private INavigationService _nav;

        public ShellViewModel()
        {
            //_nav = nav;
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