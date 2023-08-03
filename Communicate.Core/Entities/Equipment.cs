using System;
using System.Collections.Generic;
using System.Text;
using Communicate.Core.ByteTransform;

namespace Communicate.Core.Entities
{
    public class Equipment
    {
        public long EquipmetlNo;
        public DataFormat4 DataFormat4;
        public DataFormat8 DataFormat8;
        public List<Item> ModbusAddresses = new List<Item>();

        public List<BlackItem> BlackItems = new List<BlackItem>();

        //public int sendIndex = -1;
        //public List<byte[]> Sendbuffer = new List<byte[]>();
        //public List<List<Item>> blackModbusAddresses = new List<List<Item>>();
        //public int ErrorSendCount = 0;
        //public DateTime LastErrorSendTime = DateTime.Now;
    }
}
