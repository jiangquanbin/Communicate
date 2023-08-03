namespace Communicate.Core.SocketMessage
{
    public class TcpMessageConfig
    {
        /// <summary>
        /// 接收缓存大小
        /// </summary>
        public int ReceiveBufferSize = ushort.MaxValue;
        /// <summary>
        /// 消息头大小
        /// </summary>
        public int MessageHeaderSize = ushort.MaxValue;

        /// <summary>
        /// 接收数据长度byte起始位
        /// </summary>
        public int MessageHeaderSizeStart = 4;

        /// <summary>
        /// 接收数据长度byte长度
        /// </summary>
        public int MessageHeaderSizeLength = 4;

        /// <summary>
        /// 消息大小是否包含请求头
        /// </summary>
        public bool IsContainHead;
    }
}