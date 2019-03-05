using Magentaize.FluentPlayer.ViewModels;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Magentaize.FluentPlayer.Core.Extensions;
using Magentaize.FluentPlayer.Data;
using Magentaize.FluentPlayer.ViewModels.DataViewModel;

namespace Magentaize.FluentPlayer.Views
{
    public sealed partial class FullPlayerArtistView : UserControl
    {
        internal FullPlayerArtistViewViewModel Vm { get; set; } = new FullPlayerArtistViewViewModel();

        public FullPlayerArtistView()
        {
            InitializeComponent();
        }

        public CombinedDbViewModel ItemsSource
        {
            get => (CombinedDbViewModel)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(CombinedDbViewModel), typeof(FullPlayerArtistView), new PropertyMetadata(null, ItemsSourcePropertyChangedCallback));

        private static async void ItemsSourcePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            await ((FullPlayerArtistView)d).ItemsSourcePropertyChanged();
        }

        private async Task ItemsSourcePropertyChanged()
        {
            await Vm.FillCvsSourceAsync(ItemsSource);
        }

        private bool _singleTap;

        private async void ArtistItem_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            _singleTap = false;
        }

        private async void ArtistItem_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            _singleTap = true;
            await Task.Delay(200);
            if (_singleTap)
            {

            }
        }

        private async void TrackItem_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            _singleTap = false;
            await Vm.PlayAsync(sender.Cast<ListView>().SelectedItem.Cast<TrackViewModel>());

            e.Handled = true;
        }

        private async void TrackItem_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            _singleTap = true;
            await Task.Delay(200);
            if (_singleTap)
            {

            }
        }
    }
}
