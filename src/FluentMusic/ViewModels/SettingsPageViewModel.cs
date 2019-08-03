using FluentMusic.Core;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace FluentMusic.ViewModels
{
    public class SettingsPageViewModel : ReactiveObject
    {
        public ReadOnlyCollection<StorageFolder> MusicFolders { get; } = ServiceFacade.IndexService.MusicFolders;

        public ReactiveCommand<StorageFolder, Unit> RemoveMusicFolderCommand { get; }

        public ReactiveCommand<Unit, Unit> AddMusicFolderCommand { get; set; }

        public SettingsPageViewModel()
        {
            RemoveMusicFolderCommand = ReactiveCommand.Create<StorageFolder>(async f => await ServiceFacade.IndexService.RequestRemoveFolderAsync(f));
            AddMusicFolderCommand = ReactiveCommand.CreateFromTask(ServiceFacade.IndexService.RequestAddFolderAsync);
        }
    }
}
