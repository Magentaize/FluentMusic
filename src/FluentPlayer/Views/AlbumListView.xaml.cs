using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Magentaize.FluentPlayer.Data;
using Magentaize.FluentPlayer.ViewModels;

namespace Magentaize.FluentPlayer.Views
{
    public sealed partial class AlbumListView : UserControl
    {
        public AlbumListViewViewModel Vm { get; set; } = new AlbumListViewViewModel();

        public IEnumerable<Album> Data
        {
            get => (IEnumerable<Album>)GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(IEnumerable<Album>), typeof(AlbumListView), new PropertyMetadata(null, DataPropertyChangedCallback));

        private static void DataPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AlbumListView)d).DataPropertyChanged();
        }

        private void DataPropertyChanged()
        {
            Vm.FillCvsSource(Data);
        }

        public AlbumListView()
        {
            this.InitializeComponent();
        }
    }
}
