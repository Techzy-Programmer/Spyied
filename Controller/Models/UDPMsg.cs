namespace Controller.Models
{
    public class UDPHeader
    {
        public int UID { get; set; }
        public int Type { get; set; }
        public int Count { get; set; }
        public byte[]? Data { get; set; }
        public int Sequence { get; set; }
        public int DataSize { get; set; }
        public int Reserved { get; set; }
    }

    public class ScreenFrameMsg : UDPHeader
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Quality { get; set; }

        public ScreenFrameMsg(UDPHeader baseMsg)
        {
            UID = baseMsg.UID;
            Data = baseMsg.Data;
            Count = baseMsg.Count;
            Sequence = baseMsg.Sequence;
            DataSize = baseMsg.DataSize;
            Reserved = baseMsg.Reserved;
        }
    }
}
