using System.Collections;

namespace Azure.Util
{
    public class MemoryContainer
    {
        private readonly Queue _container;
        private readonly int _bufferSize;

        public MemoryContainer(int initSize, int bufferSize)
        {
            _container = new Queue(initSize);
            _bufferSize = bufferSize;

            for (int i = 0; i < initSize; i++)
                _container.Enqueue(new byte[bufferSize]);
        }

        public byte[] TakeBuffer()
        {
            if (_container.Count > 0)
            {
                lock (_container.SyncRoot)
                {
                    return (byte[])_container.Dequeue();
                }
            }

            return new byte[_bufferSize];
        }

        public void GiveBuffer(byte[] buffer)
        {
            lock (_container.SyncRoot)
            {
                _container.Enqueue(buffer);
            }
        }
    }
}