using DynamicData;
using DynamicData.Binding;
using Magentaize.FluentPlayer.Core;
using Magentaize.FluentPlayer.ViewModels.DataViewModel;
using System;
using System.Threading.Tasks;

namespace Magentaize.FluentPlayer
{
    public static class ViewModelAccessor
    {
        public static IObservableCollection<AlbumViewModel> AlbumVmSource { get; } = new ObservableCollectionExtended<AlbumViewModel>();

        public static async Task StartupAsync()
        {
            ServiceFacade.IndexService.AlbumSource
                .Transform(x => new AlbumViewModel(x))
                .Bind(AlbumVmSource)
                .Subscribe();

            await Task.CompletedTask;
        }
    }
}
