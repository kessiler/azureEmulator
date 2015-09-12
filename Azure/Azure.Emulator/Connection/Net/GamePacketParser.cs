#region

using System;
using Azure.Configuration;
using Azure.Connection.Connection;
using Azure.HabboHotel.GameClients;
using Azure.Messages;
using Azure.Messages.Parsers;
using Azure.Util;

#endregion

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
        private const int INT_SIZE = sizeof(int);
        private static readonly MemoryContainer memoryContainer = new MemoryContainer(10, 2048);
        private readonly byte[] bufferedData;
        private int bufferPos;
        private int currentPacketLength;

        /// <summary>
        /// Initializes a new instance of the <see cref="GamePacketParser"/> class.
        /// </summary>
        /// <param name="me">Me.</param>
        internal GamePacketParser()
        {
            bufferPos = 0;
            currentPacketLength = -1;
            bufferedData = memoryContainer.TakeBuffer();
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
        public void SetConnection(ConnectionInformation con, GameClient me)
        {
            _con = con;
            _currentClient = me;
        }

        /// <summary>
        /// Handles the packet data.
        /// </summary>
        /// <param name="data">The data.</param>
        public void HandlePacketData(byte[] data, int length)
        {
            if (length > 0 && _currentClient != null)
            {
                int pos = 0;
                short messageId = 0;

                try
                {
                    for (pos = 0; pos < length; )
                    {
                        if (currentPacketLength == -1)
                        {
                            if (length < INT_SIZE)
                            {
                                bufferCopy(data, length); // store the bytes in the buffer for the next read and break operation
                                break;
                            }

                            currentPacketLength = HabboEncoding.DecodeInt32(data, ref pos);
                        }
                        if (currentPacketLength < 2 || currentPacketLength > 4096)
                        {
                            currentPacketLength = -1;
                            break; //broken packet! might be better to disconnect EVERYTHING
                        }
                        if (currentPacketLength == ((length - pos) + bufferPos)) // if packet is exactly big enough (no more data needed, no excessive data)
                        {
                            if (bufferPos != 0) // do we have stuff in buffer
                            {
                                bufferCopy(data, length, pos);
                                pos = 0;
                                messageId = HabboEncoding.DecodeInt16(bufferedData, ref pos);
                                handleMessage(messageId, bufferedData, 2, currentPacketLength);
                            }
                            else
                            {
                                messageId = HabboEncoding.DecodeInt16(data, ref pos);       //pos -= 2;// -2 cuz of toint16
                                handleMessage(messageId, data, pos, currentPacketLength);
                            }
                            pos = length;
                            currentPacketLength = -1;
                        }
                        else //  we have remainder
                        {
                            int remainder = ((length - pos)) - (currentPacketLength - bufferPos);

                            if (bufferPos != 0)
                            {
                                int toCopy = remainder - bufferPos;
                                bufferCopy(data, toCopy, pos);
                                int zero = 0;
                                messageId = HabboEncoding.DecodeInt16(bufferedData, ref zero); //small hack to preserve the POS value
                                //Create packet with bufferData variable
                                handleMessage(messageId, bufferedData, 2, currentPacketLength); //Not sure
                            }
                            else
                            {
                                messageId = HabboEncoding.DecodeInt16(data, ref pos);
                                //create packet with data variable
                                //pos -= 2;
                                handleMessage(messageId, data, pos, currentPacketLength);
                                pos -= 2; //because else the remainder will fail
                            }
                            currentPacketLength = -1;

                            pos = (length - remainder); //set pos to max pos of remainder
                        }
                    }
                }
                catch (Exception exception)
                {
                    Logging.HandleException(exception, string.Format("packet handling ----> {0}", messageId));

                    _con.Dispose();
                }
            }
        }

        private void handleMessage(int messageId, byte[] packetContent, int position, int packetLength) {
            if (_currentClient != null && _currentClient.GetMessageHandler() != null)
            {
                using (ClientMessage clientMessage = new ClientMessage(messageId, packetContent, position, packetLength))
                    _currentClient.GetMessageHandler().HandleRequest(clientMessage);
            }
        }

        private void bufferCopy(byte[] data, int bytes, int offset = 0)
        {
            for (int i = 0; i < (bytes - offset); i++)
                bufferedData[bufferPos++] = data[i + offset];
        }

        private void bufferCopy(byte[] data, int bytes)
        {
            for (int i = 0; i < bytes; i++)
                bufferedData[bufferPos++] = data[i];
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            //todo: mem checking
            //_currentClient = null;
            //_con = null;
            memoryContainer.GiveBuffer(bufferedData);
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