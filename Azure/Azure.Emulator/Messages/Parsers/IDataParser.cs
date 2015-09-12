#region

using System;

#endregion

namespace Azure.Messages.Parsers
{
    /// <summary>
    /// Interface IDataParser
    /// </summary>
    public interface IDataParser : IDisposable, ICloneable
    {
        /// <summary>
        /// Handles the packet data.
        /// </summary>
        /// <param name="packet">The packet.</param>
        void HandlePacketData(byte[] packet, int bytesReceived);
    }
}