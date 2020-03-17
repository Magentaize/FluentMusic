using DynamicData;
using DynamicData.Binding;
using FluentMusic.Core.Services;
using FluentMusic.ViewModels.Common;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Z.Linq;

namespace FluentMusic.ViewModels
{
    public sealed class FullPlayerArtistViewModel : ReactiveObject
    {
        public ObservableCollectionExtended<GroupArtistViewModel> ArtistCvsSource { get; } = new ObservableCollectionExtended<GroupArtistViewModel>();
        public ObservableCollectionExtended<AlbumViewModel> AlbumCvsSource { get; } = new ObservableCollectionExtended<AlbumViewModel>();
        public ObservableCollectionExtended<GroupTrackViewModel> TrackCvsSource { get; } = new ObservableCollectionExtended<GroupTrackViewModel>();

        public ISubject<RoutedEventArgs> WidthsChanged { get; } = new Subject<RoutedEventArgs>();
        public ISubject<SelectionChangedEventArgs> ArtistListSelectionChanged { get; } = new Subject<SelectionChangedEventArgs>();
        public ISubject<SelectionChangedEventArgs> AlbumGridSelectionChanged { get; } = new Subject<SelectionChangedEventArgs>();
        public ISubject<SelectionChangedEventArgs> TrackListSelectionChanged { get; } = new Subject<SelectionChangedEventArgs>();
        public ISubject<RoutedEventArgs> ArtistListTapped { get; } = new Subject<RoutedEventArgs>();
        public ISubject<RoutedEventArgs> RestoreArtistButtonTapped { get; } = new Subject<RoutedEventArgs>();
        public ISubject<RoutedEventArgs> AlbumGridTapped { get; } = new Subject<RoutedEventArgs>();
        public ISubject<RoutedEventArgs> RestoreAlbumButtonTapped { get; } = new Subject<RoutedEventArgs>();
        public ISubject<RoutedEventArgs> TrackListDoubleTapped { get; } = new Subject<RoutedEventArgs>();

        [Reactive]
        public double LeftPaneWidthPercent { get; set; }
        [Reactive]
        public double RightPaneWidthPercent { get; set; }
        [Reactive]
        public TrackViewModel TrackListSelected { get; set; }
        [Reactive]
        public ArtistViewModel ArtistListSelectedItem { get; set; }
        [Reactive]
        public AlbumViewModel AlbumGridSelectedItem { get; set; }
        [ObservableAsProperty]
        public int CurrentTrackCount { get; }

        private ViewStatus _status = ViewStatus.Normal;

        public FullPlayerArtistViewModel()
        {
            WidthsChanged
                .ObservableOnThreadPool()
                .Throttle(TimeSpan.FromSeconds(2))
                .Subscribe(_ => Setting.Interface.ArtistPageWidths.OnNext(new double[] { LeftPaneWidthPercent, RightPaneWidthPercent }));

            Setting.Interface.ArtistPageWidths
                .Take(1)
                .SubscribeOnDispatcher()
                .Subscribe(x =>
                {
                    LeftPaneWidthPercent = x[0];
                    RightPaneWidthPercent = x[1];
                });

            var _selectedArtists = ArtistListSelectionChanged.AsObservableList<ArtistViewModel>();
            var _selectedAlbums = AlbumGridSelectionChanged.AsObservableList<AlbumViewModel>();
            var _selectedTracks = TrackListSelectionChanged.AsObservableList<TrackViewModel>();

            var artistOrigin = IndexService.ArtistSource.Connect().RemoveKey();
            artistOrigin
                .ObservableOnThreadPool()
                .GroupOn(x => x.Name.Substring(0, 1))
                .Transform(x => new GroupArtistViewModel(x))
                .Sort(SortExpressionComparer<GroupArtistViewModel>.Ascending(x => x.Key))
                .ObserveOnCoreDispatcher()
                .Bind(ArtistCvsSource)
                .Subscribe();

            var albumOrigin = IndexService.AlbumSource.Connect().RemoveKey();
            var albumSourceObservable = new ReplaySubject<IObservable<IChangeSet<AlbumViewModel>>>(1);
            albumSourceObservable
                .ObservableOnThreadPool()
                .Switch()
                .ObserveOnCoreDispatcher()
                .Bind(AlbumCvsSource)
                .Subscribe();

            var trackOrigin = IndexService.TrackSource.Connect().RemoveKey();
            var trackSourceObservable = new Subject<IObservable<IChangeSet<TrackViewModel>>>();
            trackSourceObservable
                .ObservableOnThreadPool()
                .Switch()
                .GroupOn(x => x.Title.Substring(0, 1))
                .Transform(x => new GroupTrackViewModel(x))
                .Sort(SortExpressionComparer<GroupTrackViewModel>.Ascending(x => x.Key))
                .DisposeMany()
                .ObserveOnCoreDispatcher()
                .Bind(TrackCvsSource)
                .Subscribe();

            Observable.FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                h => TrackCvsSource.CollectionChanged += h, h => TrackCvsSource.CollectionChanged -= h)
                .ObservableOnThreadPool()
                .Throttle(TimeSpan.FromMilliseconds(10))
                .Select(_ => TrackCvsSource.Sum(x => x.Count))
                .ObserveOnCoreDispatcher()
                .ToPropertyEx(this, x => x.CurrentTrackCount);

            ArtistListTapped
                .Where(_ => !_status.HasFlag(ViewStatus.AristTapped))
                .Subscribe(_ =>
                {
                    _status |= ViewStatus.AristTapped;
                    _status &= ~ViewStatus.AlbumTapped;
                    AlbumGridSelectedItem = null;

                    var albumSource = _selectedArtists.Connect().MergeManyEx(x => x.Albums.Connect().RemoveKey());
                    albumSourceObservable.OnNext(albumSource);

                    var trackSource = albumSource.MergeManyEx(x => x.Tracks.Connect().RemoveKey());
                    trackSourceObservable.OnNext(trackSource);
                });

            RestoreArtistButtonTapped
                .Where(_ => _status.HasFlag(ViewStatus.AristTapped))
                .Subscribe(_ =>
                {
                    _status &= ~ViewStatus.AristTapped;
                    _status &= ~ViewStatus.AlbumTapped;
                    ArtistListSelectedItem = null;
                    AlbumGridSelectedItem = null;

                    albumSourceObservable.OnNext(albumOrigin);
                    trackSourceObservable.OnNext(trackOrigin);
                });

            AlbumGridTapped
                .Where(_ => !_status.HasFlag(ViewStatus.AlbumTapped))
                .Subscribe(_ =>
                {
                    _status |= ViewStatus.AlbumTapped;

                    var trackSource = _selectedAlbums.Connect().MergeManyEx(x => x.Tracks.Connect().RemoveKey());
                    trackSourceObservable.OnNext(trackSource);
                });

            RestoreAlbumButtonTapped
                .Where(_ => _status.HasFlag(ViewStatus.AlbumTapped) && !_status.HasFlag(ViewStatus.AristTapped))
                .Subscribe(_ =>
                {
                    _status &= ~ViewStatus.AlbumTapped;
                    AlbumGridSelectedItem = null;

                    trackSourceObservable.OnNext(trackOrigin);
                });
            RestoreAlbumButtonTapped
                .Where(_ => _status.HasFlag(ViewStatus.AlbumTapped) && _status.HasFlag(ViewStatus.AristTapped))
                .Subscribe(_ =>
                {
                    _status &= ~ViewStatus.AlbumTapped;
                    AlbumGridSelectedItem = null;

                    var trackSource = AlbumCvsSource.ToObservableChangeSet().MergeManyEx(x => x.Tracks.Connect().RemoveKey());
                    trackSourceObservable.OnNext(trackSource);
                });

            albumSourceObservable.OnNext(albumOrigin);
            trackSourceObservable.OnNext(trackOrigin);

            async Task PlayTrackAsync()
            {
                var tracks = await TrackCvsSource.SelectMany(x => x).ToListAsync();
                if (_selectedTracks.Count == 1)
                {
                    await PlaybackService.PlayAsync(tracks, _selectedTracks.Items.First()); 
                }
                else
                {
                    await PlaybackService.PlayAsync(tracks);
                }
            }

            TrackListDoubleTapped
                .SubscribeOnThreadPool()
                .Subscribe(async _ => await PlayTrackAsync());
        }

        [Flags]
        enum ViewStatus
        {
            Normal = 0,
            AlbumTapped = 1,
            AristTapped = 2,
        }
    }
}