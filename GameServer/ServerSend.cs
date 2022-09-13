namespace GameServer
{
    class ServerSend
    {
        public static void SendPackage(int _tcpClient, string _msg)
        {
            using (Packet packet = new Packet((int)ServerPackets.sendPackage))
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
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                Server.Clients[i].Tcp.SendData(packet);
            }
        }

        private static void SendTCPDataToAll(int _exeptClient, Packet packet)
        {
            packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                if (i != _exeptClient)
                {
                    Server.Clients[i].Tcp.SendData(packet);
                }
            }
        }

        private static void SendUDPData(int toClient, Packet packet)
        {
            packet.WriteLength();
            Server.Clients[toClient].Udp.SendData(packet);
        }
        private static void SendUDPDataToAll(Packet packet)
        {
            packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                Server.Clients[i].Tcp.SendData(packet);
            }
        }

        private static void SendUDPDataToAll(int _exeptClient, Packet packet)
        {
            packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                if (i != _exeptClient)
                {
                    Server.Clients[i].Tcp.SendData(packet);
                }
            }
        }

        #region Packets
        public static void Welcome(int _tcpClient, string _msg)
        {
            using (Packet packet = new Packet((int)ServerPackets.welcome))
            {
                packet.Write(_msg);
                packet.Write(_tcpClient);

                SendTCPData(_tcpClient, packet);
            }
        }

        public static void SpawnPlayer(int toClient, Player player)
        {
            using (Packet packet = new Packet((int)ServerPackets.spawnPlayer))
            {
                packet.Write(player.Id);
                packet.Write(player.Username);
                packet.Write(player.Position);
                packet.Write(player.Rotation);

                SendTCPData(toClient, packet);
            }
        }

        #endregion
    }
}
