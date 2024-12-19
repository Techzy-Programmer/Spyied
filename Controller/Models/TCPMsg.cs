namespace Controller.Models
{
    public static class MSGType
    {
        public const string InitMSG = "init";
        public const string ScreenMSG = "screen";
    }

    public class MSG<T>
    {
        public required string Type { get; set; }
        public required T Payload { get; set; }
    }

    public class InitMSG
    {
        public int RAM { get; set; }
        public bool Is64Bit { get; set; }
        public int CPUCores { get; set; }
        public int DriveCount { get; set; }
        public required string PCName { get; set; }
        public required string Username { get; set; }
    }

    public class ScreenMSG
    {
        public bool ToPresent { get; set; }
        public uint DesiredQuality { get; set; }
    }
}
