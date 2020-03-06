using DynamicData;
using DynamicData.Binding;
using FluentMusic.Core;
using FluentMusic.Core.Services;
using FluentMusic.ViewModels.Common;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace FluentMusic.ViewModels
{
    public class SettingsPageViewModel : ReactiveObject
    {
        public ObservableCollectionExtended<FolderViewModel> MusicFolders { get; } = new ObservableCollectionExtended<FolderViewModel>();

        public ReactiveCommand<Unit, Unit> AddMusicFolderCommand { get; set; }

        public SettingsPageViewModel()
        {
            IndexService.MusicFolders.Connect().Bind(MusicFolders).Subscribe();

            AddMusicFolderCommand = ReactiveCommand.CreateFromTask(Service.IndexService.RequestAddFolderAsync);
            AddMusicFolderCommand.ThrownExceptions.Subscribe(o => Debug.WriteLine(o));
        }
    }
}
