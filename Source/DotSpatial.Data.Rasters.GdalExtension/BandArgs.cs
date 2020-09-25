using OSGeo.GDAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotSpatial.Data.Rasters.GdalExtension
{
    /// <summary>
    /// 波段参数
    /// </summary>
    public class BandArgs
    {
        /// <summary>
        /// 数据类型
        /// </summary>
        public DataType DataType { get; set; }
        /// <summary>
        /// 波段数量
        /// </summary>
        public int BandCount { get; set; }
    }
}
