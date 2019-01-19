using Magentaize.FluentPlayer.Core.Extensions;

namespace Magentaize.FluentPlayer.ViewModels.Setting
{
    public class CollectionSettingViewModel : BaseViewModel
    {
        private bool _refreshCollectionAutomaticallyChecked;

        public bool RefreshCollectionAutomaticallyChecked
        {
            get => _refreshCollectionAutomaticallyChecked;
            set
            {
                SetProperty(ref _refreshCollectionAutomaticallyChecked, value);
                Static.LocalSettings[Static.Settings.Collection.AutoRefresh] = value;
            }
        }

        public CollectionSettingViewModel()
        {
           _refreshCollectionAutomaticallyChecked =
                Static.LocalSettings[Static.Settings.Collection.AutoRefresh].Cast<bool>();
        }
    }
}