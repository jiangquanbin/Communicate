using System;
using System.Buffers;

namespace Communicate.Core.SocketMessage
{
    public class BufferManager
    {
        private ArrayPool<byte> _arrayPool;
        private int _maxBufferSize;

        public BufferManager(int maxBufferSize = ushort.MaxValue)
        {
            var maxArrayLength = maxBufferSize;
            _arrayPool = ArrayPool<byte>.Create( maxBufferSize, 4);
            _maxBufferSize = maxBufferSize;
        }

        public byte[] TakeBuffer(int size)
        {
            return _arrayPool.Rent(size);
        }

        public void ReturnBuffer(byte[] buffer)
        {
            if (buffer == null)
            {
                return;
            }
            _arrayPool.Return(buffer);
        }
    }
}