using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Magentaize.FluentPlayer.Data;
using Magentaize.FluentPlayer.ViewModels;

namespace Magentaize.FluentPlayer.Views
{
    public sealed partial class ArtistListView : UserControl
    {
        public ArtistListViewViewModel Vm { get; set; } = new ArtistListViewViewModel();

        public ObservableCollection<Artist> Data
        {
            get => (ObservableCollection<Artist>)GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(ObservableCollection<Artist>), typeof(ArtistListView), new PropertyMetadata(null, DataPropertyChangedCallback));

        private static async void DataPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            await ((ArtistListView)d).DataPropertyChanged();
        }

        private async Task DataPropertyChanged()
        {
            await Vm.FillCvsSource(Data);
        }

        public ICommand ItemTappedCommand
        {
            get => (ICommand) GetValue(ItemTappedCommandProperty);
            set => SetValue(ItemTappedCommandProperty, value);
        }

        public static readonly DependencyProperty ItemTappedCommandProperty =
            DependencyProperty.Register("ItemTappedCommand", typeof(ICommand), typeof(ArtistListView), new PropertyMetadata(null, ItemTappedCommandPropertyChangedCallback));

        private static void ItemTappedCommandPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ArtistListView)d).ItemTappedCommand.Execute(null);
        }

        public ArtistListView()
        {
            InitializeComponent();
        }

        private bool _singleTap;

        private async void Item_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            _singleTap = false;
        }

        private async void Item_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            _singleTap = true;
            await Task.Delay(200);
            if (_singleTap)
            {

            }
        }
    }
}
