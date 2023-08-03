using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace Communicate.Core.Entities
{
    public class Item
    {
        /// <summary>
        /// 属性值改变事件
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// 属性值改变事件
        /// </summary>
        /// <param name="propertyName"></param>
        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// 标识符
        /// </summary>
        public long ID { get; set; }
        /// <summary>
        /// 通讯地址所在设备下标
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// 通讯地址名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 通讯地址描述
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 通讯地址
        /// </summary>
        public virtual string Address { get; set; }
        /// <summary>
        /// 通讯地址位置
        /// </summary>
        public virtual int AddressStart { get; set; }

        /// <summary>
        /// 所在下标位置
        /// </summary>
        public virtual int IntXH { get; set; }
        /// <summary>
        /// 通讯地址类型
        /// </summary>
        public virtual string AddressType { get; set; }
        /// <summary>
        ///  通讯地址类型长度
        /// </summary>
        public virtual int AddressTypeLength { get; set; }

        public virtual byte[] SendCommand { get; set; }
        /// <summary>
        /// 通讯地址值
        /// </summary>
        private object _ItemValue;
        /// <summary>
        /// 通讯地址值
        /// </summary>
        public object ItemValue
        {
            get
            {
                return _ItemValue;
            }
            set
            {
                if (_ItemValue == null || _ItemValue.ToString() != value.ToString())
                {
                    if (scaling == Scaling.Linear)
                    {
                        object TransValue = linearTransform.TransValue(value, this.AddressType);
                        if (_ItemValue == null || _ItemValue.ToString() != TransValue.ToString())
                        {
                            _ItemValue = TransValue;
                        }
                        else
                        {
                            return;
                        }
                    }
                    else
                    {
                        _ItemValue = value;
                    }
                    UpdateDate = DateTime.Now;
                    NotifyPropertyChanged();
                }
            }
        }
        /// <summary>
        /// 最后一次更新时间
        /// </summary>
        public DateTime UpdateDate { get; set; }
        /// <summary>
        /// 点位质量
        /// </summary>
        public int Quality { get; set; } = 192;
        /// <summary>
        /// 是否线性公式
        /// </summary>
        public Scaling scaling { get; set; }
        /// <summary>
        /// 线性公式
        /// </summary>
        public LinearTransform linearTransform { get; set; }

    }
}
