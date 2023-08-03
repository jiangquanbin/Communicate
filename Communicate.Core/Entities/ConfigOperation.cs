using System;
using System.Collections.Generic;
using System.Security.Permissions;
using System.Text;

namespace Communicate.Core.Entities
{
    /// <summary>
    /// 设备参数
    /// </summary>
    public class ConfigOperation
    {
        /// <summary>
        /// 扫描刷新时间
        /// 默认100毫秒发送
        /// </summary>
        public int IntRefeshTime = 100;
        /// <summary>
        /// 接收超时设置(毫秒)
        /// 默认1000毫秒(1秒钟)
        /// </summary>
        public int ReceiveTimeout = 1000;
        /// <summary>
        /// 接收后到下一次发送间隔(毫秒)
        /// 默认为0
        /// </summary>
        public int SendInterval = 0;

        /// <summary>
        /// 是否进行降级操作默认不进行降级
        /// </summary>
        public bool Demotion = true;
        /// <summary>
        /// 错误请求块请求间隔(毫秒)
        /// 默认为10秒
        /// </summary>
        public int ErrorRequestInterval = 10000;
        /// <summary>
        /// 请求错误达到此次数剔除为错误块
        /// 默认3次后标记为错误块
        /// </summary>
        public int ErrorRequestCount = 3;
        /// <summary>
        /// 错误地址块是否舍弃(开启降级生效)
        /// 默认舍弃
        /// </summary>
        public bool DowngradeDiscardRequest = true;
    }
}