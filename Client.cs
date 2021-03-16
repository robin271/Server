using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerForRunningGame
{
    public class Client
    {
        public static int dataBufferSize = 4096;

        public TcpClient socket;

        private readonly int id;
        int ServerVariant;
        private NetworkStream stream;
        private Packet receivedData;
        private byte[] receiveBuffer;
        private delegate void PacketHandler(Packet _packet);
        
        readonly Dictionary<int, PacketHandler> packetHandler;
        Matchmaker.DedicatedServer dS;
        readonly IPAddress IP;

        public Client(int _id, TcpClient _socket)
        {
            
            id = _id;
            socket = _socket;
            socket.ReceiveBufferSize = dataBufferSize;
            socket.SendBufferSize = dataBufferSize;

            stream = socket.GetStream();

            receivedData = new Packet();
            receiveBuffer = new byte[dataBufferSize];
            packetHandler = new Dictionary<int, PacketHandler>()
            {
                {(int)ClientPackets.MaxPlayer,AssigningServer },
            };
            IPEndPoint ipep = (IPEndPoint)socket.Client.RemoteEndPoint;
            IP = ipep.Address;
            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            Console.WriteLine("Added Client: " + socket.Client.RemoteEndPoint+ "\n");
            
        }
        void AssigningServer(Packet _packet)
        {
            Packet packet = new Packet((int)ServerPackets.ServerIP);
            ServerVariant = _packet.ReadInt() - 2;
            dS = Matchmaker.dedicatedServer[ServerVariant];
            if (dS.ip != null && dS.maxPlayer > ++dS.player)
            {
                packet.Write(true) ;
                var IPBytes = dS.ip.GetAddressBytes();
                packet.Write(IPBytes.Length);
                packet.Write(IPBytes);
                Console.WriteLine($"{socket.Client.RemoteEndPoint} connected to {dS.ip}");
            }
            else
            {
                packet.Write(false);
                Matchmaker.dedicatedServer[ServerVariant] = new Matchmaker.DedicatedServer(ServerVariant + 2)
                {
                    ip = IP,
                    HostID = id,
                };
                Console.WriteLine($"Has become a local Server {socket.Client.RemoteEndPoint}");
            }
            stream.Write(packet.ToArray(), 0, packet.Length());
        }
        public void SendData(Packet _packet)
        {
            try
            {
                if (socket != null)
                {
                    stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                }
            }
            catch (Exception _ex)
            {
                Console.WriteLine($"Error sending data to player {id} via TCP: {_ex}");
            }
        }
        private void ReceiveCallback(IAsyncResult _result)
        {
                try
                {
                int _byteLength = stream.EndRead(_result);
                if (_byteLength <= 0)
                {
                    Matchmaker.Disconnect(id);
                    return;
                }

                byte[] _data = new byte[_byteLength];
                Array.Copy(receiveBuffer, _data, _byteLength);
                receivedData.Reset(HandleData(_data));
                if (stream == null)
                    return;
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }
            catch (Exception _ex)
            {
                Console.WriteLine($"Error receiving TCP data: {_ex}");
                Matchmaker.Disconnect(id) ;
            }
        }
        private bool HandleData(byte[] _data)
        {
            receivedData.SetBytes(_data);
            if (receivedData.UnreadLength() >= 4)
            {
                int packetID = receivedData.ReadInt();
                packetHandler[packetID](receivedData);
            }
                return true;

        }
        public void Disconnect()
        {
            Console.WriteLine($"{socket.Client.RemoteEndPoint} has disconnected.");
            Console.WriteLine("");
            socket.Close();
            stream = null;
            receivedData = null;
            receiveBuffer = null;
            socket = null;
            dS.player--;
           
        }
        void print(string value)
        {
            Console.WriteLine(value);
        }
    }
    
}

