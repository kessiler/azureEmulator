#region

using System;
using Azure.Connection.Connection;
using Azure.Connection.Net;

#endregion

namespace Azure.Configuration
{
    /// <summary>
    /// Class ConnectionHandling.
    /// </summary>
    public class ConnectionHandling
    {
        /// <summary>
        /// The manager
        /// </summary>
        public NewSocketManager Manager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionHandling"/> class.
        /// </summary>
        /// <param name="port">The port.</param>
        /// <param name="maxConnections">The maximum connections.</param>
        /// <param name="connectionsPerIp">The connections per ip.</param>
        /// <param name="enabeNagles">if set to <c>true</c> [enabe nagles].</param>
        public ConnectionHandling(int port, int maxConnections, int connectionsPerIp, bool antiDDoS, bool enabeNagles)
        {
            Manager = new NewSocketManager();
            Manager.Init(port, maxConnections, connectionsPerIp, antiDDoS, new InitialPacketParser(), !enabeNagles);
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        internal void Init()
        {
            Manager.Connection += ManagerConnectionEvent;
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        internal void Start()
        {
            Manager.InitializeConnectionRequests();
        }

        /// <summary>
        /// Destroys this instance.
        /// </summary>
        internal void Destroy()
        {
            Manager.Destroy();
        }

        /// <summary>
        /// Managers the connection event.
        /// </summary>
        /// <param name="connection">The connection.</param>
        private static void ManagerConnectionEvent(ConnectionInformation connection)
        {
            connection.ConnectionChanged += ConnectionChanged;
            Azure.GetGame().GetClientManager().CreateAndStartClient(((uint)connection.GetConnectionId()), connection);
        }

        /// <summary>
        /// Connections the changed.
        /// </summary>
        /// <param name="information">The information.</param>
        /// <param name="state">The state.</param>
        private static void ConnectionChanged(ConnectionInformation information, ConnectionState state)
        {
            if (state == ConnectionState.Closed)
                CloseConnection(information);
        }

        /// <summary>
        /// Closes the connection.
        /// </summary>
        /// <param name="connection">The connection.</param>
        private static void CloseConnection(ConnectionInformation connection)
        {
            try
            {
                connection.Dispose();
                Azure.GetGame().GetClientManager().DisposeConnection(((uint)connection.GetConnectionId()));
            }
            catch (Exception ex)
            {
                Logging.LogException(ex.ToString());
            }
        }
    }
}