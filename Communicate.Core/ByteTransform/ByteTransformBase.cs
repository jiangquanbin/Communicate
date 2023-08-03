using System;
using System.Collections.Generic;
using System.Text;

namespace Communicate.Core.ByteTransform
{
    public static class ByteTransformBase
    {
        #region Get Bytes From Value

        public static byte[] TransByte(short value,DataFormat dataFormat) => TransByte(new short[] { value }, dataFormat);

        public static byte[] TransByte(short[] values, DataFormat dataFormat)
        {
            if (values == null) return null;
            byte[] buffer = new byte[values.Length * 2];
            for (int i = 0; i < values.Length; i++)
            {
                byte[] bytes = BitConverter.GetBytes(values[i]);
                if (dataFormat == DataFormat.BigEndian)
                {
                    Array.Reverse(bytes);
                }
                bytes.CopyTo(buffer, 2 * i);
            }
            return buffer;
        }


        public static byte[] TransByte(int value,DataFormat4 dataFormat4) => TransByte(new int[] { value },dataFormat4);

        public static byte[] TransByte(int[] values,DataFormat4 dataFormat4)
        {
            if (values == null) return null;

            byte[] buffer = new byte[values.Length * 4];
            for (int i = 0; i < values.Length; i++)
            {
                ByteTransDataFormat4(BitConverter.GetBytes(values[i]), dataFormat4).CopyTo(buffer, 4 * i);
            }

            return buffer;
        }
        
        public static byte[] TransByte(uint value,DataFormat4 dataFormat4) => TransByte(new uint[] { value },dataFormat4);

        public static byte[] TransByte(uint[] values,DataFormat4 dataFormat4)
        {
            if (values == null) return null;

            byte[] buffer = new byte[values.Length * 4];
            for (int i = 0; i < values.Length; i++)
            {
                ByteTransDataFormat4(BitConverter.GetBytes(values[i]), dataFormat4).CopyTo(buffer, 4 * i);
            }

            return buffer;
        }

        public static byte[] TransByte(long value) => TransByte(new long[] { value },DataFormat8.ABCDEFGH);

        public static byte[] TransByte(long[] values, DataFormat8 dataFormat8)
        {
            if (values == null) return null;

            byte[] buffer = new byte[values.Length * 8];
            for (int i = 0; i < values.Length; i++)
            {
                ByteTransDataFormat8(BitConverter.GetBytes(values[i]), dataFormat8).CopyTo(buffer, 8 * i);
            }

            return buffer;
        }


        public static byte[] TransByte(float value) => TransByte(new float[] { value },DataFormat4.ABCD);

        public static byte[] TransByte(float[] values,DataFormat4 dataFormat4)
        {
            if (values == null) return null;

            byte[] buffer = new byte[values.Length * 4];
            for (int i = 0; i < values.Length; i++)
            {
                ByteTransDataFormat4(BitConverter.GetBytes(values[i]), dataFormat4).CopyTo(buffer, 4 * i);
            }

            return buffer;
        }

        #endregion


        #region 将指定的byte数组转换为指定类型

        /// <summary>
        /// 从Byte数组中提取位数组，length代表位数<br />
        /// Extracts a bit array from a byte array, length represents the number of digits
        /// </summary>
        /// <param name="InBytes">原先的字节数组</param>
        /// <param name="length">想要转换的长度，如果超出自动会缩小到数组最大长度</param>
        /// <returns>转换后的bool数组</returns>
        public static bool[] ByteToBoolArray(this byte[] InBytes, int length)
        {
            if (InBytes == null) return null;

            if (length > InBytes.Length * 8) length = InBytes.Length * 8;
            bool[] buffer = new bool[length];

            for (int i = 0; i < length; i++)
            {
                buffer[i] = BoolOnByteIndex(InBytes[i / 8], i % 8);
            }

            return buffer;
        }
        
        /// <summary>
        /// 将两位byte调换位置后转为short
        /// </summary>
        /// <param name="buffer">数据源</param>
        /// /// <param name="dataFormat">转换样式</param>
        /// <returns></returns>
        public static short ByteToShort(this byte[] buffer,DataFormat dataFormat = DataFormat.BigEndian, int startIndex = 0)
        {
            byte[] bytes = new byte[2];
            Array.Copy(buffer,startIndex,bytes,0,bytes.Length);
            switch (dataFormat)
            {
                case DataFormat.BigEndian:
                    Array.Reverse(bytes);
                    break;
            }
            return BitConverter.ToInt16(bytes,0);
        }

        /// <summary>
        /// 将两位byte调换位置后转为ushort
        /// </summary>
        /// <param name="buffer">数据源</param>
        /// /// <param name="dataFormat">转换样式</param>
        /// <returns></returns>
        public static ushort ByteToUShort(this byte[] buffer, DataFormat dataFormat, int startIndex = 0)
        {
            byte[] bytes = new byte[2];
            Array.Copy(buffer,startIndex,bytes,0,bytes.Length);
            switch (dataFormat)
            {
                case DataFormat.BigEndian:
                    Array.Reverse(bytes);
                    break;
            }
            return BitConverter.ToUInt16(bytes, 0);
        }

        /// <summary>
        /// 将四位byte调换位置后转为Int32
        /// </summary>
        /// <param name="buffer">数据源</param>
        /// <param name="TransDataFormat4">转换样式</param>
        /// <returns></returns>
        public static int ByteToInt32(this byte[] buffer, DataFormat4 TransDataFormat4, int startIndex = 0)
        {
            return BitConverter.ToInt32(ByteTransDataFormat4(buffer, TransDataFormat4, startIndex), 0);
        }

        /// <summary>
        /// 将四位byte调换位置后转为UInt32
        /// </summary>
        /// <param name="buffer">数据源</param>
        /// <param name="TransDataFormat4">转换样式</param>
        /// <returns></returns>
        public static uint ByteToUInt32(this byte[] buffer, DataFormat4 TransDataFormat4, int startIndex = 0)
        {
            return BitConverter.ToUInt32(ByteTransDataFormat4(buffer, TransDataFormat4, startIndex), 0);
        }

        /// <summary>
        /// 将八位byte调换位置后转为Int64
        /// </summary>
        /// <param name="buffer">数据源</param>
        /// <param name="TransDataFormat8">转换样式</param>
        /// <returns></returns>
        public static long ByteToInt64(this byte[] buffer, DataFormat8 TransDataFormat8, int startIndex = 0)
        {
            return BitConverter.ToInt64(ByteTransDataFormat8(buffer, TransDataFormat8, startIndex), 0);
        }

        /// <summary>
        /// 将八位byte调换位置后转为UInt64
        /// </summary>
        /// <param name="buffer">数据源</param>
        /// <param name="TransDataFormat8">转换样式</param>
        /// <returns></returns>
        public static ulong ByteToUInt64(this byte[] buffer, DataFormat8 TransDataFormat8, int startIndex = 0)
        {
            return BitConverter.ToUInt64(ByteTransDataFormat8(buffer, TransDataFormat8, startIndex), 0);
        }

        /// <summary>
        /// 将四位byte转为Float
        /// </summary>
        /// <param name="buffer">数据源</param>
        /// <param name="TransDataFormat4">转换样式</param>
        /// <returns></returns>
        public static float ByteToFloat(this byte[] buffer, DataFormat4 TransDataFormat4, int startIndex = 0)
        {
            return BitConverter.ToSingle(ByteTransDataFormat4(buffer, TransDataFormat4, startIndex), 0);
        }

        /// <summary>
        /// 将八位byte转为Double
        /// </summary>
        /// <param name="buffer">数据源</param>
        /// <param name="TransDataFormat8">转换样式</param>
        /// <returns></returns>
        public static double ByteToDouble(this byte[] buffer, DataFormat8 TransDataFormat8, int startIndex = 0)
        {
            return BitConverter.ToDouble(ByteTransDataFormat8(buffer, TransDataFormat8, startIndex), 0);
        } 

        #endregion

        
        /// <summary>
        /// 获取byte数据类型的第offset位，是否为True<br />
        /// </summary>
        /// <param name="value">byte数值</param>
        /// <param name="offset">索引位置</param>
        /// <returns>结果</returns>
        private static bool BoolOnByteIndex(this byte value, int offset)
        {
            byte temp = GetDataByBitIndex(offset);
            return (value & temp) == temp;
        }
        
        private static byte GetDataByBitIndex(int offset)
        {
            switch (offset)
            {
                case 0: return 0x01;
                case 1: return 0x02;
                case 2: return 0x04;
                case 3: return 0x08;
                case 4: return 0x10;
                case 5: return 0x20;
                case 6: return 0x40;
                case 7: return 0x80;
                default: return 0;
            }
        } 
        
        /// <summary>
        /// 转换为指定顺序
        /// </summary>
        /// <param name="value">需转换的字节数组</param>
        /// <param name="TransDataFormat4">转换的样式</param>
        /// <param name="index">转换数组起始位置</param>
        /// <returns>返回转换后的结果</returns>
        private static byte[] ByteTransDataFormat4(byte[] value, DataFormat4 TransDataFormat4, int index = 0)
        {
            byte[] buffer = new byte[4];
            switch (TransDataFormat4)
            {
                case DataFormat4.ABCD:
                    {
                        buffer[0] = value[index + 3];
                        buffer[1] = value[index + 2];
                        buffer[2] = value[index + 1];
                        buffer[3] = value[index + 0];
                        break;
                    }
                case DataFormat4.BADC:
                    {
                        buffer[0] = value[index + 2];
                        buffer[1] = value[index + 3];
                        buffer[2] = value[index + 0];
                        buffer[3] = value[index + 1];
                        break;
                    }

                case DataFormat4.CDAB:
                    {
                        buffer[0] = value[index + 1];
                        buffer[1] = value[index + 0];
                        buffer[2] = value[index + 3];
                        buffer[3] = value[index + 2];
                        break;
                    }
                case DataFormat4.DCBA:
                    {
                        buffer[0] = value[index + 0];
                        buffer[1] = value[index + 1];
                        buffer[2] = value[index + 2];
                        buffer[3] = value[index + 3];
                        break;
                    }
            }
            return buffer;
        }

        /// <summary>
        /// 转换为指定顺序
        /// </summary>
        /// <param name="value">需转换的字节数组</param>
        /// <param name="TransDataFormat8">转换的样式</param>
        /// <param name="index">转换起始位置</param>
        /// <returns>返回转换后的结果</returns>
        private static byte[] ByteTransDataFormat8(byte[] value, DataFormat8 TransDataFormat8, int index = 0)
        {
            byte[] buffer = new byte[8];
            switch (TransDataFormat8)
            {
                case DataFormat8.ABCDEFGH:
                    {
                        buffer[0] = value[index + 7];
                        buffer[1] = value[index + 6];
                        buffer[2] = value[index + 5];
                        buffer[3] = value[index + 4];
                        buffer[4] = value[index + 3];
                        buffer[5] = value[index + 2];
                        buffer[6] = value[index + 1];
                        buffer[7] = value[index + 0];
                        break;
                    }
                case DataFormat8.BADCFEHG:
                    {
                        buffer[0] = value[index + 6];
                        buffer[1] = value[index + 7];
                        buffer[2] = value[index + 4];
                        buffer[3] = value[index + 5];
                        buffer[4] = value[index + 2];
                        buffer[5] = value[index + 3];
                        buffer[6] = value[index + 0];
                        buffer[7] = value[index + 1];
                        break;
                    }

                case DataFormat8.GHEFCDAB:
                    {
                        buffer[0] = value[index + 1];
                        buffer[1] = value[index + 0];
                        buffer[2] = value[index + 3];
                        buffer[3] = value[index + 2];
                        buffer[4] = value[index + 5];
                        buffer[5] = value[index + 4];
                        buffer[6] = value[index + 7];
                        buffer[7] = value[index + 6];
                        break;
                    }
                case DataFormat8.HGFEDCBA:
                    {
                        buffer[0] = value[index + 0];
                        buffer[1] = value[index + 1];
                        buffer[2] = value[index + 2];
                        buffer[3] = value[index + 3];
                        buffer[4] = value[index + 4];
                        buffer[5] = value[index + 5];
                        buffer[6] = value[index + 6];
                        buffer[7] = value[index + 7];
                        break;
                    }
                case DataFormat8.CDABGHEF:
                    {
                        buffer[0] = value[index + 5];
                        buffer[1] = value[index + 4];
                        buffer[2] = value[index + 7];
                        buffer[3] = value[index + 6];
                        buffer[4] = value[index + 1];
                        buffer[5] = value[index + 0];
                        buffer[6] = value[index + 3];
                        buffer[7] = value[index + 2];
                        break;
                    }
            }
            return buffer;
        }

    }

}
