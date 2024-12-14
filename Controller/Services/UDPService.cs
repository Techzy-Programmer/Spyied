using Controller.Helpers;
using Controller.Models;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;

namespace Controller.Services
{
    public static class UDPService
    {
        public static event Action<ScreenFrameMsg>? ScreenFrameReceived;

        // ToDo: Implement a timeout or expiry mechanism for incomplete entries to prevent Memory leaks
        private readonly static ConcurrentDictionary<int, Tuple<UDPHeader, SlotList<byte[]>>> incomplete = [];
        private static CancellationTokenSource? cancelUDP;
        private static readonly int headerSize = 32;
        private static UdpClient? udpClient;
        private static bool isRunning;

        public static void ToggleUDPService(bool? toStop)
        {
            toStop ??= isRunning; // Toggle if null

            if ((toStop == true || udpClient != null) && isRunning)
            {
                cancelUDP?.Cancel();
                udpClient?.Close();
                udpClient = null;

                isRunning = false;
            }
            else if (toStop == false && !isRunning)
            {
                udpClient = new UdpClient(11000);
                cancelUDP = new CancellationTokenSource();
                Task.Run(() => ReceiveMessages(cancelUDP.Token));

                isRunning = true;
            }
        }

        private static async void ReceiveMessages(CancellationToken cancelTok)
        {
            try
            {
                while (udpClient != null)
                {
                    var rcvData = await udpClient.ReceiveAsync(cancelTok);
                    var data = rcvData.Buffer;

                    if (data == null || data.Length < headerSize)
                    {
                        Debug.WriteLine("Received data is null or too short");
                        continue;
                    }

                    var header = data[..headerSize];
                    var remainingData = data[headerSize..];
                    var msg = ParseHeader(header, remainingData);

                    if (!incomplete.TryGetValue(msg.UID, out var tuple))
                    {
                        var slot = new SlotList<byte[]>(msg.Count);
                        tuple = new Tuple<UDPHeader, SlotList<byte[]>>(msg, slot);

                        incomplete.AddOrUpdate(msg.UID, tuple, (key, oldValue) => tuple);
                    }

                    // Sequence should be in the range [0, Count)
                    tuple.Item2.AddAt(msg.Sequence, msg.Data!);

                    if (tuple.Item2.AreAllSlotsFilled())
                    {
                        var completeData = tuple.Item2.SelectMany(x => x).ToArray();
                        msg.Data = completeData;

                        InvokeEvent(msg);
                        incomplete.Remove(msg.UID, out _);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("UDP Receive Task was cancelled");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in UDP Receive Task: {ex.Message}");
            }
        }

        private static UDPHeader ParseHeader(byte[] header, byte[] src)
        {
            using var ms = new MemoryStream(header);
            using var br = new BinaryReader(ms);
            var type = br.ReadInt16();

            var baseMsg = new UDPHeader
            {
                Type = type,
                UID = br.ReadInt32(),
                Count = br.ReadInt16(),
                Sequence = br.ReadInt16(),
                DataSize = br.ReadInt32(),
            };

            baseMsg.Data = Decompress(src[..baseMsg.DataSize]);

            var msg = type switch
            {
                1 => new ScreenFrameMsg(baseMsg)
                {
                    X = br.ReadInt32(),
                    Y = br.ReadInt32(),
                    Width = br.ReadInt32(),
                    Height = br.ReadInt32(),
                    Quality = br.ReadInt32(),
                },
                _ => baseMsg,
            };

            return msg;
        }

        private static byte[] Decompress(byte[] compData)
        {
            using var compressedStream = new MemoryStream(compData);
            using var zipStream = new InflaterInputStream(compressedStream);
            using var resultStream = new MemoryStream();
            zipStream.CopyTo(resultStream);
            return resultStream.ToArray();
        }

        private static void InvokeEvent(UDPHeader udpHeader)
        {
            switch (udpHeader)
            {
                case ScreenFrameMsg screenFrameMsg:
                    ScreenFrameReceived?.Invoke(screenFrameMsg);
                    break;
            }
        }
    }
}
