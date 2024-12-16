using Controller.Helpers;
using Controller.Models;
using Controller.Services;
using System.Diagnostics;

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
            
            if (!_isRunning) TCPService.Start(12000, MessageHandler);
            else TCPService.Stop();

            _isRunning = !_isRunning;
        }

        private static Task MessageHandler(IClient client, string messageType, object payload)
        {
            switch (messageType.ToLower())
            {
                case MSGType.ScreenMSG:
                    var sMsg = (ScreenMSG)payload;
                    Debug.WriteLine($"ScreenMSG received: {sMsg.DesiredQuality} {sMsg.ToPresent}");
                    break;
            }

            return Task.CompletedTask;
        }
    }
}
