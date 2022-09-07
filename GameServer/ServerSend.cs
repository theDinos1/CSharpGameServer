using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    class ServerSend
    {

        public static void Welcome(int _tcpClient, string _msg)
        {
            using (Packet packet = new Packet((int)ServerPackets.welcome))
            {
                packet.Write(_msg);
                packet.Write(_tcpClient);

                SendTCPData(_tcpClient, packet);
            }
        }

        private static void SendTCPData(int tcpClient, Packet packet)
        {
            packet.WriteLength();
            Server.Clients[tcpClient].Tcp.SendData(packet);
        }
        private static void SendTCPDataToAll(Packet packet)
        {
            packet.WriteLength();
            for (int i = 0; i <= Server.MaxPlayers; i++)
            {
                Server.Clients[i].Tcp.SendData(packet);
            }
        }
        private static void SendTCPDataToAll(int _exeptClient, Packet packet)
        {
            packet.WriteLength();
            for (int i = 0; i <= Server.MaxPlayers; i++)
            {
                if (i != _exeptClient)
                {
                    Server.Clients[i].Tcp.SendData(packet);
                }
            }
        }
    }
}
