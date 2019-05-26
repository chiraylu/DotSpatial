// Copyright (c) DotSpatial Team. All rights reserved.
// Licensed under the MIT license. See License.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using OSGeo.GDAL;

namespace DotSpatial.Data.Rasters.GdalExtension
{
    /// <summary>
    /// Helper methods for GDAL.
    /// </summary>
    internal static class Helpers
    {
        #region Methods

        /// <summary>
        /// Opens the given file.
        /// </summary>
        /// <param name="fileName">File that gets opened.</param>
        /// <returns>Opened file as data set.</returns>
        public static Dataset Open(string fileName)
        {
            try
            {
                return Gdal.Open(fileName, Access.GA_Update);
            }
            catch
            {
                try
                {
                    return Gdal.Open(fileName, Access.GA_ReadOnly);
                }
                catch (Exception ex)
                {
                    throw new GdalException(ex.ToString());
                }
            }
        }

        public static int CreateOverview(Dataset _dataset, string resampling = "NEAREST", int[] overviewlist = null)
        {
            int value = -1;
            if (_dataset == null || _dataset.RasterCount <= 0)
            {
                return value;
            }

            if (overviewlist == null)
            {
                List<int> intList = new List<int>();
                int width = _dataset.RasterXSize;
                int height = _dataset.RasterYSize;
                int k = 1;
                while (width > 256 && height > 256)
                {
                    k *= 2;
                    intList.Add(k);
                    width /= 2;
                    height /= 2;
                }

                overviewlist = intList.ToArray();
            }

            value = _dataset.BuildOverviews(resampling, overviewlist);
            return value;
        }
        #endregion
    }
}