using FluentMusic.Core.Services;
using FluentMusic.Data;
using ReactiveUI;
using System.Reactive;
using System.Windows.Input;

namespace FluentMusic.ViewModels.Common
{
    public sealed class FolderViewModel : ReactiveObject
    {
        private static ICommand _removeMusicFolderCommand = ReactiveCommand.CreateFromTask((FolderViewModel x) => IndexService.RequestRemoveFolderAsync(x));
        public ICommand RemoveMusicFolderCommand => _removeMusicFolderCommand;

        public long Id { get; private set; }
        public string Path { get; private set; }
        public string Token { get; set; }

        public static FolderViewModel Create(Folder f)
        {
            var o = new FolderViewModel()
            {
                Id = f.Id,
                Path = f.Path,
                Token = f.Token
            };

            return o;
        }
    }
}
