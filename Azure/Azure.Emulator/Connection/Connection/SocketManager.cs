﻿#region

using System;
using System.Collections.Specialized;
using System.Net;
using System.Net.Sockets;
using Azure.Messages.Parsers;
using Azure.Configuration;

#endregion

namespace Azure.Connection.Connection
{
    /// <summary>
    /// Class SocketManager.
    /// </summary>
    public class SocketManager
    {
        /// <summary>
        /// The port to open socket
        /// </summary>
        private int _portInformation;

        /// <summary>
        /// Count of accepeted connections
        /// </summary>
        private int acceptedConnections;

        /// <summary>
        /// The _connection listener
        /// </summary>
        private TcpListener _listener;

        /// <summary>
        /// The _disableNagleAlgorithm in connectios
        /// </summary>
        private bool _disableNagleAlgorithm;

        /// <summary>
        /// The _parser
        /// </summary>
        private IDataParser _parser;

        /// <summary>
        ///     A client has connected (nothing has been sent or received yet)
        /// </summary>
        public delegate void OnClientConnectedEvent(ConnectionInformation connection);
        public event OnClientConnectedEvent OnClientConnected = delegate { };
        /// <summary>
        ///     A client has disconnected
        /// </summary>
        public delegate void OnClientDisconnectedEvent(ConnectionInformation connection, Exception exception);
        public event OnClientDisconnectedEvent OnClientDisconnected = delegate { };

        /// <summary>
        /// Gets or sets the maximum connections.
        /// </summary>
        /// <value>The maximum connections.</value>
        public int MaximumConnections { get; set; }

        /// <summary>
        /// Gets or sets the maximum ip connection count.
        /// </summary>
        /// <value>The maximum ip connection count.</value>
        public int MaxIpConnectionCount { get; set; }

        /// <summary>
        /// Gets or sets the AntiDDoS Status.
        /// </summary>
        public bool AntiDDosStatus { get; set; }

        /// <summary>
        /// Initializes the specified port identifier.
        /// </summary>
        /// <param name="portId">The port identifier.</param>
        /// <param name="maxConnections">The maximum connections.</param>
        /// <param name="connectionsPerIp">The connections per ip.</param>
        /// <param name="antiDDoS">The antiDDoS status</param>
        /// <param name="parser">The parser.</param>
        /// <param name="disableNaglesAlgorithm">if set to <c>true</c> [disable nagles algorithm].</param>
        public void Init(int portId, int maxConnections, int connectionsPerIp, bool antiDDoS, IDataParser parser, bool disableNaglesAlgorithm)
        {
            _parser = parser;
            _disableNagleAlgorithm = disableNaglesAlgorithm;
            MaximumConnections = maxConnections;
            _portInformation = portId;
            acceptedConnections = 0;
            MaxIpConnectionCount = connectionsPerIp;
            AntiDDosStatus = antiDDoS;
            if (_portInformation < 0)
                throw new ArgumentOutOfRangeException("port", _portInformation, "Port must be 0 or more.");
            if (_listener != null)
                throw new InvalidOperationException("Already listening.");
            PrepareConnectionDetails();
        }

        /// <summary>
        /// Prepares the connection details.
        /// </summary>
        /// <exception cref="SocketInitializationException"></exception>
        private void PrepareConnectionDetails()
        {
            try
            {
                SocketConnectionCheck.SetupTcpAuthorization(20000);
                _listener = new TcpListener(IPAddress.Any, _portInformation);
                _listener.Start();
                _listener.BeginAcceptSocket(OnAcceptSocket, _listener);
            }
            catch (SocketException ex)
            {
                throw new SocketInitializationException(ex.Message);
            }
        }

        protected virtual void OnMessage(ConnectionInformation connection, object msg) { }

        private void OnChannelDisconnect(ConnectionInformation connection, Exception exception)
        {
            OnClientDisconnected(connection, exception);
            connection.Cleanup();
        }



        private void OnAcceptSocket(IAsyncResult ar)
        {
            try
            {
                Socket socket = _listener.EndAcceptSocket(ar);
                if (socket.Connected)
                {
                    if (SocketConnectionCheck.CheckConnection(socket, MaxIpConnectionCount, AntiDDosStatus))
                    {
                        socket.NoDelay = _disableNagleAlgorithm;
                        acceptedConnections++;
                        var connectionInfo = new ConnectionInformation(socket, _parser.Clone() as IDataParser, acceptedConnections);
                        connectionInfo.Disconnected = OnChannelDisconnect;
                        connectionInfo.MessageReceived = OnMessage;
                        OnClientConnected(connectionInfo);
                    }
                }
            }
            catch (Exception){}


            _listener.BeginAcceptSocket(OnAcceptSocket, _listener);
        }

        /// <summary>
        /// Destroys this instance.
        /// </summary>
        public void Destroy()
        {
            _listener.Stop();
        }

    }
}