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
        private static UdpClient _UdpListener;

        public delegate void PacketHandler(int _fromClient, Packet _packet);
        public static Dictionary<int, PacketHandler> PacketHandlers;

        public static void Start(int _maxPlayers, int _port)
        {
            MaxPlayers = _maxPlayers;
            Port = _port;

            Console.WriteLine("Starting Server...");
            InitializeServerData();

            _TcpListener = new TcpListener(IPAddress.Any, Port);
            _TcpListener.Start();
            _TcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

            _UdpListener = new UdpClient(Port);
            _UdpListener.BeginReceive(UDPReceiveCallback, null);

            Console.WriteLine($"Server started on {Port}");

        }

        private static void TCPConnectCallback(IAsyncResult result)
        {
            TcpClient client = _TcpListener.EndAcceptTcpClient(result);
            _TcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

            Console.WriteLine($"incoming connection from {client.Client.RemoteEndPoint}");

            for (int i = 1; i <= MaxPlayers; i++)
            {
                if (Clients[i].Tcp.Socket == null)
                {
                    Clients[i].Tcp.Connect(client);
                    return;
                }
            }

            Console.WriteLine($"{client.Client.RemoteEndPoint} failed to connect: Server full");
        }

        private static void UDPReceiveCallback(IAsyncResult result)
        {
            try
            {
                IPEndPoint clientEndpoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = _UdpListener.EndReceive(result, ref clientEndpoint);
                Console.WriteLine($"UDP Receive from {clientEndpoint}");
                _UdpListener.BeginReceive(UDPReceiveCallback, null);

                if (data.Length < 4)
                {
                    Console.WriteLine("UDP receive callback return cause data length is less than 4");
                    return;
                }

                using (Packet packet = new Packet(data))
                {
                    int clientId = packet.ReadInt();

                    if (clientId == 0)
                    {
                        Console.WriteLine("UDP receive callback return cause client id is zero");
                        return;
                    }

                    if (Clients[clientId].Udp.EndPoint == null)
                    {
                        Clients[clientId].Udp.Connect(clientEndpoint);
                        return;
                    }

                    if (Clients[clientId].Udp.EndPoint.Equals(clientEndpoint))
                    {
                        Clients[clientId].Udp.HandleData(packet);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error receiving UDP data: {ex}");
            }
        }

        public static void SendUDPData(IPEndPoint clientEndPoint, Packet packet)
        {
            try
            {
                if (clientEndPoint != null)
                {
                    _UdpListener.BeginSend(packet.ToArray(), packet.Length(), clientEndPoint, null, null);
                    int packageId = new Packet(packet.ReadBytes(packet.ReadInt())).ReadInt();
                    Console.WriteLine($"Udp data Sended, package id: {packageId} to {clientEndPoint}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending data to {clientEndPoint} via UDP: {ex}");
            }
        }

        private static void InitializeServerData()
        {
            for (int i = 1; i <= MaxPlayers; i++)
            {
                Clients.Add(i, new Client(i));
            }
            PacketHandlers = new Dictionary<int, PacketHandler>()
            {
                { (int)ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived },
                { (int)ClientPackets.udpTestReceived, ServerHandle.UDPTestReceived },
            };
        }
    }
}