using DynamicData;
using DynamicData.Binding;
using FluentMusic.Core;
using FluentMusic.ViewModels.Common;
using System;
using System.Threading.Tasks;

namespace FluentMusic
{
    public static class ViewModelAccessor
    {
        public static IObservableCollection<AlbumViewModel> AlbumVmSource { get; } = new ObservableCollectionExtended<AlbumViewModel>();

        public static async Task StartupAsync()
        {
            ServiceFacade.IndexService.AlbumSource
                .RemoveKey()
                .Bind(AlbumVmSource)
                .Subscribe();

            await Task.CompletedTask;
        }
    }
}
