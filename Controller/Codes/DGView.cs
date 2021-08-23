using System;
using EasyTcp4;
using System.Drawing;
using Controller.Server;
using Controller.ActionClass;

namespace Controller.Codes
{
    public class Victim
    {
        private string TGLStatus = "N/A";

        public long ID { get; private set; }
        public string PCName { get; private set; }
        public string Antivirus { get; private set; }
        public string IPAddress { get; private set; }
        public byte[] Country { get; private set; }
        public string Timestamp { get; private set; }
        
        public string ToggleStatus
        {
            get => TGLStatus;
            set
            {
                if (TGLStatus != value)
                {
                    TGLStatus = value;
                    TargetV.VictimList.RefreshUI();
                }
            }
        }

        public VLocation LocDet { get; private set; }
        public VPCDetails PCDet { get; private set; }
        public EasyTcpClient NetComunicator { get; private set; }

        public Victim(VictimBase VBase, EasyTcpClient VClient)
        {
            PCDet = VBase.SysInfo;
            PCName = PCDet.PCName;
            LocDet = VBase.Location;
            NetComunicator = VClient;
            ID = TargetV.VictimsIDs++;
            IPAddress = LocDet.IPQuery;
            Antivirus = PCDet.Antivirus;
            var ResManager = Properties.Resources.ResourceManager;
            Timestamp = System.DateTime.Now.ToString("ddd, dd-MM-yy ~ hh:mm:ss tt");
            var FImg = (Image)ResManager.GetObject(LocDet.CountryCode.ToLower());
            if (FImg == null) Country = ImageToByteArray(Properties.Resources.FNull);
            else Country = ImageToByteArray(FImg);
            ToggleStatus = "Control Me";
        }

        public byte[] ImageToByteArray(Image ImgIn)
        {
            var ITBMS = new System.IO.MemoryStream();
            ImgIn.Save(ITBMS, System.Drawing.Imaging.ImageFormat.Bmp);
            return ITBMS.ToArray();
        }
    }

    public class PendingTask
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public string Details { get; set; }
        public string Initiated { get; set; }

        public PendingTask(long _ID, string _TDet)
        {
            ID = _ID;
            Status = "Pending";
            Name = _TDet.Split('|')[0] ?? "Not Available";
            Initiated = DateTime.Now.ToString("ddd hh:mm:ss tt");
            Details = _TDet.Split('|')[1] ?? "Task Description Unavailable.";
        }

        public void UpdateStatus(string _Status)
        {
            Status = _Status;
            Tasker.PendingTasks.RefreshUI();
        }
    }

    public class CompletedTask
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public string Ended { get; set; }
        public string Status { get; set; }
        public string Initiated { get; set; }
        public Exception FailError { get; set; }

        public CompletedTask(PendingTask _PrevP, Exception ExceptionErO)
        {
            ID = _PrevP.ID;
            Name = _PrevP.Name;
            Status = _PrevP.Status;
            FailError = ExceptionErO;
            Initiated = _PrevP.Initiated;
            Ended = DateTime.Now.ToString("ddd hh:mm:ss tt");
        }
    }
}