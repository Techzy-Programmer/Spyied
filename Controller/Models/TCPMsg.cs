namespace Controller.Models
{
    public static class MSGType
    {
        public const string ScreenMSG = "screen";
    }

    public class MSG<T>
    {
        public required string Type { get; set; }
        public required T Payload { get; set; }
    }

    public class ScreenMSG
    {
        public bool ToPresent { get; set; }
        public uint DesiredQuality { get; set; }
    }
}
