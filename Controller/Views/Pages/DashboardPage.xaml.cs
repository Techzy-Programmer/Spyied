using Controller.Models;
using Controller.Services;
using Controller.ViewModels.Pages;
using Wpf.Ui.Controls;

namespace Controller.Views.Pages
{
    public partial class DashboardPage : INavigableView<DashboardViewModel>
    {
        public DashboardViewModel ViewModel { get; }

        public DashboardPage(DashboardViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }

        private void ListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs _)
        {
            var victim = (Victim)((ListView)sender).SelectedItem;
            TCPService.SetTarget(victim);
        }
    }
}
