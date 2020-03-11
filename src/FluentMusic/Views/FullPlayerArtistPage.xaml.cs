using FluentMusic.ViewModels;
using Kasay.DependencyProperty;
using ReactiveUI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace FluentMusic.Views
{
    public sealed partial class FullPlayerArtistPage : Page, IViewFor<FullPlayerArtistViewModel>
    {
        public FullPlayerArtistPage()
        {
            InitializeComponent();
            NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Required;
            ViewModel = new FullPlayerArtistViewModel();

            ArtistList.Events()
                .SelectionChanged
                .Subscribe(ViewModel.ArtistListSelectionChanged);

            AlbumGridView.Events()
                .SelectionChanged
                .Subscribe(ViewModel.AlbumGridSelectionChanged);

            TrackList.Events().DoubleTapped
                .InvokeCommand(ViewModel.PlayTrackCommand);

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

            //TrackList.Events().DoubleTapped
            //    .Subscribe(ViewModel.PlayTrackCommand);

            this.WhenActivated(d =>
            {
                return;
            });
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
