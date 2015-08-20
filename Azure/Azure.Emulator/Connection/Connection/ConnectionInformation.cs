#region

using System;
using System.Net.Sockets;
using Azure.Encryption.Hurlant.Crypto.Prng;
using Azure.Messages;
using Azure.Messages.Parsers;

#endregion

namespace Azure.Connection.Connection
{
    /// <summary>
    /// Class ConnectionInformation.
    /// </summary>
    public class ConnectionInformation : IDisposable
    {
        /// <summary>
        /// The disable send
        /// </summary>
        public static bool DisableSend;

        /// <summary>
        /// The disable receive
        /// </summary>
        public static bool DisableReceive;

        /// <summary>
        /// The _data socket
        /// </summary>
        private readonly Socket _dataSocket;

        /// <summary>
        /// The _ip
        /// </summary>
        private readonly string _ip;

        /// <summary>
        /// The _connection identifier
        /// </summary>
        private readonly int _connectionId;

        /// <summary>
        /// The _buffer
        /// </summary>
        private readonly byte[] _buffer;

        /// <summary>
        /// The _is connected
        /// </summary>
        private bool _isConnected;

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
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="parser">The parser.</param>
        /// <param name="ip">The ip.</param>
        public ConnectionInformation(Socket dataStream, int connectionId, IDataParser parser,
                                     string ip)
        {
            try
            {
                Parser = parser;
                _buffer = new byte[GameSocketManagerStatics.BufferSize];
                _dataSocket = dataStream;
                _dataSocket.SendBufferSize = GameSocketManagerStatics.BufferSize;
                _ip = ip;
                _connectionId = connectionId;
                if (ConnectionChanged != null)
                    ConnectionChanged(this, ConnectionState.Open);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Delegate ConnectionChange
        /// </summary>
        /// <param name="information">The information.</param>
        /// <param name="state">The state.</param>
        public delegate void ConnectionChange(ConnectionInformation information, ConnectionState state);

        public event ConnectionChange ConnectionChanged;

        /// <summary>
        /// Gets or sets the parser.
        /// </summary>
        /// <value>The parser.</value>
        public IDataParser Parser { get; set; }

        /// <summary>
        /// Starts the packet processing.
        /// </summary>
        public void StartPacketProcessing()
        {
            if (_isConnected) return;
            _isConnected = true;
            try
            {
                _dataSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(IncomingDataPacket), _dataSocket);
            }
            catch
            {
                Disconnect();
            }
        }

        /// <summary>
        /// Gets the ip.
        /// </summary>
        /// <returns>System.String.</returns>
        public string GetIp()
        {
            return _ip;
        }

        /// <summary>
        /// Gets the connection identifier.
        /// </summary>
        /// <returns>System.Int32.</returns>
        public int GetConnectionId()
        {
            return _connectionId;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (_isConnected) Disconnect();
        }

        /// <summary>
        /// Disconnects this instance.
        /// </summary>
        internal void Disconnect()
        {
            try
            {
                if (!_isConnected) return;
                _isConnected = false;

                try
                {
                    if (_dataSocket != null && _dataSocket.Connected)
                    {
                        _dataSocket.Shutdown(SocketShutdown.Both);
                        _dataSocket.Close();
                    }
                }
                catch
                {
                }

                if (_dataSocket != null) _dataSocket.Dispose();
                Parser.Dispose();
                try
                {
                    if (ConnectionChanged != null) ConnectionChanged(this, ConnectionState.Closed);
                }
                catch
                {
                }
                ConnectionChanged = null;
                SocketConnectionCheck.FreeConnection(_ip);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Incomings the data packet.
        /// </summary>
        /// <param name="iAr">The i ar.</param>
        private void IncomingDataPacket(IAsyncResult iAr)
        {
            if (_dataSocket == null) return;
            int length;
            try
            {
                length = _dataSocket.EndReceive(iAr);
            }
            catch
            {
                Disconnect();
                return;
            }
            if (length != 0)
            {
                try
                {
                    if (!DisableReceive)
                    {
                        byte[] array = new byte[length];
                        Array.Copy(_buffer, array, length);

                        HandlePacketData(array);
                    }
                }
                catch
                {
                    Disconnect();
                }
                finally
                {
                    try
                    {
                        _dataSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(IncomingDataPacket),
                            _dataSocket);
                    }
                    catch
                    {
                        Disconnect();
                    }
                }
                return;
            }
            Disconnect();
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
            try
            {
                if (!_isConnected || DisableSend || _dataSocket == null || !_dataSocket.Connected) return;
                if (ARC4ClientSide != null) ARC4ClientSide.Parse(ref packet);

                _dataSocket.BeginSend(packet, 0, packet.Length, SocketFlags.None, new AsyncCallback(SentData), null);
            }
            catch
            {
                Disconnect();
            }
        }

        /// <summary>
        /// Sents the data.
        /// </summary>
        /// <param name="iAr">The i ar.</param>
        private void SentData(IAsyncResult iAr)
        {
            try
            {
                if (_dataSocket == null || !_dataSocket.Connected)
                {
                    Disconnect();
                    return;
                }

                _dataSocket.EndSend(iAr);
            }
            catch
            {
                Disconnect();
            }
        }
    }
}