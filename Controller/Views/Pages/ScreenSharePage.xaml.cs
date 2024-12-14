using Controller.ViewModels.Pages;
using Wpf.Ui.Controls;

namespace Controller.Views.Pages
{
    public partial class ScreenSharePage : INavigableView<ScreenShareViewModel>
    {
        public ScreenShareViewModel ViewModel { get; }

        public ScreenSharePage(ScreenShareViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}
