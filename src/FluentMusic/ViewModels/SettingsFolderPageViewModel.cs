using DynamicData;
using DynamicData.Binding;
using FluentMusic.Core;
using FluentMusic.Core.Services;
using FluentMusic.ViewModels.Common;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace FluentMusic.ViewModels
{
    public sealed class SettingsFolderPageViewModel : ReactiveObject
    {
        public ObservableCollectionExtended<FolderViewModel> MusicFolders { get; } = new ObservableCollectionExtended<FolderViewModel>();

        public ReactiveCommand<Unit, Unit> AddMusicFolderCommand { get; set; }

        public SettingsFolderPageViewModel()
        {
            IndexService.MusicFolders.Connect().Bind(MusicFolders).Subscribe();

            AddMusicFolderCommand = ReactiveCommand.CreateFromTask(IndexService.RequestAddFolderAsync);
            AddMusicFolderCommand.ThrownExceptions.Subscribe(o => Debug.WriteLine(o));
        }
    }
}
