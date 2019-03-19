using Magentaize.FluentPlayer.Core.Extensions;
using Magentaize.FluentPlayer.ViewModels;
using Magentaize.FluentPlayer.ViewModels.DataViewModel;
using ReactiveUI;
using System;
using System.Diagnostics;
using System.Reactive.Linq;
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

            ArtistList.Events().DoubleTapped
                .InvokeCommand(ViewModel.PlayArtist);
            

            TrackList.Events().DoubleTapped
                .InvokeCommand(ViewModel.PlayTrack);
        }

        public FullPlayerArtistViewModel ViewModel
        {
            get => (FullPlayerArtistViewModel)GetValue(ViewModelProperty);

            set => SetValue(ViewModelProperty, value);
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", typeof(FullPlayerArtistViewModel), typeof(FullPlayerArtistView), null);

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (FullPlayerArtistViewModel)value;
        }

        private bool _singleTap;
    }
}
