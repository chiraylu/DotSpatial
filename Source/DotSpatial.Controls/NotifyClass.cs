using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotSpatial.Controls
{
    public abstract class NotifyClass : INotifyPropertyChanged
    {
        /// <summary>
        /// 设置值并调用属性改变通知
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <param name="value"></param>
        public void SetProperty<T>(ref T t, T value, string propertyName)
        {
            if (!Equals(t, value))
            {
                t = value;
                OnPropertyChanged(propertyName);
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
