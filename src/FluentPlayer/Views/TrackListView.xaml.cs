using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Magentaize.FluentPlayer.Data;
using Magentaize.FluentPlayer.ViewModels;

namespace Magentaize.FluentPlayer.Views
{
    public sealed partial class TrackListView : UserControl
    {
        public TrackListViewViewModel Vm { get; set; } = new TrackListViewViewModel();

        public IEnumerable<Track> Data
        {
            get => (IEnumerable<Track>) GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(IEnumerable<Track>), typeof(TrackListView), new PropertyMetadata(null, DataPropertyChangedCallback));

        private static void DataPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TrackListView)d).DataPropertyChanged();
        }

        public TrackListView()
        {
            InitializeComponent();
        }

        private void DataPropertyChanged()
        {
            Vm.FillCvsSource(Data);
        }
    }
}
