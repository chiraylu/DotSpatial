using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotSpatial.Plugins.ShapeEditor
{
    /// <summary>
    /// SnapMode
    /// </summary>
    [Flags]
    public enum SnapMode
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,

        /// <summary>
        /// Point
        /// </summary>
        Point = 1,

        /// <summary>
        /// End
        /// </summary>
        End = 2,

        /// <summary>
        /// Vertex
        /// </summary>
        Vertex = 4,

        /// <summary>
        /// Edege
        /// </summary>
        Edege = 8
    }
}
