using System;
using communicate;
using Communicate.Core;
using communicate.Entities;
using ModbusCommunicate;

namespace ModBusCommunication
{
    public class ModbusServer : CommunicationServer
    {
        /// <summary>
        /// 添加设备
        /// </summary>
        /// <param name="IpAddress">PLC IP地址</param>
        /// <param name="Port">PLC 端口</param>
        /// <param name="PLCType">PLC类型</param>
        public void AddDrive(long id,string ipAddress, int port, byte station)
        {
            ModbusDrive drive = new ModbusDrive(id,ipAddress, port, station);
            AddDrive(drive);
        }
        
        public void AddItem(int DriverIndex, string Address, string AddressType, int AddressID)
        {
            AddressType = AddressType.ToUpper();
            Address = Address.ToUpper();
            ModbusItem item = new ModbusItem(){ Address = Address, AddressType = AddressType, ID = AddressID};
            AddItem(DriverIndex, item);
        }

        public override object Read(long DriveIndex, int TagIndex)
        {
            throw new NotImplementedException();
        }
    }
}