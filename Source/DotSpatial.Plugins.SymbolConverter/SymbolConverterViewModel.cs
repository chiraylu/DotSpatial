using DotSpatial.Controls;
using DotSpatial.Symbology;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotSpatial.Plugins.SymbolConverter
{
    /// <summary>
    /// 符号转换视图模型
    /// </summary>
    public class SymbolConverterViewModel : NotifyClass
    {
        private double _pixelSize;
        /// <summary>
        /// 像素大小
        /// </summary>
        public double PixelSize
        {
            get { return _pixelSize; }
            set { SetProperty(ref _pixelSize, value, nameof(PixelSize)); }
        }
        private double _worldSize;
        /// <summary>
        /// 世界大小
        /// </summary>
        public double WorldSize
        {
            get { return _worldSize; }
            set { SetProperty(ref _worldSize, value, nameof(WorldSize)); }
        }
    }
}
