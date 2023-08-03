using System;
using System.Collections.Generic;
using System.Text;
using Communicate.Core.ByteTransform;

namespace Communicate.Core.Entities
{
    public class BlackItem
    {
        public int RequestId;
        public bool State = true;
        public List<Item> Items = new List<Item>();
        public byte[] SendBuffer;
        public DataFormat4 DataFormat4;
        public DataFormat8 DataFormat8;
        public int ErrorSendCount;
        public int NoReceivedCount;
        public DateTime LastSendDate;
    }
}
