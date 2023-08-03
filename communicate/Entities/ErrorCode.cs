using System;
using System.Collections.Generic;
using System.Text;

namespace ModbusCommunicate.Entities
{
   
    public class ErrorCode
    {
        /// <summary>
        /// 非法功能
        /// </summary>
        public const string IllegalFunctionCode = "非法功能: 在请求中接收的功能代码不是从设备的一个授权操作。从设备可能处于错误状态，无法处理特定请求。";
        /// <summary>
        /// 非法数据地址
        /// </summary>
        public const string IllegalDataAddress = "非法数据地址: 从设备接收的数据地址不是从设备的一个授权地址。";
        /// <summary>
        /// 非法数据值
        /// </summary>
        public const string IllegalDataValue = "非法数据值: 在请求数据栏中的数值不是从设备的一个授权值。";
        /// <summary>
        /// 从设备故障
        /// </summary>
        public const string SecondaryEquipmentFailure = "从设备故障: 从设备未能执行一个请求的操作，因为出现了一个无法修复的错误。";
        /// <summary>
        /// 确认
        /// </summary>
        public const string Notarize = "确认: 从设备接受了请求，但是需要较长的时间来处理它。";
        /// <summary>
        /// 从设备繁忙
        /// </summary>
        public const string SlaveEquipmentBusy = "从设备繁忙: 从设备忙于处理另一个命令。主设备必须在从设备空闲后发送请求。";
        /// <summary>
        /// 否定确认
        /// </summary>
        public const string NegativeAcknowledgementNck = "否定确认: 从设备无法执行主设备发送的编程请求。";
        /// <summary>
        /// 存储器奇偶校验错误
        /// </summary>
        public const string MemoryParityError = "存储器奇偶校验错误: 从设备在尝试读取扩展存储器的时候从存储器中检测到一个奇偶校验错误。";
        /// <summary>
        /// 网关通道不可用
        /// </summary>
        public const string TheGatewayChannelIsUnavailable = "网关通道不可用: 网关过载，或者没有正确配置。";
        /// <summary>
        /// 网关目标设备未能晌应
        /// </summary>
        public const string TheGatewayTargetDeviceFailedToRespond = "网关目标设备未能晌应: 在网络中不存在从设备。";
    }
}
