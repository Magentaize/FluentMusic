using Kasay.DependencyProperty;
using Magentaize.FluentPlayer.ViewModels;
using ReactiveUI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Magentaize.FluentPlayer.Views
{
    public sealed partial class FullPlayerArtistView : UserControl, IViewFor<FullPlayerArtistViewModel>
    {
        public FullPlayerArtistView()
        {
            ViewModel = new FullPlayerArtistViewModel();

            InitializeComponent();

            ArtistList.Events().Tapped
                .InvokeCommand(ViewModel.ArtistListTapped);

            ArtistList.Events().DoubleTapped
                .InvokeCommand(ViewModel.PlayArtist);
            
            TrackList.Events().DoubleTapped
                .InvokeCommand(ViewModel.PlayTrack);

            AlbumGridView.Events().Tapped
                .InvokeCommand(ViewModel.AlbumGridViewTapped);

            AlbumGridView.Events().DoubleTapped
                .InvokeCommand(ViewModel.PlayAlbum);

            this.WhenActivated(d => { });
        }

        [Bind]
        public FullPlayerArtistViewModel ViewModel { get; set; }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (FullPlayerArtistViewModel)value;
        }
    }
}
