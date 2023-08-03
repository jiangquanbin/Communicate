using System;
using System.Buffers;
using System.Net;
using System.Net.Sockets;
using Communicate.Core.ByteTransform;

namespace Communicate.Core.SocketMessage
{
    public class TCPMessageSocket : IDisposable
    {
        /// <summary>
        /// Socket连接成功代理
        /// </summary>
        public delegate void OnConnectComplete(object sender, SocketAsyncEventArgs args);

        public event OnConnectComplete onConnectComplete;

        public Action<Exception> onSendError;

        /// <summary>
        /// 数据接收代理
        /// </summary>
        public delegate void OnMessageReceived(byte[] message);

        public event OnMessageReceived onMessageReceived;

        private Socket _socket;
        private string _ipAddress;
        private int _port;
        private BufferManager _bufferManager;
        private byte[] _receiveBuffer;
        private bool _closed;
        private int _bytesReceived;
        private int _bytesToReceive;
        private int _incomingMessageSize = -1;
        private TcpMessageConfig _tcpMessageConfig;


        public TCPMessageSocket(string ipAddress, int port, TcpMessageConfig tcpMessageConfig)
        {
            _ipAddress = ipAddress;
            _port = port;
            _tcpMessageConfig = tcpMessageConfig;
            _bufferManager = new BufferManager();
        }

        /// <summary>
        /// 建立连接
        /// </summary>
        /// <param name="UserToken"></param>
        /// <returns></returns>
        public SocketError BeginConnect(object UserToken = null)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var args = new SocketAsyncEventArgs()
            {
                UserToken = UserToken,
                RemoteEndPoint = new IPEndPoint(IPAddress.Parse(_ipAddress), _port),
            };
            args.Completed += OnSocketConnected;
            if (!socket.ConnectAsync(args))
            {
                // I/O completed synchronously
                OnSocketConnected(socket, args);
                return args.SocketError;
            }

            return SocketError.InProgress;
        }

        /// <summary>
        /// 异步连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnSocketConnected(object sender, SocketAsyncEventArgs args)
        {
            var socket = sender as Socket;
            bool success = false;
            if (!_closed && (_socket == null || !_socket.Connected))
            {
                if (args.SocketError == SocketError.Success)
                {
                    _socket?.Dispose();
                    _socket = socket;
                    success = true;
                }
            }

            if (success)
            {
                // 开启消息读取.
                _bytesToReceive = _tcpMessageConfig.MessageHeaderSize;
                StartReceive();
            }
            else
            {
                try
                {
                    if (socket.Connected)
                    {
                        socket.Shutdown(SocketShutdown.Both);
                    }
                }
                catch
                {
                    // socket.Shutdown 抛出异常可以忽略
                }
                finally
                {
                    socket.Dispose();
                }
            }
            onConnectComplete?.Invoke(this, args);
            args.Dispose();
        }

        private void StartReceive()
        {
            var args = new SocketAsyncEventArgs();
            try
            {
                if (_receiveBuffer == null)
                {
                    _receiveBuffer = _bufferManager.TakeBuffer(_tcpMessageConfig.ReceiveBufferSize);
                }
                args.SetBuffer(_receiveBuffer, _bytesReceived, _bytesToReceive - _bytesReceived);
                args.Completed += OnReadComplete;
                if (!_socket.ReceiveAsync(args))
                {
                    ProcessReceive(args);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("StartReceive" + ex);
            }
        }

        /// <summary>
        /// 数据读取完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnReadComplete(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                ProcessReceive(e);
            }
            catch (Exception xe)
            {
                Console.WriteLine(xe);
            }
        }

        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="e"></param>
        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            try
            {
                // check if the remote host closed the connection  
                int bytesTransferred = e.BytesTransferred;

                if (bytesTransferred == 0)
                {
                    return;
                }

                _bytesReceived += bytesTransferred;

                if (_bytesReceived < _bytesToReceive)
                {
                    StartReceive();
                    return;
                }

                if (_incomingMessageSize < 0)
                {
                    _incomingMessageSize = getMessageSize();
                    if (_incomingMessageSize > 0)
                    {
                        if (_tcpMessageConfig.IsContainHead)
                        {
                            _bytesToReceive = _incomingMessageSize;
                        }
                        else
                        {
                            _bytesToReceive = _incomingMessageSize + _tcpMessageConfig.MessageHeaderSize;
                        }

                        StartReceive();
                        return;
                    }
                }

                byte[] buffer = new byte[_bytesToReceive];
                Array.Copy(_receiveBuffer, buffer, buffer.Length);
                onMessageReceived?.Invoke(buffer);
                if (_receiveBuffer != null)
                {
                    _bufferManager.ReturnBuffer(_receiveBuffer);
                    _receiveBuffer = null;
                }

                _bytesReceived = 0;
                _bytesToReceive = _tcpMessageConfig.MessageHeaderSize;
                _incomingMessageSize = -1;

                StartReceive();
            }
            catch (Exception xe)
            {
                Console.WriteLine(xe);
            }
            finally
            {
                e.Dispose();
            }
        }

        private int getMessageSize()
        {
            switch (_tcpMessageConfig.MessageHeaderSizeLength)
            {
                case 2:
                    return _receiveBuffer.ByteToShort(
                        startIndex: _tcpMessageConfig.MessageHeaderSizeStart); //.ByteToShort(MessageHeaderSizeStart);
                case 4:
                    return _receiveBuffer.ByteToInt32(DataFormat4.ABCD, _tcpMessageConfig.MessageHeaderSizeStart);
                default: throw new Exception("无法确认消息体大小");
            }
        }

        public void SendMessage(byte[] buffer)
        {
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            try
            {
                args.SetBuffer(buffer, 0, buffer.Length);
                args.Completed += OnWriteComplete;
                if (!SendAsync(args))
                {
                    //SocketException socketException = new SocketException((int)args.SocketError);

                    //// Get the detailed error message
                    //string errorMessage = socketException.Message;

                    //// Handle the error
                    //Console.WriteLine("Error occurred: " + errorMessage);
                    //// I/O completed synchronously
                    //if (args.BytesTransferred < buffer.Length)
                    //{
                    //    args.Dispose();
                    //}
                    //else
                    //{
                        // success, call Complete
                        OnWriteComplete(null, args);
                    //}
                }
            }
            catch (Exception ex)
            {
                args.Dispose();
                throw;
            }
        }

        public void SendMessage(ArraySegment<byte> buffer)
        {
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            try
            {
                args.SetBuffer(buffer.Array, buffer.Offset, buffer.Count);
                args.Completed += OnWriteComplete;
                if (!SendAsync(args))
                {
                    // I/O completed synchronously
                    //if (args.BytesTransferred < buffer.Count)
                    //{
                    //    args.Dispose();
                    //}
                    //else
                    //{
                        // success, call Complete
                        OnWriteComplete(null, args);
                    //}
                }
            }
            catch (Exception ex)
            {
                args.Dispose();
                throw;
            }
        }

        protected virtual void OnWriteComplete(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success || e.BytesTransferred == 0)
            {
                onSendError?.Invoke(new SocketException((int)e.SocketError));
            }
            e.Dispose();
        }

        public bool SendAsync(SocketAsyncEventArgs args)
        {
            SocketAsyncEventArgs eventArgs = args as SocketAsyncEventArgs;
            if (eventArgs == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            if (_socket == null)
            {
                throw new InvalidOperationException("套接字未连接!");
            }

            return _socket.SendAsync(eventArgs);
        }


        /// <summary>
        /// 是否资源
        /// </summary>
        public void Dispose()
        {
            _closed = true;
            _socket?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}