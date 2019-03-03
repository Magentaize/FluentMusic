using Magentaize.FluentPlayer.Data;
using Prism.Mvvm;

namespace Magentaize.FluentPlayer.ViewModels
{
    public class TrackViewModel
    {
        public Track Track { get; set; }
        
        public BindableBase ParentViewModel { get; set; }
    }
}