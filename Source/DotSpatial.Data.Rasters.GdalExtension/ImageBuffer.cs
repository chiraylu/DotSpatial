using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotSpatial.Data.Rasters.GdalExtension
{
    /// <summary>
    /// 图片缓存
    /// </summary>
    public class ImageBuffer
    {
        /// <summary>
        /// 世界范围
        /// </summary>
        public Extent Extent { get; set; }
        /// <summary>
        /// 图片范围
        /// </summary>
        public Rectangle Rectangle { get; set; }
        private Bitmap _bitmap;
        /// <summary>
        /// 图片
        /// </summary>
        public Bitmap Bitmap
        {
            get { return _bitmap; }
            set 
            {
                if (_bitmap != value)
                {
                    _bitmap?.Dispose();
                    _bitmap = value;
                }
            }
        }

    }
}
