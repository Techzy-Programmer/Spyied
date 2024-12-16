using Controller.Models;
using Controller.Services;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Wpf.Ui.Controls;

namespace Controller.ViewModels.Pages
{
    public partial class ScreenShareViewModel : ObservableObject, INavigationAware
    {
        [ObservableProperty]
        private WriteableBitmap? _screenBitmap;

        private byte[]? screenBuffer;
        private int stride;

        [ObservableProperty]
        private string _appVersion = String.Empty;

        public void OnNavigatedFrom()
        {
            TCPService.SendAsync(MSGType.ScreenMSG, new ScreenMSG { DesiredQuality = 0, ToPresent = false });
            UDPService.ScreenFrameReceived -= OnScreenFrameReceived;
            ScreenBitmap = null;
        }

        public void OnNavigatedTo()
        {
            TCPService.SendAsync(MSGType.ScreenMSG, new ScreenMSG { DesiredQuality = 1, ToPresent = true });
            UDPService.ScreenFrameReceived += OnScreenFrameReceived;
        }

        private void OnScreenFrameReceived(ScreenFrameMsg msg)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (ScreenBitmap == null && msg.X == 0 && msg.Y == 0)
                {
                    Debug.WriteLine($"Creating new WriteableBitmap with size: {msg.Width}x{msg.Height}");
                    ScreenBitmap = new WriteableBitmap(msg.Width, msg.Height, 96, 96, PixelFormats.Bgra32, null);
                    screenBuffer = new byte[msg.Width * msg.Height * 4];
                    stride = ScreenBitmap.BackBufferStride;
                }
            });

            Task.Run(() =>
            {
                try
                {
                    byte[] localBuffer = new byte[screenBuffer!.Length];
                    Array.Copy(screenBuffer, localBuffer, screenBuffer.Length);

                    for (int row = 0; row < msg.Height; row++)
                    {
                        var sourceIndex = row * msg.Width * 4;
                        var targetIndex = ((msg.Y + row) * stride) + (msg.X * 4);
                        Buffer.BlockCopy(msg.Data ?? [], sourceIndex, localBuffer, targetIndex, msg.Width * 4);
                    }

                    msg.Data = ConvertRgbaToBgra(msg.Data!);

                    // Perform updation on UI thread
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        try
                        {
                            ScreenBitmap?.Lock();
                            ScreenBitmap?.WritePixels(
                                new Int32Rect(msg.X, msg.Y, msg.Width, msg.Height),
                                msg.Data ?? [],
                                msg.Width * 4,
                                0);
                        }
                        finally
                        {
                            ScreenBitmap?.Unlock();
                        }
                    });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error processing frame: {ex.Message}");
                }
            });
        }

        private static byte[] ConvertRgbaToBgra(byte[] rgbaBuffer)
        {
            for (int i = 0; i < rgbaBuffer.Length; i += 4)
                (rgbaBuffer[i + 2], rgbaBuffer[i]) = (rgbaBuffer[i], rgbaBuffer[i + 2]);

            return rgbaBuffer;
        }
    }
}
