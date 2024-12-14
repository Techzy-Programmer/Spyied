using Wpf.Ui.Controls;

namespace Controller.ViewModels.Pages
{
    public partial class ScreenShareViewModel : ObservableObject, INavigationAware
    {
        [ObservableProperty]
        private string _appVersion = String.Empty;

        public void OnNavigatedFrom()
        {
            // ToDo: Send stop screen share command
        }

        public void OnNavigatedTo()
        {
            // ToDo: Send start screen share command
        }
    }
}
