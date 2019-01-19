using Windows.UI.Xaml.Controls;
using Magentaize.FluentPlayer.ViewModels.Setting;

namespace Magentaize.FluentPlayer.Views.Setting
{
    public sealed partial class CollectionSetting : UserControl
    {
        public CollectionSettingViewModel ViewModel => DataContext as CollectionSettingViewModel;

        public CollectionSetting()
        {
            this.InitializeComponent();
        }
    }
}
