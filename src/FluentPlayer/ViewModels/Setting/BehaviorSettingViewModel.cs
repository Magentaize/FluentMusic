using Magentaize.FluentPlayer.Core.Extensions;

namespace Magentaize.FluentPlayer.ViewModels.Setting
{
    public class BehaviorSettingViewModel : BaseViewModel
    {
        private bool _scrollToPlayingSongAutomaticallyChecked;

        public bool ScrollToPlayingSongAutomaticallyChecked
        {
            get => _scrollToPlayingSongAutomaticallyChecked;
            set
            {
                SetProperty(ref _scrollToPlayingSongAutomaticallyChecked, value);
                Static.LocalSettings[Static.Settings.Behavior.AutoScroll] = value;
            }
        }

        public BehaviorSettingViewModel()
        {
            _scrollToPlayingSongAutomaticallyChecked =
                Static.LocalSettings[Static.Settings.Behavior.AutoScroll].Cast<bool>();
        }
    }
}