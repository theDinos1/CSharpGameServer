namespace GameServer
{
    class ServerHandle
    {
        public static void WelcomeReceived(int fromClient, Packet packet)
        {
            int _clientIdCheck = packet.ReadInt();
            string _username = packet.ReadString();

            Console.WriteLine($"{Server.Clients[fromClient].Tcp.Socket.Client.RemoteEndPoint} connected successfully and is now player {fromClient}.");
            if (fromClient != _clientIdCheck)
            {
                Console.WriteLine($"Player \"{_username}\" (ID: {fromClient}) has assumed the wrong client ID ({_clientIdCheck})!");
            }
            Server.Clients[fromClient].SendIntoGame(_username);
        }
    }
}