namespace Payload.Codes
{
    public class UpdateInfo
    {
        public double Version { get; set; }
        public string UpdateURL { get; set; }
    }

    public class EndpointInfo
    {
        public bool IsServerRunning { get; set; }
        public string IPAddress { get; set; }
        public string Port { get; set; }
    }
}
