using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Communicate.Core;
using Communicate.Core.ByteTransform;
using Communicate.Core.Entities;
using Communicate.Core.SocketMessage;
using communicate.Entities;
using ModbusCommunicate.Entities;

namespace ModbusCommunicate
{
    public class ModbusDrive : EquipmentDrive
    {
        public byte Station { get; set; }

        public ModbusDrive(long id, string ipAddress, int port, byte station) : this(id, ipAddress, port, station, null)
        {

        }

        public ModbusDrive(long id, string ipAddress, int port, byte station, ModbusConfigOperation configOperation) : base(ipAddress, port, 0, 2)
        {
            ID = id;
            this.Station = station;
            _configOperation = configOperation ?? new ModbusConfigOperation();
            _tcpMessageConfig = new TcpMessageConfig()
            {
                ReceiveBufferSize = ushort.MaxValue,
                MessageHeaderSize = 6,
                MessageHeaderSizeStart = 4,
                MessageHeaderSizeLength = 2
            };
        }

        protected override void OrderAddress()
        {
            List<ModbusItem> modbusAddresses = Items.Where(o => o.Address.ToLower() != "_system._error")
                .OrderBy(o => ((ModbusItem)o).FunctionCode)
                .ThenBy(o => Convert.ToInt32(((ModbusItem)o).AddressStart))
                .Cast<ModbusItem>().ToList();
            List<ModbusItem> ReadAddress = new List<ModbusItem>();

            int StarAddress = 0;
            int FunctionCode = 0;
            int AddressLength = 0;
            for (int i = 0; i < modbusAddresses.Count; i++)
            {
                bool IsBlack = false;
                if (ReadAddress.Count == 0)
                {
                    StarAddress = modbusAddresses[i].AddressStart;
                    FunctionCode = modbusAddresses[i].FunctionCode;
                    AddressLength = 0;
                }
                ReadAddress.Add(modbusAddresses[i]);
                if (i != modbusAddresses.Count - 1)
                {
                    if (FunctionCode == 1 || FunctionCode == 2)
                    {
                        AddressLength = modbusAddresses[i + 1].AddressStart - StarAddress;
                        if (AddressLength > ((ModbusConfigOperation)_configOperation).MaxCoilSize * 8)
                        {
                            IsBlack = true;
                        }
                    }

                    if (FunctionCode == 3 || FunctionCode == 4)
                    {
                        AddressLength = modbusAddresses[i + 1].AddressStart - StarAddress;
                        if (AddressLength > ((ModbusConfigOperation)_configOperation).MaxRegisterSize * 2)
                        {
                            IsBlack = true;
                        }
                    }

                    if (FunctionCode != modbusAddresses[i + 1].FunctionCode)
                    {
                        IsBlack = true;
                    }
                }


                if (IsBlack || i == modbusAddresses.Count - 1)
                {

                    BlackItem blackItem = new BlackItem();
                    blackItem.Items.AddRange(ReadAddress.ToList());
                    blackItem.SendBuffer = BuildReadCommandHead().Concat(BuildReadCommand(ReadAddress.ToArray()))
                        .ToArray();

                    BlackItems.Add(blackItem);
                    ReadAddress.Clear();
                }
            }
        }

        protected override bool AnalysisSocketData(byte[] readData, BlackItem blackItem)
        {
            try
            {
                //BlackItem blackAddress = BlackItems[CurSendBlackIndex];
                int starAddress = Convert.ToInt32(blackItem.Items[0].AddressStart);
                if (readData[7] == 0x80 + ((ModbusItem)blackItem.Items[0]).FunctionCode)
                {
                    switch (readData[8])
                    {
                        case 0x01:
                            Console.WriteLine(ErrorCode.IllegalFunctionCode);
                            break;
                        case 0x02:
                            Console.WriteLine(ErrorCode.IllegalDataAddress);
                            break;
                        case 0x03:
                            Console.WriteLine(ErrorCode.IllegalDataValue);
                            break;
                        case 0x04:
                            Console.WriteLine(ErrorCode.SecondaryEquipmentFailure);
                            break;
                        case 0x05:
                            Console.WriteLine(ErrorCode.Notarize);
                            break;
                        case 0x06:
                            Console.WriteLine(ErrorCode.SlaveEquipmentBusy);
                            break;
                        case 0x07:
                            Console.WriteLine(ErrorCode.NegativeAcknowledgementNck);
                            break;
                        case 0x08:
                            Console.WriteLine(ErrorCode.MemoryParityError);
                            break;
                        case 0x0A:
                            Console.WriteLine(ErrorCode.TheGatewayChannelIsUnavailable);
                            break;
                        case 0x0B:
                            Console.WriteLine(ErrorCode.TheGatewayTargetDeviceFailedToRespond);
                            break;
                        default:
                            break;
                    }
                    foreach (var item in blackItem.Items)
                    {
                        item.Quality = 198;
                        item.ItemValue = "";
                    }
                    return false;
                }

                foreach (var item in blackItem.Items)
                {
                    int starIndex = 0;
                    int TypeLength = item.AddressTypeLength;
                    if (((ModbusItem)item).FunctionCode == 3 || ((ModbusItem)item).FunctionCode == 4)
                    {
                        starIndex = 9 + (Convert.ToInt32(item.AddressStart) - starAddress) * 2;
                    }
                    else
                    {
                        starIndex = 9 + Convert.ToInt32(item.AddressStart) / 8;
                        TypeLength = 1;
                    }

                    byte[] buffer = new byte[TypeLength];
                    Array.Copy(readData, starIndex, buffer, 0, TypeLength);
                    item.Quality = 192;
                    item.ItemValue = AnalysisDataValue(item.AddressType, buffer, item.IntXH);
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        private object AnalysisDataValue(string AddressType, byte[] buffer, int HexIndex)
        {
            try
            {
                switch (AddressType)
                {
                    case "BOOL":
                        bool[] bools = buffer.ByteToBoolArray(16);
                        return bools[HexIndex];
                    case "REAL":
                        return buffer.ByteToFloat(dataFormat4);
                    case "WORD":
                    case "SHORT":
                        return buffer.ByteToShort();
                    case "DWORD":
                        return buffer.ByteToUInt32(dataFormat4);
                    case "DOUBLE":
                        return buffer.ByteToDouble(dataFormat8);
                    default:
                        break;
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception("AnalysisDataValue:" + ex.Message);
            }
        }

        protected override byte[] BuildReadCommandHead(params object[] param)
        {
            byte[] buffer = new byte[6];
            buffer[0] = 0x00;
            buffer[1] = 0x00;
            buffer[4] = BitConverter.GetBytes(6)[1];
            buffer[5] = BitConverter.GetBytes(6)[0];
            return buffer;
        }

        protected override byte[] BuildReadCommand(Item[] items)
        {
            try
            {
                byte[] command = BitConverter.GetBytes(items[0].AddressStart);
                byte[] result = BitConverter.GetBytes(items[items.Length - 1].AddressStart - items[0].AddressStart +
                                                      items[items.Length - 1].AddressTypeLength / 2);
                byte[] buffer = new byte[6];
                buffer[0] = Station;
                buffer[1] = (byte)((ModbusItem)items[0]).FunctionCode;
                buffer[2] = command[1];
                buffer[3] = command[0];
                buffer[4] = result[1];
                buffer[5] = result[0];

                return buffer;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        protected override byte[] BuildWriteCommand(Item[] items)
        {
            throw new System.NotImplementedException();
        }

        protected override void CreateHandshakeProtocol()
        {
            IsConnection = true;
        }

        public override Item BuildItem(Item item)
        {
            if (item.Address.ToLower() == "_system._error")
            {
                return item;
            }

            ModbusItem modbusItem = item as ModbusItem;
            string address = modbusItem.Address.PadLeft(5, '0');
            if (address[0] == '4')
            {
                modbusItem.FunctionCode = 3;
            }
            else
            {
                modbusItem.FunctionCode = address[0] + 1 - '0';
            }

            if (address.IndexOf('.') > 0)
            {
                string[] strSplit = address.Split('.');
                modbusItem.IntXH = Convert.ToInt32(strSplit[1]);
                address = strSplit[0];
            }

            modbusItem.AddressStart = Convert.ToInt32(address.Substring(1)) - 1;
            if (modbusItem.FunctionCode == 1 || modbusItem.FunctionCode == 2)
            {
                modbusItem.IntXH = Convert.ToInt32(modbusItem.AddressStart) % 8;
            }

            return modbusItem;
        }
    }
}