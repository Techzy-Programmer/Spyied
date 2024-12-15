using Controller.Services;

namespace Controller.ViewModels.Pages
{
    public partial class DashboardViewModel : ObservableObject
    {
        private bool _isRunning = false;

        [ObservableProperty]
        private string _status = "Start Server";

        [RelayCommand]
        private void OnToggleServer() {
            Status = (_isRunning ? "Start" : "Stop") + " Server";
            UDPService.ToggleUDPService(null);
            _isRunning = !_isRunning;
        }
    }
}
