using Magentaize.FluentPlayer.ViewModels.Setting;

namespace Magentaize.FluentPlayer.ViewModels.FullPlayer
{
    public class FullPlayerViewModel : BaseViewModel
    {
        public SettingViewModel SettingViewModel { get; }
        public ArtistsControlViewModel ArtistsControlViewModel { get; }

        public FullPlayerViewModel(SettingViewModel svm, ArtistsControlViewModel acvm)
        {
            SettingViewModel = svm;
            ArtistsControlViewModel = acvm;
        }
    }
}