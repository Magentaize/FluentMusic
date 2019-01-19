using Windows.UI.Xaml.Controls;
using Magentaize.FluentPlayer.ViewModels.Setting;

namespace Magentaize.FluentPlayer.Views.Setting
{
    public sealed partial class BehaviorSetting : UserControl
    {
        public BehaviorSettingViewModel ViewModel => DataContext as BehaviorSettingViewModel;

        public BehaviorSetting()
        {
            this.InitializeComponent();
        }
    }
}
