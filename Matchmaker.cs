
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ServerForRunningGame
{
    public class Matchmaker
    {
        #region Declarations
        private static TcpListener tcpListener;
        static Packet _packet;
        static Stream _stream;

        readonly static int _maxPlayer = 100,Port=26950;
        static int _clientNumber;
        public class DedicatedServer { public IPAddress ip;public int player, maxPlayer,HostID;
            public DedicatedServer(int _maxPlayer) { maxPlayer = _maxPlayer; }
        }
        
        public static DedicatedServer[] dedicatedServer = new DedicatedServer[3];
        public static Client[] clients = new Client[_maxPlayer];

        #endregion
        public static void Start()
        {
           
            tcpListener = new TcpListener(IPAddress.Any, Port);
   
            //Starts listening to incoming connections
            tcpListener.Start();
            //Calls at the end of process TCPConnectCalllback, and passes "null"
            tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);
           Console.WriteLine("Matchmaker: "+tcpListener.LocalEndpoint);
            for (int i = 0; i < dedicatedServer.Length; i++)
            {
                dedicatedServer[i] = new DedicatedServer(i + 2);
            }
        }
        private static void TCPConnectCallback(IAsyncResult _result)
        {

            TcpClient _client = tcpListener.EndAcceptTcpClient(_result);
            if (_clientNumber < _maxPlayer)
                tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);
            else Console.WriteLine("Matchmaker full!");
            clients[_clientNumber] = new Client(_clientNumber++,_client);
                        
            


        }
        public static void Disconnect(int _id)
        {
            
            for (int i = 0; i < dedicatedServer.Length; i++)
            {
                
                if (dedicatedServer[i].HostID == _id)
                {
                    dedicatedServer[i].ip = null;
                    dedicatedServer[i].player = 0;
                }
            }
            clients[_id].Disconnect();

            for (int i = _id; i < _clientNumber-1; i++)
            {
                clients[i] = clients[i + 1];
            }
            //not yet tested
            clients[_clientNumber] = null;
            if (_clientNumber-- == 100)
            {
                tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);
            }
            
        }



    }
}
