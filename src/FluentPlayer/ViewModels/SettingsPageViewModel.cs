using Magentaize.FluentPlayer.Core;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Magentaize.FluentPlayer.ViewModels
{
    internal class SettingsPageViewModel : ReactiveObject
    {
        public ReadOnlyCollection<StorageFolder> MusicFolders { get; } = ServiceFacade.IndexService.MusicFolders;

        public SettingsPageViewModel()
        {
            
        }
    }
}
