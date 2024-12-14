namespace Controller.ViewModels.Pages
{
    public partial class DashboardViewModel : ObservableObject
    {
        private bool _isRunning = false;

        [ObservableProperty]
        private string _status = "Start Server";

        [RelayCommand]
        private void OnToggleServer() {
            if (_isRunning)
            {
                return;
            }

        }
    }
}
