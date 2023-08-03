using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Timers;
using Communicate.Core.ByteTransform;
using Communicate.Core.Entities;
using Communicate.Core.SocketMessage;

namespace Communicate.Core
{
    public abstract class EquipmentDrive : IDisposable
    {
        /// <summary>
        /// 接收等待事件
        /// </summary>
        private ManualResetEvent ReceiveTimeOut = new ManualResetEvent(false);

        /// <summary>
        /// 设备ID
        /// </summary>
        public long ID;

        /// <summary>
        /// 设备IP地址
        /// </summary>
        public string IPAddress;

        /// <summary>
        /// 设备端口号
        /// </summary>
        public int Port;

        /// <summary>
        /// 设备集合
        /// </summary>
        public List<Item> Items = new List<Item>();

        /// <summary>
        /// 整合数据块
        /// </summary>
        public List<BlackItem> BlackItems = new List<BlackItem>();

        /// <summary>
        /// 状态点位
        /// </summary>
        public List<Item> StatusItems = new List<Item>();

        /// <summary>
        /// 连接客户端
        /// </summary>
        public TCPMessageSocket _Client;
        /// <summary>
        /// 是否在进行重连防止网络延迟连续发送冲突
        /// </summary>
        private bool Reconnection = false;

        /// <summary>
        /// 是否连接成功, 防止存在TCP连接后存在握手协议(完整握手后判定连接成功)
        /// </summary>
        public bool IsConnection;

        /// <summary>
        /// 当前发送消息块下标
        /// </summary>
        public int CurSendBlackIndex = 0;

        /// <summary>
        /// 扫描PLC数据线程
        /// </summary>
        private Thread _PLCSendThread;

        /// <summary>
        /// 设备配置参数
        /// </summary>
        protected ConfigOperation _configOperation;

        /// <summary>
        /// 大端开始小端字节
        /// 默认大端字节
        /// </summary>
        protected DataFormat _dataFormat = DataFormat.BigEndian;

        /// <summary>
        /// 4字节顺序
        /// </summary>
        public DataFormat4 dataFormat4 = DataFormat4.ABCD;

        /// <summary>
        /// 8字节顺序
        /// </summary>
        public DataFormat8 dataFormat8 = DataFormat8.ABCDEFGH;

        /// <summary>
        /// 是否已是否资源
        /// </summary>
        private bool _Disposable = false;

        /// <summary>
        /// 设备TCP消息参数
        /// </summary>
        protected TcpMessageConfig _tcpMessageConfig;

        /// <summary>
        /// 发送消息等待响应队列
        /// </summary>
        protected SortedDictionary<uint, BlackItem> m_queuedResponses = new SortedDictionary<uint, BlackItem>();

        /// <summary>
        /// 发送标识符
        /// </summary>
        private long RequestIdentifier;

        private int _IdentifierStarIndex;
        private int _IdentifierLength;

        public EquipmentDrive(string ipAddress, int port, int IdentifierStarIndex, int IdentifierLength)
        {
            IPAddress = ipAddress;
            Port = port;
            _IdentifierStarIndex = IdentifierStarIndex;
            _IdentifierLength = IdentifierLength;
        }

        public void AddEquipment(Equipment equipment, string dataFormat4 = "CDAB", string dataFormat8 = "CDABGHEF")
        {
            _configOperation = new ConfigOperation();
            ID = equipment.EquipmetlNo;
            switch (dataFormat4)
            {
                case "ABCD":
                    equipment.DataFormat4 = DataFormat4.ABCD;
                    break;
                case "BADC":
                    equipment.DataFormat4 = DataFormat4.BADC;
                    break;
                case "DCBA":
                    equipment.DataFormat4 = DataFormat4.DCBA;
                    break;
                case "CDAB":
                    equipment.DataFormat4 = DataFormat4.CDAB;
                    break;
                default:
                    equipment.DataFormat4 = DataFormat4.CDAB;
                    break;
            }

            switch (dataFormat8)
            {
                case "ABCDEFGH":
                    equipment.DataFormat8 = DataFormat8.ABCDEFGH;
                    break;
                case "BADCFEHG":
                    equipment.DataFormat8 = DataFormat8.BADCFEHG;
                    break;
                case "GHEFCDAB":
                    equipment.DataFormat8 = DataFormat8.GHEFCDAB;
                    break;
                case "HGFEDCBA":
                    equipment.DataFormat8 = DataFormat8.HGFEDCBA;
                    break;
                case "CDABGHEF":
                    equipment.DataFormat8 = DataFormat8.CDABGHEF;
                    break;
                default:
                    equipment.DataFormat8 = DataFormat8.CDABGHEF;
                    break;
            }
        }


        /// <summary>
        /// 获取协议类型字节长度
        /// </summary>
        /// <param name="PointType"></param>
        /// <returns></returns>
        public virtual ushort GetTypeLength(string PointType)
        {
            switch (PointType)
            {
                case "BIT":
                case "BYTE":
                case "CHAR":
                    return 1;
                case "BOOL": // bool类型为两字节读取
                case "WORD":
                case "INT":
                case "USHORT":
                case "SHORT":
                    return 2;
                case "REAL":
                case "FLOAT":
                case "DWORD":
                case "DINT":
                    return 4;
                case "DOUBLE":
                case "LONG":
                    return 8;
                default:
                    return 2;
            }
        }

        /// <summary>
        /// 构建读取地址结构
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public abstract Item BuildItem(Item item);

        public virtual void Start()
        {
            try
            {
                StatusItems = Items.Where(o => o.Address.ToLower() == "_system._error").ToList();
                OrderAddress();
                ConnectionSocket();
                //启动发送进程
                _PLCSendThread = new Thread(SendDataScan);
                _PLCSendThread.IsBackground = true;
                _PLCSendThread.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine("连接失败" + ex.Message);
            }
        }

        /// <summary>
        /// 连接Socket
        /// </summary>
        private void ConnectionSocket()
        {
            try
            {
                _Client = new TCPMessageSocket(IPAddress, Port, _tcpMessageConfig);
                _Client.onConnectComplete += Client_onConnectComplete;
                _Client.onSendError += Client_onSendComplete;
                _Client.onMessageReceived += Client_onMessageReceived;
                _Client.BeginConnect();
            }
            catch (Exception ex)
            {
                IsConnection = false;
                Console.WriteLine(ex.Message);
            }
        }

        private void Client_onSendComplete(Exception e)
        {
            if (IsConnection)
            {
                IsConnection = false;
                Console.WriteLine(e);
            }
        }

        /// <summary>
        /// 扫描发送PLC数据
        /// </summary>
        private void SendDataScan()
        {
            while (true)
            {
                try
                {
                    if (_Disposable)
                    {
                        break;
                    }

                    if (IsConnection)
                    {
                        var blackItem = LoadSendBlack(CurSendBlackIndex);
                        if (blackItem != null)
                        {

                            var RequestIdentifier = BuildSendIdentifier(blackItem);
                            ReceiveTimeOut.Reset();
                            _Client.SendMessage(blackItem.SendBuffer);
                            blackItem.LastSendDate = DateTime.Now;
                            if (!ReceiveTimeOut.WaitOne(_configOperation.ReceiveTimeout))
                            {
                                m_queuedResponses.Remove(RequestIdentifier);
                                if (_configOperation.Demotion && blackItem.State)
                                {
                                    blackItem.ErrorSendCount++;
                                    if (blackItem.ErrorSendCount > _configOperation.ErrorRequestCount)
                                    {
                                        blackItem.State = false;
                                    }
                                }
                                Console.WriteLine("接收超时!");
                            }
                        }


                    }
                    else
                    {
                        if (!Reconnection)
                        {
                            Reconnection = true;
                            _Client.BeginConnect();
                            Thread.Sleep(1000);
                        }
                    }
                }
                catch (Exception err)
                {
                    Console.WriteLine("PLCSendData" + err.Message);
                    IsConnection = false;
                }
                finally
                {
                    Thread.Sleep(100);
                }
            }
        }

        /// <summary>
        /// 数据发送之前
        /// </summary>
        /// <param name="blackItem"></param>
        private uint BuildSendIdentifier(BlackItem blackItem)
        {
            byte[] bytes = new byte[_IdentifierLength];
            switch (_IdentifierLength)
            {
                case 2:
                    Interlocked.CompareExchange(ref RequestIdentifier, 0, ushort.MaxValue);
                    Interlocked.Increment(ref RequestIdentifier);
                    bytes = ByteTransformBase.TransByte((short)RequestIdentifier, _dataFormat);
                    break;
                case 4:
                    Interlocked.CompareExchange(ref RequestIdentifier, 0, uint.MaxValue);
                    Interlocked.Increment(ref RequestIdentifier);
                    bytes = ByteTransformBase.TransByte((uint)RequestIdentifier, dataFormat4);
                    break;
            }

            Array.Copy(bytes, 0, blackItem.SendBuffer, _IdentifierStarIndex, _IdentifierLength);
            m_queuedResponses.Add((uint)RequestIdentifier, blackItem);
            return (uint)RequestIdentifier;
        }

        /// <summary>
        /// 查找可发送块
        /// </summary>
        /// <param name="RequestID"></param>
        /// <returns></returns>
        private BlackItem LoadSendBlack(int RequestID)
        {
            CurSendBlackIndex++;
            if (CurSendBlackIndex >= BlackItems.Count)
            {
                CurSendBlackIndex = 0;
            }
            if (_configOperation.Demotion)
            {
                // 一次循环后无可发送块
                if (CurSendBlackIndex == RequestID)
                {
                    // 循环一次只有上次请求块可发送时再次发送
                    if (BlackItems[CurSendBlackIndex].State)
                    {
                        return BlackItems[CurSendBlackIndex];
                    }
                    // 无任何可发送块返回null
                    return null;
                }

                if (BlackItems[CurSendBlackIndex].State || 
                    (!_configOperation.DowngradeDiscardRequest && !BlackItems[CurSendBlackIndex].State && DateTime.Now.Subtract(BlackItems[CurSendBlackIndex].LastSendDate).TotalMilliseconds >= _configOperation.ErrorRequestInterval))
                {
                    return BlackItems[CurSendBlackIndex];
                }

                return LoadSendBlack(RequestID);
            }
            else
            {
                return BlackItems[CurSendBlackIndex];
            }

        }

        /// <summary>
        /// 连接完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Client_onConnectComplete(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success)
            {
                CreateHandshakeProtocol();
            }
            else
            {
                Console.WriteLine("连接地址:" + IPAddress + ":" + Port + "失败" + new SocketException((int)args.SocketError));
                IsConnection = false;
            }
            Reconnection = false;
        }

        private void Client_onMessageReceived(byte[] message)
        {
            if (!IsConnection)
            {
                HandshakeProtocolReceived(message);
            }
            else
            {
                uint Identifier = 0;
                switch (_IdentifierLength)
                {
                    case 2:
                        Identifier = message.ByteToUShort(_dataFormat, _IdentifierStarIndex);
                        break;
                    case 4:
                        Identifier = message.ByteToUInt32(dataFormat4, _IdentifierStarIndex);
                        break;
                    default:
                        break;
                }
                BlackItem blackItem = null;
                if (!m_queuedResponses.TryGetValue(Identifier, out blackItem))
                {
                    return;
                }
                m_queuedResponses.Remove(Identifier);
                bool AnalysisState = AnalysisSocketData(message, blackItem);
                // 判断是否为错误块
                if (!AnalysisState)
                {
                    // 是否开启降级处理
                    if (_configOperation.Demotion && blackItem.State)
                    {
                        blackItem.ErrorSendCount++;
                        if (blackItem.ErrorSendCount > _configOperation.ErrorRequestCount)
                        {
                            blackItem.ErrorSendCount = 0;
                            blackItem.State = false;
                        } 
                    }
                }else if (!blackItem.State && !_configOperation.DowngradeDiscardRequest) // 判断如果是错误块并且不进行舍弃时
                {
                    blackItem.State = true;
                }
            }
            ReceiveTimeOut.Set();
        }


        /// <summary>
        /// 整合地址成块一起发送请求
        /// </summary>
        protected abstract void OrderAddress();

        /// <summary>
        /// 单个地址发送读取请求
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public byte[] SingleReadCommand(Item item)
        {
            List<byte> list = BuildReadCommandHead(1).ToList();
            list.AddRange(BuildReadCommand(new[] { item }));
            return list.ToArray();
        }

        /// <summary>
        /// 接收异常回调
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="exception"></param>
        /// <summary>
        /// 握手协议返回结果
        /// </summary>
        /// <param name="handshakeMessage"></param>
        public virtual void HandshakeProtocolReceived(byte[] handshakeMessage)
        {

        }

        /// <summary>
        /// 解析Socket返回消息
        /// </summary>
        /// <param name="blockCommand"></param>
        /// <param name="reallData"></param>
        protected abstract bool AnalysisSocketData(byte[] reallData, BlackItem blackItem);

        /// <summary>
        /// 发送读取字节头
        /// </summary>
        /// <param name="readCount">发送地址数量</param>
        /// <returns></returns>
        protected abstract byte[] BuildReadCommandHead(params object[] param);

        /// <summary>
        /// 发送读取数据字节
        /// </summary>
        /// <param name="items">转换读取地址</param>
        /// <returns></returns>
        protected abstract byte[] BuildReadCommand(Item[] items);

        /// <summary>
        /// 发送写入数据字节
        /// </summary>
        /// <param name="items">转换写入地址</param>
        /// <returns></returns>
        protected abstract byte[] BuildWriteCommand(Item[] items);

        /// <summary>
        /// 连接成功,如存在建立握手协议进行使用
        /// </summary>
        protected virtual void CreateHandshakeProtocol()
        {
            IsConnection = true;
        }

        public void Dispose()
        {
            try
            {
                _Disposable = true;
                Items.Clear();
                BlackItems.Clear();
                _Client.Dispose();
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}