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
        public SocketManager Manager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionHandling"/> class.
        /// </summary>
        /// <param name="port">The port.</param>
        /// <param name="maxConnections">The maximum connections.</param>
        /// <param name="connectionsPerIp">The connections per ip.</param>
        /// <param name="enabeNagles">if set to <c>true</c> [enabe nagles].</param>
        public ConnectionHandling(int port, int maxConnections, int connectionsPerIp, bool antiDDoS, bool enabeNagles)
        {
            Manager = new SocketManager();
            Manager.OnClientConnected += OnClientConnected;
            Manager.OnClientDisconnected += OnClientDisconnected;
            Manager.Init(port, maxConnections, connectionsPerIp, antiDDoS, new InitialPacketParser(), !enabeNagles);
        }

        /// <summary>
        /// Managers the connection event.
        /// </summary>
        /// <param name="connection">The connection.</param>
        private static void OnClientConnected(ConnectionInformation connection)
        {
            try
            {
                Azure.GetGame().GetClientManager().CreateAndStartClient((uint)connection.GetConnectionId(), connection);
                Out.WriteLine("Client connected - ID: "+ connection.GetConnectionId() + " /  IP: " + connection.GetIp(), "Azure.Socket", ConsoleColor.DarkYellow);
            }
            catch (Exception ex)
            {
                Logging.LogException(ex.ToString());
            }
        }

        /// <summary>
        /// Closes the connection.
        /// </summary>
        /// <param name="connection">The connection.</param>
        private static void OnClientDisconnected(ConnectionInformation connection, Exception exception)
        {
            try
            {
                Azure.GetGame().GetClientManager().DisposeConnection((uint)connection.GetConnectionId());
                Out.WriteLine("Client disconnected - ID: " + connection.GetConnectionId() + " /  IP: " + connection.GetIp(), "Azure.Socket", ConsoleColor.DarkYellow);
            }
            catch (Exception ex)
            {
                Logging.LogException(ex.ToString());
            }
        }

        /// <summary>
        /// Destroys this instance.
        /// </summary>
        internal void Destroy()
        {
            Manager.Destroy();
        }
    }
}