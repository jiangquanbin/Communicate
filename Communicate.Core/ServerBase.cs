using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Communicate.Core.Entities;

namespace Communicate.Core
{
    public abstract class CommunicationServer : IDisposable
    {
        public delegate void OnDataChange(List<MonitorItem> Tag);
        public event OnDataChange onDataChange;

        public object _lock = new object();
        public List<MonitorItem> stayMonitorItems = new List<MonitorItem>();
        protected List<EquipmentDrive> _Channels = new List<EquipmentDrive>();

        private ILogger _logger;

        /// <summary>
        /// 发布变化值时间(单位毫秒)
        /// </summary>
        public int publishTime = 100;
        public Thread publishThread;

        private Thread BeatThread;
        private int _beatInterval = 1000;

        private bool _dispose = false;

        public CommunicationServer(ILogger logger = null)
        {
            _logger = logger;
        }

        protected virtual void AddDrive(EquipmentDrive drive)
        {
            _Channels.Add(drive);
        }

        protected void AddItem(long DriverId, Item item)
        {
            var channel = _Channels.FirstOrDefault(o => o.ID == DriverId);
            if (channel != null)
            {
                item.AddressTypeLength = channel.GetTypeLength(item.AddressType);
                item = channel.BuildItem(item);
                item.PropertyChanged += Item_OnDataChanged;
                channel.Items.Add(item);
            }
        }

        public void Connection()
        {
            try
            {
                for (int i = 0; i < _Channels.Count; i++)
                {
                    _Channels[i].Start();
                }
                publishThread = new Thread(publishDataChange);
                publishThread.IsBackground = true;
                publishThread.Start();

                BeatThread = new Thread(Beat);
                BeatThread.IsBackground =true;
                BeatThread.Start();
            }
            catch (Exception e)
            {
                throw new Exception("连接PLC地址失败" + e.Message);
            }
        }

        public void publishDataChange()
        {
            while (true)
            {
                try
                {
                    if (_dispose)
                    {
                        break;
                    }
                    List<MonitorItem> publishMonitorItems = new List<MonitorItem>();

                    lock (_lock)
                    {
                        if (stayMonitorItems.Count > 0)
                        {
                            publishMonitorItems.AddRange(new List<MonitorItem>(stayMonitorItems));
                            stayMonitorItems.Clear();
                        }
                    }
                    if (publishMonitorItems.Count > 0 && onDataChange != null)
                    {
                        Thread thread = new Thread(()=> onDataChange(publishMonitorItems));
                        thread.IsBackground = true;
                        thread.Start();
                    }

                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    Thread.Sleep(publishTime);
                }
            }
        }

        private void Beat()
        {
            while (true)
            {
                try
                {
                    if (_dispose)
                    {
                        break;
                    }

                    foreach (EquipmentDrive channel in _Channels)
                    {
                        if (Ping(channel.IPAddress))
                        {
                            foreach (Item statusItem in channel.StatusItems)
                            {
                                statusItem.ItemValue = 192;
                            }
                        }
                        else
                        {
                            foreach (Item statusItem in channel.StatusItems)
                            {
                                statusItem.ItemValue = 198;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
                finally
                {
                    Thread.Sleep(_beatInterval);
                }
            }
        }

        private void Item_OnDataChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Item item = (Item)sender;
            lock (_lock)
            {
                MonitorItem monitorItem1 = stayMonitorItems.FirstOrDefault(o => o.id == item.ID);
                if (monitorItem1 == null)
                {
                    MonitorItem monitorItem = new MonitorItem() { id = item.ID, ItemValue = item.ItemValue, Quality = item.Quality, UpdateDate = item.UpdateDate };
                    stayMonitorItems.Add(monitorItem);
                }
                else
                {
                    monitorItem1.UpdateDate = item.UpdateDate;
                    monitorItem1.Quality = item.Quality;
                    monitorItem1.ItemValue = item.ItemValue;
                }
            }
        }



        private bool Ping(string ip)
        {
            Ping ping = new Ping();
            PingOptions pingOptions = new PingOptions();
            pingOptions.DontFragment = true;
            string data = "Test Data";
            byte[] bytes = Encoding.ASCII.GetBytes(data);
            int timeout = 1000;
            PingReply pingReply = ping.Send(ip,timeout,bytes,pingOptions);
            if (pingReply.Status == IPStatus.Success)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 读取PLC数据
        /// </summary>
        /// <param name="DriveIndex">设备编号</param>
        /// <param name="TagIndex">PLC地址所在设备编号</param>
        /// <returns></returns>
        public abstract object Read(long DriveId, int TagIndex);

        public void Dispose()
        {
            _dispose = true;
            foreach (var channel in _Channels)
            {
                channel.Dispose();
            }
            _Channels.Clear();
        }
    }
}