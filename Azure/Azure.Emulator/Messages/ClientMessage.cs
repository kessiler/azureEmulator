#region

using System;
using System.Globalization;
using System.Text;

#endregion

namespace Azure.Messages
{
    /// <summary>
    /// Class ClientMessage.
    /// </summary>
    public class ClientMessage : IDisposable
    {
        /// <summary>
        /// The _body
        /// </summary>
        private byte[] _body;

        /// <summary>
        /// The _pointer
        /// </summary>
        private int _pointer;

        /// <summary>
        /// The length
        /// </summary>
        internal int Length;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientMessage"/> class.
        /// </summary>
        /// <param name="messageId">The message identifier.</param>
        /// <param name="body">The body.</param>
        internal ClientMessage(int messageId, byte[] body)
        {
            Init(messageId, body);
        }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        internal int Id { get; private set; }

        /// <summary>
        /// Gets the length of the remaining.
        /// </summary>
        /// <value>The length of the remaining.</value>
        internal int RemainingLength
        {
            get { return (_body.Length - _pointer); }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            string stringValue = string.Empty;

            stringValue += Encoding.Default.GetString(_body);
           
            for (int i = 0; i < 13; i++)
                stringValue = stringValue.Replace(char.ToString((char)(i)), string.Format("[{0}]", i));

            return stringValue;
        }

        /// <summary>
        /// Initializes the specified message identifier.
        /// </summary>
        /// <param name="messageId">The message identifier.</param>
        /// <param name="body">The body.</param>
        internal void Init(int messageId, byte[] body)
        {
            if (body == null)
                body = new byte[0];

            Id = messageId;

            _body = body;
            Length = body.Length;
            _pointer = 0;
        }

        /// <summary>
        /// Reads the bytes.
        /// </summary>
        /// <param name="len">The bytes length.</param>
        /// <returns>System.Byte[].</returns>
        internal byte[] ReadBytes(int len)
        {
            if (len > RemainingLength)
                len = RemainingLength;

            byte[] arrayBytes = new byte[len];

            for (int i = 0; i < len; i++)
                arrayBytes[i] = _body[_pointer++];

            return arrayBytes;
        }

        /// <summary>
        /// Gets the bytes.
        /// </summary>
        /// <param name="len">The bytes length.</param>
        /// <returns>System.Byte[].</returns>
        internal byte[] GetBytes(int len)
        {
            if (len > RemainingLength)
                len = RemainingLength;

            byte[] arrayBytes = new byte[len];
            int pos = _pointer;

            for (int i = 0; i < len; i++)
            {
                arrayBytes[i] = _body[pos];
                pos++;
            }

            return arrayBytes;
        }

        /// <summary>
        /// Gets the next.
        /// </summary>
        /// <returns>System.Byte[].</returns>
        internal byte[] GetNext()
        {
            int length = HabboEncoding.DecodeInt16(ReadBytes(2));

            return ReadBytes(length);
        }

        /// <summary>
        /// Gets the string.
        /// </summary>
        /// <returns>System.String.</returns>
        internal string GetString()
        {
            return GetString(Encoding.UTF8);
        }

        /// <summary>
        /// Gets the string.
        /// </summary>
        /// <param name="encoding">The encoding.</param>
        /// <returns>System.String.</returns>
        internal string GetString(Encoding encoding)
        {
            return encoding.GetString(GetNext());
        }

        /// <summary>
        /// Gets the integer from string.
        /// </summary>
        /// <returns>System.Int32.</returns>
        internal int GetIntegerFromString()
        {
            int result;

            string stringValue = GetString(Encoding.ASCII);

            int.TryParse(stringValue, out result);

            return result;
        }

        /// <summary>
        /// Gets the bool.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool GetBool()
        {
            if (RemainingLength <= 0) 
                return false;

            return _body[_pointer++] == (char)1;
        }

        /// <summary>
        /// Gets the integer16.
        /// </summary>
        /// <returns>System.Int16.</returns>
        internal short GetInteger16()
        {
            return short.Parse(GetInteger().ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Gets the integer.
        /// </summary>
        /// <returns>System.Int32.</returns>
        internal int GetInteger()
        {
            if (RemainingLength < 1)
                return 0;

            byte[] bytesArray = GetBytes(4);

            int result = HabboEncoding.DecodeInt32(bytesArray);
            _pointer += 4;

            return result;
        }

        internal bool GetIntegerAsBool()
        {
            if (RemainingLength < 1)
                return false;

            byte[] bytesArray = GetBytes(4);

            int result = HabboEncoding.DecodeInt32(bytesArray);
            _pointer += 4;

            return result == 1;
        }

        /// <summary>
        /// Gets the integer32.
        /// </summary>
        /// <returns>System.UInt32.</returns>
        internal uint GetUInteger()
        {
            return (uint)(GetInteger());
        }

        /// <summary>
        /// Gets the integer16.
        /// </summary>
        /// <returns>System.UInt16.</returns>
        internal ushort GetUInteger16()
        {
            return (UInt16)(GetInteger());
        }
    }
}