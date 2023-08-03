using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Communicate.Core.ByteTransform
{
    /// <summary>
    /// 数据大小端判断(判断两字节顺序)
    /// </summary>
    public enum DataFormat
    {
        /// <summary>
        /// 大端模式
        /// </summary>
        BigEndian,
        /// <summary>
        /// 小端模式
        /// </summary>
        LittleEndian
    }

    /// <summary>
    /// 4位字节根据指定样式排序
    /// </summary>
    public enum DataFormat4
    {
        /// <summary>
        /// 按照顺序排序
        /// </summary>
        ABCD = 0,
        /// <summary>
        /// 按照单字反转
        /// </summary>
        BADC = 1,
        /// <summary>
        /// 按照双字反转
        /// </summary>
        CDAB = 2,
        /// <summary>
        /// 按照倒序排序
        /// </summary>
        DCBA = 3,
    }

    /// <summary>
    /// 8位字节根据指定样式排序
    /// </summary>
    public enum DataFormat8
    {
        /// <summary>
        /// 按照顺序排序
        /// </summary>
        ABCDEFGH = 0,
        /// <summary>
        /// 按照单字反转
        /// </summary>
        BADCFEHG = 1,
        /// <summary>
        /// 按照双字反转
        /// </summary>
        GHEFCDAB = 2,
        /// <summary>
        /// 按照倒序排序
        /// </summary>
        HGFEDCBA = 3,
        /// <summary>
        /// 四字节一组倒序
        /// </summary>
        CDABGHEF = 4,
    }
}
