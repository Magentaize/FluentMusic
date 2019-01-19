using Magentaize.FluentPlayer.ViewModels.Setting;

namespace Magentaize.FluentPlayer.Views.Setting
{
    public sealed partial class Setting
    {
        public SettingViewModel ViewModel => DataContext as SettingViewModel;

        public Setting()
        {
            this.InitializeComponent();
        }
    }
}
