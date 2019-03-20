using DynamicData;
using DynamicData.Binding;
using Magentaize.FluentPlayer.Collections;
using Magentaize.FluentPlayer.Core;
using Magentaize.FluentPlayer.Data;
using Magentaize.FluentPlayer.ViewModels.DataViewModel;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;

namespace Magentaize.FluentPlayer.ViewModels
{
    public sealed class FullPlayerArtistViewModel : ReactiveObject, ISupportsActivation
    {
        private ReadOnlyObservableCollection<Grouping<char, TrackViewModel>> _trackCvsSource;

        public ReadOnlyObservableCollection<Grouping<char, TrackViewModel>> TrackCvsSource => _trackCvsSource;

        private ReadOnlyObservableCollection<Grouping<char, ArtistViewModel>> _artistCvsSource;

        public ReadOnlyObservableCollection<Grouping<char, ArtistViewModel>> ArtistCvsSource => _artistCvsSource;

        [Reactive]
        public int AlbumSelectedIndex { get; set; } = -1;

        [Reactive]
        public int ArtistListSelectedIndex { get; set; }

        [Reactive]
        public TrackViewModel TrackListSelected { get; set; }

        [Reactive]
        public ArtistViewModel ArtistListSelectedItem { get; set; }

        public ICommand RestoreArtistsCommand { get; }
        public ICommand PlayTrack { get; }
        public ICommand PlayArtist => PlayTrack;

        public ViewModelActivator Activator => throw new NotImplementedException();

        //private ReadOnlyObservableCollection<TrackViewModel> _trackList;

        public FullPlayerArtistViewModel()
        {

            RestoreArtistsCommand = ReactiveCommand.Create(() =>
            {
                ArtistListSelectedIndex = -1;
            });

            var artistList = ServiceFacade.IndexService.ArtistSource
                .Transform(x => new ArtistViewModel(x));
            //artistList.Subscribe();

            artistList
                .GroupOn(x => x.Artist.Name[0])
                .Transform(x => new Grouping<char, ArtistViewModel>(x))
                .Sort(SortExpressionComparer<Grouping<char, ArtistViewModel>>.Ascending(x => x.Key))
                .Bind(out _artistCvsSource)
                .Subscribe();

            var artistFilter = this.WhenAnyValue(x => x.ArtistListSelectedItem)
                .Select(BuildArtistFilter);

            var trackVmList = ServiceFacade.IndexService.TrackSource
                .Transform(x => new TrackViewModel(x));
            trackVmList.Subscribe();

            var filteredTrackVm = trackVmList.Filter(artistFilter);
            filteredTrackVm.Bind(out var _filteredTrackVm).Subscribe();

            PlayTrack = ReactiveCommand.Create<object>(async _ =>
            {
                var playlist = _filteredTrackVm.Select(x => x.Track);
                await ServiceFacade.PlaybackService.PlayAsync(playlist, TrackListSelected?.Track);
            });

            _filteredTrackVm
                .ToObservableChangeSet()
                .GroupOn(x => x.Track.TrackTitle[0])
                .Transform(x => new Grouping<char, TrackViewModel>(x))
                .Sort(SortExpressionComparer<Grouping<char, TrackViewModel>>.Ascending(x => x.Key))
                .Bind(out _trackCvsSource)
                .Subscribe();

            TrackViewModel lastPlayedTrack = null;

            ServiceFacade.PlaybackService.CurrentTrack
                .ObserveOnDispatcher()
                .Subscribe(x =>
                {
                    if (lastPlayedTrack != null) lastPlayedTrack.IsPlaying = false;
                    var xvm = _filteredTrackVm.First(vm => vm.Track == x);
                    xvm.IsPlaying = true;
                    lastPlayedTrack = xvm;
                });
        }

        private Func<TrackViewModel, bool> BuildArtistFilter(ArtistViewModel vm)
        {
            if (vm == null)
            {
                return t => true;
            }
            else
            {
                return t => t.Track.Artist == vm.Artist;
            }
        }
    }
}