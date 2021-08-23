using Syncfusion.WinForms.DataGrid;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Net.NetworkInformation;

namespace Controller.Codes
{
    public static class SUtils
    {
        private static Form Owner;
        private static readonly string[] HostToPing = new string[2];
        private static bool HasInitialsSet = false, IsNetAvailable = false;
        private static readonly Dictionary<Control, string> ICtrlsTxt = new Dictionary<Control, string>();

        public static event InternetChanged OnInternetChanged;
        public delegate void InternetChanged(bool IsConnected);

        public static event DoUIEvents OnDoUIEvents;
        public delegate void DoUIEvents(string EType, object Data = null);

        public static string WorkDir
            { get => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Spyied-Controller\"; }

        public static void SetInitials(Form _Owner)
        {
            if (HasInitialsSet) return;

            HostToPing[0] = "www.msftncsi.com";
            HostToPing[1] = "www.msftconnecttest.com";
            Owner = _Owner; EnqueueThread(delegate () { LoopInternetCheck(); });
            foreach (Label Lbl in GetAllChild(Owner.Controls).OfType<Label>()) ICtrlsTxt.Add(Lbl, Lbl.Text);
            foreach (Button Btn in GetAllChild(Owner.Controls).OfType<Button>()) ICtrlsTxt.Add(Btn, Btn.Text);
            foreach (ComboBox CBox in GetAllChild(Owner.Controls).OfType<ComboBox>()) CBox.SelectedIndex = 0;
            HasInitialsSet = true;
        }

        public static void ResetToInitials()
        {
            string[] Prevented = new string[] { "btStartSvr", "lpConnectionStatus" };

            if (!Owner.IsDisposed && Owner.IsHandleCreated)
            {
                if (Owner.InvokeRequired) Owner.Invoke(new MethodInvoker(delegate () { ResetToInitials(); }));
                else
                {
                    foreach (var LKPr in ICtrlsTxt.ToArray())
                    {
                        if (!LKPr.Key.IsDisposed && LKPr.Key.IsHandleCreated)
                        {
                            if (Prevented.Contains(LKPr.Key.Name)) continue;
                            else LKPr.Key.Text = LKPr.Value;
                        }
                    }

                    foreach (TextBox TBox in GetAllChild(Owner.Controls).OfType<TextBox>()) TBox.Text = string.Empty;
                }
            }
        }
                
        public static void EnqueueThread(ThreadStart IMethod) => new Thread(IMethod) { IsBackground = true }.Start();

        public static async Task<bool> IsNetConnected()
        {
            int Count = -1; ReCheck:; Count++;
            if (await HasPinged(HostToPing[Count])) return true;
            else if (Count < 1) goto ReCheck;
            else return false;
        }

        public static void InstructUI(string EType, object Data = null) => OnDoUIEvents?.Invoke(EType, Data);

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
            try { using (var NPinger = new Ping())
                    PReply = await NPinger.SendPingAsync(Host, 2000, new byte[32]); } catch { }
            return PReply != null && PReply.Status == 0;
        }

        private static IEnumerable<Control> GetAllChild(Control.ControlCollection Ctrls)
        {
            foreach (Control Ctrl in Ctrls)
            {
                foreach (Control GChild in GetAllChild(Ctrl.Controls)) yield return GChild;
                yield return Ctrl;
            }
        }
    }

    public class ADVCollection<T>
    {
        private readonly bool Scrollable;
        private SfDataGrid _DGrid = null;
        private readonly ObservableCollection<T> Holder = new ObservableCollection<T>();

        public T this[int Index] { get => Holder[Index]; set => Holder[Index] = value; }

        public ADVCollection(SfDataGrid DGrid) => SetDatagridView(DGrid);
        public ADVCollection(bool ShouldScroll) => Scrollable = ShouldScroll;

        public void SetDatagridView(SfDataGrid DGrid)
        {
            _DGrid = DGrid;
            _DGrid.DataSource = Holder;
        }

        public T[] ToArray() { var Temp = Holder; return Temp.ToArray(); }
        public void RemoveAt(int Index) => Holder.RemoveAt(Index);
        public void Remove(T RItem) => Holder.Remove(RItem);
        public void Clear() => Holder.Clear();
        public int Count() => Holder.Count;

        public void Add(T NewV)
        {
            Holder.Add(NewV);

            if (Scrollable)
            {
                if (_DGrid != null && !_DGrid.IsDisposed && _DGrid.IsHandleCreated)
                {
                    _DGrid.Invoke(new MethodInvoker(delegate ()
                    {
                        _DGrid.TableControl.ScrollRows.ScrollInView(_DGrid.TableControl.ScrollRows.LineCount);
                        RefreshUI();
                    }));
                }
            }
        }

        public void RefreshUI()
        {
            if (_DGrid != null && !_DGrid.IsDisposed && _DGrid.IsHandleCreated)
            {
                if (_DGrid.InvokeRequired) _DGrid.Invoke(new MethodInvoker(delegate () { RefreshUI(); }));
                else
                {
                    int SelColumn = _DGrid.CurrentCell.ColumnIndex;
                    int SelRow = _DGrid.SelectedIndex + 1;
                    var PrevSelMode = _DGrid.SelectionMode;
                    var Mode = Syncfusion.WinForms.DataGrid.Enums.GridSelectionMode.Multiple;
                    _DGrid.SelectionMode = Mode; _DGrid.SelectAll();
                    _DGrid.SelectionMode = PrevSelMode; _DGrid.ClearSelection();
                    _DGrid.MoveToCurrentCell(new Syncfusion.WinForms.GridCommon.ScrollAxis.RowColumnIndex(SelRow, SelColumn));
                }
            }
        }
    }

    public static class StreamAddON
    {
        public static async Task PushStreamAsync(this EasyTcp4.EasyTcpClient RcvClient, Stream SndStream, Func<decimal, int> ProgressCallback = null)
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

        public static async Task PullStreamAsync(this EasyTcp4.Message MRcv, Stream RcvStream, Func<decimal, int> ProgressCallback = null)
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
