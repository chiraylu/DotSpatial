using DotSpatial.Data;
using DotSpatial.Projections;
using GeoAPI.Geometries;
using System.Collections.Generic;
using System.Linq;

namespace DotSpatial.Plugins.WebMap
{
    public static class ProjectionExtentions
    {
        public static Extent Reproject(this Extent extent, ProjectionInfo srcProjection, ProjectionInfo destProjection)
        {
            Extent destExtent = extent.Copy();
            if (extent == null || srcProjection == null || destProjection == null || srcProjection.Equals(destProjection))
            {
                return destExtent;
            }
            var xmin = extent.MinX;
            var xmax = extent.MaxX;
            var ymin = extent.MinY;
            var ymax = extent.MaxY;
            double[] z = { 0, 0 };
            var mapVertices = new[] { xmin, ymax, xmax, ymin };
            Projections.Reproject.ReprojectPoints(mapVertices, z, srcProjection, destProjection, 0, mapVertices.Length / 2);
            destExtent.MinX = mapVertices[0];
            destExtent.MinY = mapVertices[3];
            destExtent.MaxX = mapVertices[2];
            destExtent.MaxY = mapVertices[1];
            return destExtent;
        }
        public static List<Coordinate> Reproject(this IEnumerable<Coordinate> coords, ProjectionInfo srcProjection, ProjectionInfo destProjection)
        {
            List<Coordinate> destCoords = null;
            if (coords == null)
            {
                return destCoords;
            }
            destCoords = new List<Coordinate>();
            if (srcProjection == null || destProjection == null || srcProjection.Equals(destProjection))
            {
                destCoords.AddRange(coords);
            }
            else
            {
                double[] z = { 0, 0 };
                var coordCount = coords.Count();
                double[] mapVertices = new double[2 * coordCount];
                for (int i = 0; i < coordCount; i++)
                {
                    var coord = coords.ElementAt(i);
                    mapVertices[i * 2] = coord.X;
                    mapVertices[i * 2 + 1] = coord.Y;
                }
                Projections.Reproject.ReprojectPoints(mapVertices, z, srcProjection, destProjection, 0, mapVertices.Length / 2);
                for (int i = 0; i < coordCount; i++)
                {
                    var coord = new Coordinate(mapVertices[i * 2], mapVertices[i * 2 + 1]);
                    destCoords.Add(coord);
                }
            }
            return destCoords;
        }
        public static Coordinate Reproject(this Coordinate coord, ProjectionInfo srcProjection, ProjectionInfo destProjection)
        {
            Coordinate destCoord = null;
            if (coord == null)
            {
                return destCoord;
            }
            if (Reproject(coord.X, coord.Y, srcProjection, destProjection, out double destX, out double destY))
            {
                destCoord = new Coordinate(destX, destY);
            }
            return destCoord;
        }
        public static bool Reproject(double srcX, double srcY, ProjectionInfo srcProjection, ProjectionInfo destProjection, out double destX, out double destY)
        {
            bool ret = false;
            destX = srcX;
            destY = srcY;
            if (srcProjection == null || destProjection == null || srcProjection.Equals(destProjection))
            {
                return ret;
            }
            double[] z = { 0, 0 };
            var mapVertices = new[] { srcX, srcY };
            Projections.Reproject.ReprojectPoints(mapVertices, z, srcProjection, destProjection, 0, mapVertices.Length / 2);
            destX = mapVertices[0];
            destY = mapVertices[1];
            ret = true;
            return ret;
        }
    }
}
