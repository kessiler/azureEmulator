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
    public class ConnectionInformation : IDisposable
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
        public int ChannelId { get; private set; }

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
        public ConnectionInformation(Socket socket, IDataParser parser, int _ChannelId)
        {
            _socket = socket;
            socket.SendBufferSize = GameSocketManagerStatics.BufferSize;
            Parser = parser;
            _buffer = new byte[GameSocketManagerStatics.BufferSize];
            _remoteEndPoint = socket.RemoteEndPoint;
            _connected = true;
            ChannelId = _ChannelId;
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
                if (_socket != null && _socket.Connected)
                {
                    try
                    {
                        _socket.Shutdown(SocketShutdown.Both);
                        _socket.Close();
                    }
                    catch (Exception) { } // silent
                }
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
            try
            {
                Socket dataSocket = (Socket)async.AsyncState;
                int bytesReceived = dataSocket.EndReceive(async);
                if (bytesReceived != 0)
                {
                    byte[] array = new byte[bytesReceived];
                    Array.Copy(_buffer, array, bytesReceived);
                    lock (array)
                    {

                        HandlePacketData(array, bytesReceived);
                    }
                }
                else
                {
                    Disconnect();
                }
            }
            catch (Exception exception)
            {
                HandleDisconnect(SocketError.ProtocolNotSupported, exception);

                // event handler closed the socket.
                if (_socket == null || !_socket.Connected)
                    return;
            }
            finally
            {
                try
                {
                    if (_socket != null && _socket.Connected && _connected)
                    {
                        _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, OnReadCompleted, _socket);
                    }
                    else
                    {
                        Disconnect();
                    }
                }
                catch (Exception exception)
                {
                    HandleDisconnect(SocketError.ConnectionAborted, exception);
                }
            }

        }

        private void OnSendCompleted(IAsyncResult async)
        {
            try
            {
                Socket dataSocket = (Socket)async.AsyncState;
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
            if (_connected) ReadAsync();
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
        public int GetConnectionId()
        {
            return ChannelId;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Disconnect();
        }

        /// <summary>
        /// Disconnects this instance.
        /// </summary>
        internal void Disconnect()
        {
            if (_connected) HandleDisconnect(SocketError.ConnectionReset, new SocketException((int)SocketError.ConnectionReset));
        }

        /// <summary>
        /// Handles the packet data.
        /// </summary>
        /// <param name="packet">The packet.</param>
        private void HandlePacketData(byte[] packet, int bytesReceived)
        {
            if (Parser != null)
            {
                if (ARC4ServerSide != null) ARC4ServerSide.Parse(ref packet);
                Parser.HandlePacketData(packet, bytesReceived);
            }
        }

        /// <summary>
        /// Sends the data.
        /// </summary>
        /// <param name="packet">The packet.</param>
        public void SendData(byte[] packet)
        {
            if (_socket != null && _socket.Connected)
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
            else
            {
                Disconnect();
            }
        }
    }
}