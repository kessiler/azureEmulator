using System;
using SharpNetty;
using System.Collections.Specialized;
using System.Net;
using System.Net.Sockets;
using ConnectionManager;
using ConnectionManager.Socket_Exceptions;
using SharedPacketLib;
using System.Collections.Generic;

namespace Mercury.Connection.Connection
{

    public class SocketManager
    {
        protected int acceptedConnections;
        //protected Socket connectionListener;
        protected int portInformation;
        protected bool acceptConnections;
        protected IDataParser parser;
        protected bool disableNagleAlgorithm;

        public delegate void ConnectionEvent(ConnectionInformation _connection);

        public event ConnectionEvent connectionEvent;

        public int MaximumConnections { get; set; }

        public int MaxIpConnectionCount { get; set; }

        public HybridDictionary IpConnectionCount { get; set; }

        public HybridDictionary ActiveConnections { get; set; }

        protected NettyServer nettyServer;

        protected Dictionary<int, Client> clients;

        public void Handle_NewConnection(int socketIndex)
        {
            clients.Add(socketIndex, new Client(nettyServer.GetConnection(socketIndex)));
        }

        public void Handle_LostConnection(int socketIndex)
        {
            Console.WriteLine("Lost connection with " + this.clients[socketIndex].IP);
            this.clients.Remove(socketIndex);
        }

        public void Init(int portID, int maxConnections, int connectionsPerIP, IDataParser parser, bool disableNaglesAlgorithm)
        {
            clients = new Dictionary<int, Client>();
            this.parser = parser;
            this.parser = parser.Clone() as IDataParser;
            this.InitializeFields();
            this.MaximumConnections = maxConnections;
            this.portInformation = portID;
            this.MaxIpConnectionCount = connectionsPerIP;
            this.acceptedConnections = 0;
            this.PrepareConnectionDetails();
        }

        public void InitializeConnectionRequests()
        {
            this.acceptConnections = true;
            string hostName = Dns.GetHostName();
            Dns.GetHostEntry(hostName);
            try
            {
                nettyServer.Handle_NewConnection = this.Handle_NewConnection;
                nettyServer.Handle_LostConnection = this.Handle_LostConnection;
                nettyServer.Listen(100, MaximumConnections);
            }
            catch
            {
                this.Destroy();
            }
        }

        public void Destroy()
        {
            this.acceptConnections = false;
            try
            {
                this.nettyServer.StopListening();
            }
            catch
            {

            }
            this.nettyServer = null;
        }

        public void ReportDisconnect(ConnectionInformation gameConnection)
        {
            gameConnection.connectionChanged -= new ConnectionInformation.ConnectionChange(this.CConnectionChanged);
            this.reportUserLogout(gameConnection.getIp());
        }

        public int getAcceptedConnections()
        {
            return this.acceptedConnections;
        }

        internal bool isConnected()
        {
            return this.nettyServer != null;
        }

        protected void InitializeFields()
        {
            this.ActiveConnections = new HybridDictionary();
            this.IpConnectionCount = new HybridDictionary();
        }

        protected void PrepareConnectionDetails()
        {
            IPAddress Ip = IPAddress.Any;
            string ip = Ip.ToString();
            int port = this.portInformation;
            this.clients = new Dictionary<int, Client>();
            this.nettyServer = new NettyServer(true);
            try
            {
                nettyServer.BindSocket(ip, port);
            }
            catch (SocketException ex)
            {
                throw new SocketInitializationException(ex.Message);
            }
        }

        protected void CConnectionChanged(ConnectionInformation information, ConnectionState state)
        {
            if (state == ConnectionState.closed)
            {
                this.ReportDisconnect(information);
            }
        }

        protected void reportUserLogin(string ip)
        {
            this.alterIpConnectionCount(ip, checked(this.getAmountOfConnectionFromIp(ip) + 1));
        }

        protected void reportUserLogout(string ip)
        {
            this.alterIpConnectionCount(ip, checked(this.getAmountOfConnectionFromIp(ip) - 1));
        }

        protected void alterIpConnectionCount(string ip, int amount)
        {

        }

        protected int getAmountOfConnectionFromIp(string ip)
        {
            return 0;
        }

        protected class Client : SocketManager
        {

            private NettyServer.Connection _connection;

            public string IP { get; private set; }

            public Client(NettyServer.Connection iAr)
            {
                try
                {
                    _connection = iAr;
                    Socket thisSocket = _connection.Socket;
                    IP = thisSocket.RemoteEndPoint.ToString().Split(new char[]{':'})[0];
                    this.acceptedConnections++;
                    ConnectionInformation connectionInformation = new ConnectionInformation(thisSocket, this.acceptedConnections, this, this.parser, IP);
                    this.reportUserLogin(IP);
                    connectionInformation.connectionChanged += new ConnectionInformation.ConnectionChange(this.CConnectionChanged);
                    if (connectionEvent != null)
                    {
                        connectionEvent(connectionInformation);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
    }
}
