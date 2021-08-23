using System;
using System.Threading;
using Syncfusion.Licensing;
using System.Windows.Forms;

namespace Controller
{
    static class Program
    {
        private static readonly string LKey = "NDU4NzkxQDMxMzkyZTMxMmUzMEIyc1hYZ2xza29vakxPVUI1TUM3SFBjNi9mYWl2T1FDUHJHdHR3UFRLWHM9";

        [STAThread]
        static void Main()
        {
            Application.ThreadException += UIThreadException;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            AppDomain.CurrentDomain.UnhandledException += NUIUnhandledException;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            SyncfusionLicenseProvider.RegisterLicense(LKey);
            Application.Run(new Dashboard());
        }


        private static void NUIUnhandledException(object _, UnhandledExceptionEventArgs UEP)
        {
            try { HandleGlobalExeptions((Exception)UEP.ExceptionObject, 1); }
            catch (Exception EP) { HandleGlobalExeptions(EP, 0); }
            #pragma warning disable CS0618
            finally { Thread.CurrentThread.Suspend(); }
            #pragma warning restore CS0618
        }

        private static void UIThreadException(object _, ThreadExceptionEventArgs TEP)
        {
            try { HandleGlobalExeptions(TEP.Exception, 2); }
            catch (Exception EP) { HandleGlobalExeptions(EP, 0); }
        }

        private static void HandleGlobalExeptions(Exception EToHandle, int ExepType)
        {
            MessageBox.Show($"Check Below For Detailed Info :{Environment.NewLine}Type = {EToHandle.GetType().Name}{Environment.NewLine}Message = {EToHandle.Message}{Environment.NewLine}{Environment.NewLine}Stacktrace -------------{Environment.NewLine}{EToHandle.StackTrace}-------------", $"{(ExepType == 1 ? "Non-UI Exception Occured!" : ExepType == 2 ? "UI Exception Encountered" : "Fatal Error Found")}", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
