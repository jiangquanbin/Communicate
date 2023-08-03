using Communicate.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModbusCommunicate.Entities
{
    public class ModbusConfigOperation:ConfigOperation
    {
        /// <summary>
        /// 最大连续读取寄存器数量
        /// </summary>
        public int MaxRegisterSize = 32;
        /// <summary>
        /// 最大连续读取线圈数量
        /// </summary>
        public int MaxCoilSize = 32;

    }
}
