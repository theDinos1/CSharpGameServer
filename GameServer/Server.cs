using System.Net;
using System.Net.Sockets;
namespace GameServer
{
    class Server
    {
        public static int MaxPlayers { get; private set; }
        public static int Port { get; private set; }
        public static Dictionary<int, Client> Clients = new Dictionary<int, Client>();
        private static TcpListener _TcpListener;

        public static void Start(int _maxPlayers, int _port)
        {
            MaxPlayers = _maxPlayers;
            Port = _port;

            Console.WriteLine("Starting Server...");
            InitializeServerData();

            _TcpListener = new TcpListener(IPAddress.Any, Port);
            _TcpListener.Start();
            _TcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

            Console.WriteLine($"Server started on {Port}");

        }

        private static void TCPConnectCallback(IAsyncResult _result)
        {
            TcpClient client = _TcpListener.EndAcceptTcpClient(_result);
            _TcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

            Console.WriteLine($"incoming connection from {client.Client.RemoteEndPoint}");

            for (int i = 0; i <= MaxPlayers; i++)
            {
                if(Clients[i].Tcp.Socket == null)
                {
                    Clients[i].Tcp.Connect(client);
                    return;
                }
            }

            Console.WriteLine($"{client.Client.RemoteEndPoint} failed to connect: Server full");
        }

        private static void InitializeServerData()
        {
            for (int i = 0; i <= MaxPlayers; i++)
            {
                Clients.Add(i, new Client(i));
            }
        }
    }
}