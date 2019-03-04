using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AutoMapper;
using Magentaize.FluentPlayer.Data;
using Prism.Mvvm;

namespace Magentaize.FluentPlayer.ViewModels
{
    public class AlbumListViewViewModel : BindableBase
    {
        private ObservableCollection<AlbumViewModel> _albums;

        public ObservableCollection<AlbumViewModel> Albums
        {
            get => _albums;
            set => SetProperty(ref _albums, value);
        }

        public void FillCvsSource(IEnumerable<Album> data)
        {
            Albums = new ObservableCollection<AlbumViewModel>(data.Select(Mapper.Map<Album, AlbumViewModel>));
        }
    }
}