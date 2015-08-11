#region

using System;
using System.Collections.Specialized;
using System.Net;
using System.Net.Sockets;
using Azure.Messages.Parsers;

#endregion

namespace Azure.Connection.Connection
{
    /// <summary>
    /// Class NewSocketManager.
    /// </summary>
    public class NewSocketManager
    {
        /// <summary>
        /// The _accepted connections
        /// </summary>
        private int _acceptedConnections, _portInformation;

        /// <summary>
        /// The _connection listener
        /// </summary>
        private Socket _connectionListener;

        /// <summary>
        /// The _accept connections
        /// </summary>
        private bool _acceptConnections, _disableNagleAlgorithm;

        /// <summary>
        /// The _parser
        /// </summary>
        private IDataParser _parser;

        /// <summary>
        /// Delegate ConnectionEvent
        /// </summary>
        /// <param name="connection">The connection.</param>
        public delegate void ConnectionEvent(ConnectionInformation connection);

        public event ConnectionEvent Connection;

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
        /// Gets or sets the ip connection count.
        /// </summary>
        /// <value>The ip connection count.</value>
        public HybridDictionary IpConnectionCount { get; set; }

        /// <summary>
        /// Gets or sets the active connections.
        /// </summary>
        /// <value>The active connections.</value>
        public HybridDictionary ActiveConnections { get; set; }

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
            InitializeFields();
            MaximumConnections = maxConnections;
            _portInformation = portId;
            MaxIpConnectionCount = connectionsPerIp;
            AntiDDosStatus = antiDDoS;
            _acceptedConnections = 0;
            PrepareConnectionDetails();
        }

        /// <summary>
        /// Initializes the connection requests.
        /// </summary>
        public void InitializeConnectionRequests()
        {
            _connectionListener.Listen(100);
            _acceptConnections = true;
            string hostName = Dns.GetHostName();
            Dns.GetHostEntry(hostName);
            try
            {
                _connectionListener.BeginAccept(NewConnectionRequest, _connectionListener);
            }
            catch
            {
                Destroy();
            }
            SocketConnectionCheck.SetupTcpAuthorization(20000);
        }

        /// <summary>
        /// Destroys this instance.
        /// </summary>
        public void Destroy()
        {
            _acceptConnections = false;
            try
            {
                _connectionListener.Close();
            }
            catch
            {
            }
            _connectionListener = null;
        }

        /// <summary>
        /// Reports the disconnect.
        /// </summary>
        /// <param name="gameConnection">The game connection.</param>
        public void ReportDisconnect(ConnectionInformation gameConnection)
        {
            gameConnection.ConnectionChanged -= ConnectionChanged;
            ReportUserLogout(gameConnection.GetIp());
        }

        /// <summary>
        /// Initializes the fields.
        /// </summary>
        private void InitializeFields()
        {
            ActiveConnections = new HybridDictionary();
            IpConnectionCount = new HybridDictionary();
        }

        /// <summary>
        /// Prepares the connection details.
        /// </summary>
        /// <exception cref="SocketInitializationException"></exception>
        private void PrepareConnectionDetails()
        {
            _connectionListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _connectionListener.NoDelay = _disableNagleAlgorithm;
            try
            {
                _connectionListener.Bind(new IPEndPoint(IPAddress.Any, _portInformation));
            }
            catch (SocketException ex)
            {
                throw new SocketInitializationException(ex.Message);
            }
        }

        /// <summary>
        /// News the connection request.
        /// </summary>
        /// <param name="iAr">The i ar.</param>
        private void NewConnectionRequest(IAsyncResult iAr)
        {
            if (_connectionListener == null || !_acceptConnections) return;
            Socket socket = ((Socket)iAr.AsyncState).EndAccept(iAr);
            try
            {
                if (SocketConnectionCheck.CheckConnection(socket, MaxIpConnectionCount, AntiDDosStatus))
                {
                    socket.NoDelay = _disableNagleAlgorithm;
                    string ip = socket.RemoteEndPoint.ToString().Split(':')[0];
                    _acceptedConnections++;
                    var connectionInformation = new ConnectionInformation(socket, _acceptedConnections,
                        _parser.Clone() as IDataParser, ip);
                    ReportUserLogin(ip);
                    connectionInformation.ConnectionChanged += ConnectionChanged;
                    if (Connection != null)
                    {
                        Connection(connectionInformation);
                    }
                }
                else
                {
                    try
                    {
                        socket.Dispose();
                        socket.Close();
                    }
                    catch { }
                }
            }
            catch
            {
            }
            finally
            {
                _connectionListener.BeginAccept(NewConnectionRequest, _connectionListener);
            }
        }

        /// <summary>
        /// cs the connection changed.
        /// </summary>
        /// <param name="information">The information.</param>
        /// <param name="state">The state.</param>
        private void ConnectionChanged(ConnectionInformation information, ConnectionState state)
        {
            if (state == ConnectionState.Closed)
            {
                ReportDisconnect(information);
            }
        }

        /// <summary>
        /// Reports the user login.
        /// </summary>
        /// <param name="ip">The ip.</param>
        private void ReportUserLogin(string ip)
        {
            AlterIpConnectionCount(ip, (GetAmountOfConnectionFromIp(ip) + 1));
        }

        /// <summary>
        /// Reports the user logout.
        /// </summary>
        /// <param name="ip">The ip.</param>
        private void ReportUserLogout(string ip)
        {
            AlterIpConnectionCount(ip, (GetAmountOfConnectionFromIp(ip) - 1));
        }

        /// <summary>
        /// Alters the ip connection count.
        /// </summary>
        /// <param name="ip">The ip.</param>
        /// <param name="amount">The amount.</param>
        private void AlterIpConnectionCount(string ip, int amount)
        {
        }

        /// <summary>
        /// Gets the amount of connection from ip.
        /// </summary>
        /// <param name="ip">The ip.</param>
        /// <returns>System.Int32.</returns>
        private int GetAmountOfConnectionFromIp(string ip)
        {
            return 0;
        }
    }
}