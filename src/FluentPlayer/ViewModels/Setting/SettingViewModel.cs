namespace Magentaize.FluentPlayer.ViewModels.Setting
{
    public class SettingViewModel : BaseViewModel
    {
        public CollectionSettingViewModel CollectionSettingViewModel { get; }

        public BehaviorSettingViewModel BehaviorSettingViewModel { get; }

        public SettingViewModel(CollectionSettingViewModel cvm, BehaviorSettingViewModel avm)
        {
            CollectionSettingViewModel = cvm;
            BehaviorSettingViewModel = avm;
        }
    }
}