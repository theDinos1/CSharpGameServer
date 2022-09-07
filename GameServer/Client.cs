using System.Net.Sockets;

namespace GameServer
{
    class Client
    {
        public static int DataBufferSize = 4096;
        public int Id;
        public TCP Tcp;

        public Client(int _id)
        {
            Id = _id;
            Tcp = new TCP(Id);
        }

        public class TCP
        {
            public TcpClient Socket;
            private readonly int _Id;
            private NetworkStream _Stream;
            private byte[] _RecieveBuffer;
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

                _RecieveBuffer = new byte[DataBufferSize];

                _Stream.BeginRead(_RecieveBuffer, 0, DataBufferSize, RecieveCallback, null);
                
                ServerSend.Welcome(_Id, "Welcome to server!");
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
                    // TODO: handle data

                    _Stream.BeginRead(_RecieveBuffer, 0, DataBufferSize, RecieveCallback, null);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error recieving TCP data: {ex}");
                    // TODO: disconnect
                }
            }
        }
    }
}
