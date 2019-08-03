using Kasay.DependencyProperty;
using FluentMusic.ViewModels;
using ReactiveUI;
using Windows.UI.Xaml.Controls;

namespace FluentMusic.Views
{
    public sealed partial class FullPlayerPage : Page, IViewFor<FullPlayerPageViewModel>
    {
        [Bind]
        public FullPlayerPageViewModel ViewModel { get; set; }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (FullPlayerPageViewModel)value;
        }

        public FullPlayerPage()
        {
            ViewModel = new FullPlayerPageViewModel();

            InitializeComponent();

            this.WhenActivated(disposables => { });
        }
    }
}
