using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotSpatial.Data.Rasters.GdalExtension
{
    /// <summary>
    /// Gdal extensions
    /// </summary>
    public static class GdalExtensions
    {
        /// <summary>
        /// brief Compose two geotransforms.
        /// </summary>
        /// <param name="padfGT1">geoTransform1</param>
        /// <param name="padfGT2">geoTransform2</param>
        /// <returns>geoTransform</returns>
        public static double[] GDALComposeGeoTransforms(double[] padfGT1, double[] padfGT2)
        {
            double[] gtwrk = new double[6];
            // We need to think of the geotransform in a more normal form to do
            // the matrix multiple:
            //
            //  __                     __
            //  | gt[1]   gt[2]   gt[0] |
            //  | gt[4]   gt[5]   gt[3] |
            //  |  0.0     0.0     1.0  |
            //  --                     --
            //
            // Then we can use normal matrix multiplication to produce the
            // composed transformation.  I don't actually reform the matrix
            // explicitly which is why the following may seem kind of spagettish.

            gtwrk[1] =
            padfGT2[1] * padfGT1[1]
            + padfGT2[2] * padfGT1[4];
            gtwrk[2] =
                padfGT2[1] * padfGT1[2]
                + padfGT2[2] * padfGT1[5];
            gtwrk[0] =
                padfGT2[1] * padfGT1[0]
                + padfGT2[2] * padfGT1[3]
                + padfGT2[0] * 1.0;

            gtwrk[4] =
                padfGT2[4] * padfGT1[1]
                + padfGT2[5] * padfGT1[4];
            gtwrk[5] =
                padfGT2[4] * padfGT1[2]
                + padfGT2[5] * padfGT1[5];
            gtwrk[3] =
                padfGT2[4] * padfGT1[0]
                + padfGT2[5] * padfGT1[3]
                + padfGT2[3] * 1.0;
            return gtwrk;
        }
    }
}
