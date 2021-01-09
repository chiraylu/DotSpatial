using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotSpatial.Plugins.WebMap
{
    /// <summary>
    /// 公共地图
    /// </summary>
    public enum KnownMaps
    {
        EsriHydroBaseMap, 
        EsriWorldStreetMap, 
        EsriWorldImagery, 
        EsriWorldTopo, 
        BingHybrid, 
        BingAerial,
        GoogleLabel,
        GoogleLabelSatellite,
        GoogleLabelTerrain,
        GoogleMap,
        GoogleSatellite,
        GoogleTerrain,
        /// <summary>
        /// 天地图影像
        /// </summary>
        TianDiTuSatellite
    }
}
