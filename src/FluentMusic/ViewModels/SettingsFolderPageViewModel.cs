using DynamicData;
using DynamicData.Binding;
using FluentMusic.Core.Services;
using FluentMusic.ViewModels.Common;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive;
using System.Reactive.Linq;

namespace FluentMusic.ViewModels
{
    public sealed class SettingsFolderPageViewModel : ReactiveObject
    {
        public ObservableCollectionExtended<FolderViewModel> MusicFolders { get; } = new ObservableCollectionExtended<FolderViewModel>();
        [ObservableAsProperty]
        public bool IsIndexing { get; }

        public ReactiveCommand<Unit, Unit> AddMusicFolderCommand { get; set; }

        public SettingsFolderPageViewModel()
        {
            IndexService.MusicFolders
                .Connect()
                .Bind(MusicFolders)
                .Subscribe();

            IndexService.IsIndexing
                .ObserveOnCoreDispatcher()
                .ToPropertyEx(this, x => x.IsIndexing);


            AddMusicFolderCommand = ReactiveCommand.CreateFromTask(IndexService.RequestAddFolderAsync);

        }
    }
}
