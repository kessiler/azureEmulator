#region

using Azure.Messages.Parsers;

#endregion

namespace Azure.Connection.Net
{
    /// <summary>
    /// Class InitialPacketParser.
    /// </summary>
    public class InitialPacketParser : IDataParser
    {
        /// <summary>
        /// Delegate NoParamDelegate
        /// </summary>
        public delegate void NoParamDelegate();

        /// <summary>
        /// Delegate with params
        /// </summary>
        public delegate void DualParamDelegate(byte[] packet, int amountOfBytes);

        public event NoParamDelegate PolicyRequest;

        public event DualParamDelegate SwitchParserRequest;

        /// <summary>
        /// Handles the packet data.
        /// </summary>
        /// <param name="packet">The packet.</param>
        public void HandlePacketData(byte[] packet, int amountOfBytes)
        {
            if (Azure.ShutdownStarted)
                return;

            if (packet[0] == 60 && PolicyRequest != null)
                PolicyRequest();
            else if (packet[0] != 67 || SwitchParserRequest == null)
                SwitchParserRequest(packet, amountOfBytes);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            PolicyRequest = null;
            SwitchParserRequest = null;
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public object Clone()
        {
            return new InitialPacketParser();
        }
    }
}