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
        /// The current data
        /// </summary>
        public byte[] CurrentData;

        /// <summary>
        /// Delegate NoParamDelegate
        /// </summary>
        public delegate void NoParamDelegate();

        public event NoParamDelegate PolicyRequest;

        public event NoParamDelegate SwitchParserRequest;

        /// <summary>
        /// Handles the packet data.
        /// </summary>
        /// <param name="packet">The packet.</param>
        public void HandlePacketData(byte[] packet)
        {
            if (Azure.ShutdownStarted)
                return;

            if (packet[0] == 60 && PolicyRequest != null)
            {
                PolicyRequest();
                return;
            }

            if (packet[0] == 67 || SwitchParserRequest == null)
                return;

            CurrentData = packet;
            SwitchParserRequest();
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