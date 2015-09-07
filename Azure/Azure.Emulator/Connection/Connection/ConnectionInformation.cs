#region

using System;
using System.Net.Sockets;
using Azure.Encryption.Hurlant.Crypto.Prng;
using Azure.Messages;
using Azure.Messages.Parsers;
using Azure.Configuration;
using System.IO;

#endregion

namespace Azure.Connection.Connection
{
    /// <summary>
    /// Class ConnectionInformation.
    /// </summary>
    public class ConnectionInformation
    {

        private Socket _socket;
        private System.Net.EndPoint _remoteEndPoint;
        public delegate void OnClientDisconnectedEvent(ConnectionInformation connection, Exception exception);
        public event OnClientDisconnectedEvent _disconnectAction = delegate { };
        public delegate void OnMessageReceiveEvent(ConnectionInformation connection, object message);
        public event OnMessageReceiveEvent _messageReceived = delegate { };


        /// <summary>
        ///     Identity of this channel
        /// </summary>
        /// <remarks>
        ///     Must be unique within a server.
        /// </remarks>
        public string ChannelId { get; private set; }

        /// <summary>
        /// The _is connected
        /// </summary>
        private bool _connected = false;

        /// <summary>
        /// The _buffer
        /// </summary>
        private readonly byte[] _buffer;

        /// <summary>
        /// Gets or sets the parser.
        /// </summary>
        /// <value>The parser.</value>
        public IDataParser Parser { get; set; }

        /// <summary>
        /// The ar c4 server side
        /// </summary>
        internal ARC4 ARC4ServerSide;

        /// <summary>
        /// The ar c4 client side
        /// </summary>
        internal ARC4 ARC4ClientSide;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionInformation"/> class.
        /// </summary>
        /// <param name="dataStream">The data stream.</param>
        /// <param name="parser">The parser.</param>
        public ConnectionInformation(Socket socket, IDataParser parser)
        {
            _socket = socket;
            socket.SendBufferSize = GameSocketManagerStatics.BufferSize;
            Parser = parser;
            _buffer = new byte[GameSocketManagerStatics.BufferSize];
            _remoteEndPoint = socket.RemoteEndPoint;
            _connected = true;
            ChannelId = Guid.NewGuid().ToString();
        }

        public OnClientDisconnectedEvent Disconnected
        {
            get { return _disconnectAction; }

            set
            {
                if (value == null)
                    _disconnectAction = (x, e) => { };
                else
                    _disconnectAction = value;
            }
        }

        public OnMessageReceiveEvent MessageReceived
        {
            get { return _messageReceived; }
            set
            {
                if (value == null)
                    throw new ArgumentException("You must have a MessageReceived delegate");


                _messageReceived = value;
            }
        }

        private void ReadAsync()
        {
            try
            {
                _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, OnReadCompleted, _socket);
            }
            catch (Exception e)
            {
                HandleDisconnect(SocketError.ConnectionReset, e);
            }
        }


        private void HandleDisconnect(SocketError socketError, Exception exception)
        {
            try
            {
                _socket.Close();
                _connected = false;
                Parser.Dispose();
                SocketConnectionCheck.FreeConnection(GetIp());
                _disconnectAction(this, exception);
            }
            catch (Exception ex)
            {
                Logging.LogException(ex.ToString());
            }
        }

        private void OnReadCompleted(IAsyncResult async)
        {
            Socket dataSocket = (Socket)async.AsyncState;
            try
            {
                int bytesReceived = dataSocket.EndReceive(async);
                if (bytesReceived != 0)
                {
                    byte[] array = new byte[bytesReceived];
                    Array.Copy(_buffer, array, bytesReceived);
                    HandlePacketData(array);
                }
            }
            catch (Exception exception)
            {
                HandleDisconnect(SocketError.ProtocolNotSupported, exception);

                // event handler closed the socket.
                if (_socket == null || !_socket.Connected)
                    return;
            }
            if (_socket.Connected)
            {
                _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, OnReadCompleted, _socket);
            }
        }

        private void OnSendCompleted(IAsyncResult async)
        {
            Socket dataSocket = (Socket)async.AsyncState;
            try
            {
                dataSocket.EndSend(async);
            }
            catch (Exception exception)
            {
                HandleDisconnect(SocketError.ProtocolNotSupported, exception);

                // event handler closed the socket.
                if (_socket == null || !_socket.Connected)
                    return;
            }
        }

        /// <summary>
        ///     Cleanup everything so that the channel can be reused.
        /// </summary>
        public void Cleanup()
        {
            _socket = null;
            _connected = false;
        }

        /// <summary>
        /// Starts the packet processing.
        /// </summary>
        public void StartPacketProcessing()
        {
            ReadAsync();
        }

        /// <summary>
        /// Gets the ip.
        /// </summary>
        /// <returns>System.String.</returns>
        public string GetIp()
        {
            return _remoteEndPoint.ToString().Split(':')[0];
        }

        /// <summary>
        /// Gets the connection identifier.
        /// </summary>
        /// <returns>System.Int32.</returns>
        public int GetConnectionId()
        {
            return 1;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (_connected) Disconnect();
        }

        /// <summary>
        /// Disconnects this instance.
        /// </summary>
        internal void Disconnect()
        {
            HandleDisconnect(SocketError.ConnectionReset, new SocketException((int)SocketError.ConnectionReset));
        }

        /// <summary>
        /// Handles the packet data.
        /// </summary>
        /// <param name="packet">The packet.</param>
        private void HandlePacketData(byte[] packet)
        {
            if (Parser != null)
            {
                if (ARC4ServerSide != null) ARC4ServerSide.Parse(ref packet);
                Parser.HandlePacketData(packet);
            }
        }

        /// <summary>
        /// Sends the data.
        /// </summary>
        /// <param name="packet">The packet.</param>
        public void SendData(byte[] packet)
        {
            if (_socket.Connected)
            {
                if (ARC4ClientSide != null)
                    ARC4ClientSide.Parse(ref packet);
                try
                {
                    _socket.BeginSend(packet, 0, packet.Length, SocketFlags.None, OnSendCompleted, _socket);
                }
                catch (Exception e)
                {
                    HandleDisconnect(SocketError.ConnectionReset, e);
                }
            }
        }
    }
}