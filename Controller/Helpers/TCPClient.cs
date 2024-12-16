using Controller.Models;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Controller.Helpers
{

    public interface IClient
    {
        Task SendMessageAsync<T>(string messageType, T payload);
        NetworkStream Stream { get; }
        void DisconnectAsync();
        string Id { get; }
    }

    public class TCPClient : IClient
    {
        private readonly TcpClient client;
        private readonly JsonSerializerOptions jsonOptions;

        public string Id { get; }

        public NetworkStream Stream { get; }

        public TCPClient(TcpClient client)
        {
            this.client = client;
            Stream = client.GetStream();
            Id = Guid.NewGuid().ToString();
            jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
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

    public class ClientManager
    {
        private readonly Dictionary<string, IClient> clients = [];
        private readonly Lock clientLock = new();

        public void AddClient(IClient client)
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

        public IEnumerable<IClient> GetAllClients()
        {
            lock (clientLock)
            {
                return [.. clients.Values];
            }
        }
    }

}
