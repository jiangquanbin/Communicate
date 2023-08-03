using System;
using System.Collections.Generic;
using System.Text;

namespace Communicate.Core.Entities
{
    public class TCPEquipment: Equipment
    {
        public string IpAddress;
        public int Port;
        public string MACAddress;
    }
}
