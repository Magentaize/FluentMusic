using Magentaize.FluentPlayer.ViewModels.Setting;

namespace Magentaize.FluentPlayer.ViewModels.FullPlayer
{
    public class FullPlayerViewModel : BaseViewModel
    {
        public SettingViewModel SettingViewModel { get; }

        public FullPlayerViewModel(SettingViewModel svm)
        {
            SettingViewModel = svm;
        }
    }
}