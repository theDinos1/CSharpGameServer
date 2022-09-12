using System.Net;
using System.Net.Sockets;

namespace GameServer
{
    class Client
    {
        public static int DataBufferSize = 4096;
        public int Id;
        public TCP Tcp;
        public UDP Udp;

        public Client(int _id)
        {
            Id = _id;
            Tcp = new TCP(Id);
            Udp = new UDP(Id);
        }

        public class TCP
        {
            public TcpClient Socket;
            private readonly int _Id;
            private NetworkStream _Stream;
            private byte[] _RecieveBuffer;
            private Packet _ReceivedData;
            public TCP(int id)
            {
                _Id = id;
            }

            public void Connect(TcpClient _tcpClient)
            {
                Socket = _tcpClient;
                Socket.ReceiveBufferSize = DataBufferSize;
                Socket.SendBufferSize = DataBufferSize;

                _Stream = Socket.GetStream();

                _ReceivedData = new Packet();
                _RecieveBuffer = new byte[DataBufferSize];

                _Stream.BeginRead(_RecieveBuffer, 0, DataBufferSize, RecieveCallback, null);

                ServerSend.Welcome(_Id, "Welcome to server!");
                ServerSend.SendPackage(_Id, "Kuy rai i sus");
            }
            public void SendData(Packet _packet)
            {
                try
                {
                    if (Socket != null)
                    {
                        _Stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending data to player {_Id} via TCP: {ex}");
                }
            }
            private void RecieveCallback(IAsyncResult _result)
            {
                try
                {
                    int byteLength = _Stream.EndRead(_result);
                    if (byteLength <= 0)
                    {
                        // TODO: disconnect
                        return;
                    }

                    byte[] data = new byte[byteLength];
                    Array.Copy(_RecieveBuffer, data, byteLength);
                    
                    _ReceivedData.Reset(HandleData(data));
                    Packet packet = new Packet(data);
                    Console.WriteLine($"Welcome to Server {packet.ReadString()}!");

                    _Stream.BeginRead(_RecieveBuffer, 0, DataBufferSize, RecieveCallback, null);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error recieving TCP data: {ex}");
                    // TODO: disconnect
                }
            }
            private bool HandleData(byte[] _data)
            {
                int packetLength = 0;

                _ReceivedData.SetBytes(_data);

                if (_ReceivedData.UnreadLength() >= 4)
                {
                    packetLength = _ReceivedData.ReadInt();
                    if (packetLength <= 0)
                    {
                        return true;
                    }
                }

                while (packetLength > 0 && packetLength <= _ReceivedData.UnreadLength())
                {
                    byte[] packetBytes = _ReceivedData.ReadBytes(packetLength);
                    ThreadManager.ExecuteOnMainThread(() =>
                    {
                        using (Packet packet = new Packet(packetBytes))
                        {
                            int packetId = packet.ReadInt();
                            Console.WriteLine($"Received package id: {packetId}");
                            Server.PacketHandlers[packetId](_Id, packet);
                        }
                    });

                    packetLength = 0;
                    if (_ReceivedData.UnreadLength() >= 4)
                    {
                        packetLength = _ReceivedData.ReadInt();
                        if (packetLength <= 0)
                        {
                            return true;
                        }
                    }
                }
                if (packetLength <= 1)
                {
                    return true;
                }

                return false;
            }
        }
        public class UDP
        {
            public IPEndPoint EndPoint;
            private int _Id;

            public UDP(int id)
            {
                _Id = id;
            }

            public void Connect(IPEndPoint endPoint)
            {
                EndPoint = endPoint;
                ServerSend.UdpTest(_Id);
            }

            public void SendData(Packet packet)
            {
                Server.SendUDPData(EndPoint, packet);
            }

            public void HandleData(Packet packet)
            {
                int packetLength = packet.ReadInt();
                byte[] packetBytes = packet.ReadBytes(packetLength);

                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet packet = new Packet(packetBytes))
                    {
                        int packetId = packet.ReadInt();
                        Console.WriteLine($"packet id: {packetId}");
                        Server.PacketHandlers[packetId](_Id, packet);
                    }
                });
            }
        }
    }
}
