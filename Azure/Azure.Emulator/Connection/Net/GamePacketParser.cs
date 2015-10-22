using System;
using Azure.Configuration;
using Azure.Connection.Connection;
using Azure.HabboHotel.GameClients.Interfaces;
using Azure.Messages;
using Azure.Messages.Factorys;
using Azure.Messages.Parsers;
using Azure.Util;

namespace Azure.Connection.Net
{
    /// <summary>
    /// Class GamePacketParser.
    /// </summary>
    public class GamePacketParser : IDataParser
    {
        /// <summary>
        /// The _current client
        /// </summary>
        private GameClient _currentClient;

        /// <summary>
        /// The _con
        /// </summary>
        private ConnectionInformation _con;

        private const int IntSize = sizeof(int);
        private static readonly MemoryContainer MemoryContainer = new MemoryContainer(10, 2048);
        private readonly byte[] _bufferedData;
        private int _bufferPos;
        private int _currentPacketLength;

        /// <summary>
        /// Initializes a new instance of the <see cref="GamePacketParser"/> class.
        /// </summary>
        internal GamePacketParser()
        {
            _bufferPos = 0;
            _currentPacketLength = -1;
            _bufferedData = MemoryContainer.TakeBuffer();
        }

        /// <summary>
        /// Delegate HandlePacket
        /// </summary>
        /// <param name="message">The message.</param>
        public delegate void HandlePacket(ClientMessage message);

        /// <summary>
        /// Sets the connection.
        /// </summary>
        /// <param name="con">The con.</param>
        /// <param name="me"></param>
        public void SetConnection(ConnectionInformation con, GameClient me)
        {
            _con = con;
            _currentClient = me;
        }

        /// <summary>
        /// Handles the packet data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="length"></param>
        public void HandlePacketData(byte[] data, int length)
        {
            if (length > 0 && _currentClient != null)
            {
                short messageId = 0;

                try
                {
                    int pos;
                    for (pos = 0; pos < length;)
                    {
                        if (_currentPacketLength == -1)
                        {
                            if (length < IntSize)
                            {
                                bufferCopy(data, length); // store the bytes in the buffer for the next read and break operation
                                break;
                            }

                            _currentPacketLength = HabboEncoding.DecodeInt32(data, ref pos);
                        }
                        if (_currentPacketLength < 2 || _currentPacketLength > 4096)
                        {
                            _currentPacketLength = -1;
                            break; //broken packet! might be better to disconnect EVERYTHING
                        }
                        if (_currentPacketLength == ((length - pos) + _bufferPos)) // if packet is exactly big enough (no more data needed, no excessive data)
                        {
                            if (_bufferPos != 0) // do we have stuff in buffer
                            {
                                bufferCopy(data, length, pos);
                                pos = 0;
                                messageId = HabboEncoding.DecodeInt16(_bufferedData, ref pos);
                                HandleMessage(messageId, _bufferedData, 2, _currentPacketLength);
                            }
                            else
                            {
                                messageId = HabboEncoding.DecodeInt16(data, ref pos);       //pos -= 2;// -2 cuz of toint16
                                HandleMessage(messageId, data, pos, _currentPacketLength);
                            }
                            pos = length;
                            _currentPacketLength = -1;
                        }
                        else //  we have remainder
                        {
                            int remainder = ((length - pos)) - (_currentPacketLength - _bufferPos);

                            if (_bufferPos != 0)
                            {
                                int toCopy = remainder - _bufferPos;
                                bufferCopy(data, toCopy, pos);
                                int zero = 0;
                                messageId = HabboEncoding.DecodeInt16(_bufferedData, ref zero); //small hack to preserve the POS value
                                //Create packet with bufferData variable
                                HandleMessage(messageId, _bufferedData, 2, _currentPacketLength); //Not sure
                            }
                            else
                            {
                                messageId = HabboEncoding.DecodeInt16(data, ref pos);
                                //create packet with data variable
                                //pos -= 2;
                                HandleMessage(messageId, data, pos, _currentPacketLength);
                                pos -= 2; //because else the remainder will fail
                            }
                            _currentPacketLength = -1;

                            pos = (length - remainder); //set pos to max pos of remainder
                        }
                    }
                }
                catch (Exception exception)
                {
                    Logging.HandleException(exception, $"packet handling ----> {messageId}");

                    _con.Dispose();
                }
            }
        }

        private void HandleMessage(int messageId, byte[] packetContent, int position, int packetLength)
        {
            using (ClientMessage clientMessage = ClientMessageFactory.GetClientMessage(messageId, packetContent, position, packetLength))
            {
                if (_currentClient != null && _currentClient.GetMessageHandler() != null)
                {
                    _currentClient.GetMessageHandler().HandleRequest(clientMessage);
                }
            }
        }

        private void bufferCopy(byte[] data, int bytes, int offset = 0)
        {
            for (int i = 0; i < (bytes - offset); i++)
                _bufferedData[_bufferPos++] = data[i + offset];
        }

        private void bufferCopy(byte[] data, int bytes)
        {
            for (int i = 0; i < bytes; i++)
                _bufferedData[_bufferPos++] = data[i];
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            //todo: mem checking
            //_currentClient = null;
            //_con = null;
            MemoryContainer.GiveBuffer(_bufferedData);
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public object Clone()
        {
            return new GamePacketParser();
        }
    }
}