using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.Messages
{
    class ClientMessageFactory
    {
        private static ConcurrentQueue<ClientMessage> freeObjects;

        internal static void Init()
        {
            freeObjects = new ConcurrentQueue<ClientMessage>();
        }

        internal static ClientMessage GetClientMessage(int messageId, byte[] body, int position, int packetLength)
        {
            if (freeObjects.Count > 0)
            {
                ClientMessage message;
                if (freeObjects.TryDequeue(out message))
                {
                    message.Init(messageId, body, position, packetLength);
                    return message;
                }
            }
            return new ClientMessage(messageId, body, position, packetLength);
        }

        internal static void ObjectCallback(ClientMessage message)
        {
            freeObjects.Enqueue(message);
        }
    }
}
