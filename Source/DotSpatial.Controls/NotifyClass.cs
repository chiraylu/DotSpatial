using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotSpatial.Controls
{
    /// <summary>
    /// 通知类
    /// </summary>
    public abstract class NotifyClass : INotifyPropertyChanged
    {
        /// <summary>
        /// 属性改变事件
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 设置值并调用属性改变通知
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="t">变量</param>
        /// <param name="value">值</param>
        /// <param name="propertyName">属性名称</param>
        public void SetProperty<T>(ref T t, T value, string propertyName)
        {
            if (!Equals(t, value))
            {
                t = value;
                OnPropertyChanged(propertyName);
            }
        }

        /// <summary>
        /// 属性更改方法
        /// </summary>
        /// <param name="propertyName">属性名</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
