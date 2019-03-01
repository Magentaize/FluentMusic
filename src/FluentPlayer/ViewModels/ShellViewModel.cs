using Magentaize.FluentPlayer.Core;

namespace Magentaize.FluentPlayer.ViewModels
{
    public class ShellViewModel : BindableBase
    {
        private double _titleBarHeight;

        public double TitleBarHeight
        {
            get => _titleBarHeight;
            set => SetProperty(ref _titleBarHeight, value);
        }

        public ShellViewModel()
        {
            //_nav = nav;
        }
    }
}