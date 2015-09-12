using System.Collections;

namespace Azure.Util
{
    public class MemoryContainer
    {
        private readonly Queue container;
        private readonly int bufferSize;

        public MemoryContainer(int initSize, int bufferSize)
        {
            container = new Queue(initSize);
            this.bufferSize = bufferSize;

            for (int i = 0; i < initSize; i++)
                container.Enqueue(new byte[bufferSize]);
        }

        public byte[] TakeBuffer()
        {
            if (container.Count > 0)
            {
                lock (container.SyncRoot)
                {
                    return (byte[])container.Dequeue();
                }
            }

            return new byte[bufferSize];
        }

        public void GiveBuffer(byte[] buffer)
        {
            lock (container.SyncRoot)
            {
                container.Enqueue(buffer);
            }
        }
    }
}
