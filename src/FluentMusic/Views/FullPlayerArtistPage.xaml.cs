using FluentMusic.ViewModels;
using Kasay.DependencyProperty;
using ReactiveUI;
using System.Reactive.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Navigation;

namespace FluentMusic.Views
{
    public sealed partial class FullPlayerArtistPage : Page
    {
        public FullPlayerArtistPage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
            ViewModel = new FullPlayerArtistViewModel();

            Observable.FromEventPattern<RoutedEventHandler, RoutedEventArgs>(
                h => MultiPanePanel.WidthPercentChanged += h,
                h => MultiPanePanel.WidthPercentChanged -= h)
                .Select(x => x.EventArgs)
                .Subscribe(ViewModel.WidthsChanged);

            ArtistList.Events()
                .SelectionChanged
                .Subscribe(ViewModel.ArtistListSelectionChanged);

            AlbumGridView.Events()
                .SelectionChanged
                .Subscribe(ViewModel.AlbumGridSelectionChanged);

            TrackList.Events()
                .SelectionChanged
                .Subscribe(ViewModel.TrackListSelectionChanged);

            RestoreArtistButton.Events()
                .Tapped
                .Subscribe(ViewModel.RestoreArtistButtonTapped);

            RestoreAlbumButton.Events()
                .Tapped
                .Subscribe(ViewModel.RestoreAlbumButtonTapped);

            ArtistList.Events()
                .Tapped
                .Subscribe(ViewModel.ArtistListTapped);

            //ArtistList.Events().DoubleTapped
            //    .Subscribe(ViewModel.PlayArtistCommand);

            AlbumGridView.Events()
                .Tapped
                .Subscribe(ViewModel.AlbumGridTapped);

            //AlbumGridView.Events().DoubleTapped
            //    .Subscribe(ViewModel.PlayAlbumCommand);
            TrackList.Events()
                .DoubleTapped
                .Subscribe(ViewModel.TrackListDoubleTapped);
        }

        [Bind]
        public FullPlayerArtistViewModel ViewModel { get; set; }
    }
}
