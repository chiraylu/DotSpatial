using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotSpatial.Symbology
{
    public interface IMarkerStroke:ICartographicStroke
    {
        IPointSymbolizer Marker { get; set; }
    }
}
