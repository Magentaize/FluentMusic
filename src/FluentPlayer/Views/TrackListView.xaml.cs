using Magentaize.FluentPlayer.Data;
using Magentaize.FluentPlayer.ViewModels;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Magentaize.FluentPlayer.Views
{
    public sealed partial class TrackListView : UserControl
    {
        public TrackListViewViewModel Vm { get; set; } = new TrackListViewViewModel();

        public ObservableCollection<Track> Data
        {
            get => (ObservableCollection<Track>) GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(ObservableCollection<Track>), typeof(TrackListView), new PropertyMetadata(null, DataPropertyChangedCallback));

        private static async void DataPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            await ((TrackListView)d).DataPropertyChanged();
        }

        public TrackListView()
        {
            InitializeComponent();
        }

        private async Task DataPropertyChanged()
        {
            await Vm.FillCvsSource(Data);
        }
    }
}
