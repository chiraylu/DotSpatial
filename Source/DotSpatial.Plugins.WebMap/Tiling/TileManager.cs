// Copyright (c) DotSpatial Team. All rights reserved.
// Licensed under the MIT license. See License.txt file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using DotSpatial.Plugins.WebMap.Properties;
using DotSpatial.Serialization;
using GeoAPI.Geometries;

namespace DotSpatial.Plugins.WebMap.Tiling
{
    /// <summary>
    /// The tile manager manages the way tiles are gotten.
    /// </summary>
    public class TileManager
    {
        public ServiceProvider ServiceProvider { get; }

        #region  Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TileManager"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider that gets the tiles.</param>
        public TileManager(ServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the tiles.
        /// </summary>
        /// <param name="wgs84Envelope">Envelope that indicates for which region the tiles are needed.</param>
        /// <param name="bounds">Bounds needed for zoom level calculation.</param>
        /// <param name="bw">The background worker.</param>
        /// <returns>The tiles needed for the envelope.</returns>
        public Tiles GetTiles(Envelope wgs84Envelope, Rectangle bounds, BackgroundWorker bw)
        {
            Coordinate mapTopLeft = new Coordinate(wgs84Envelope.MinX, wgs84Envelope.MaxY);
            Coordinate mapBottomRight = new Coordinate(wgs84Envelope.MaxX, wgs84Envelope.MinY);

            // Clip the coordinates so they are in the range of the web mercator projection
            mapTopLeft.Y = TileCalculator.Clip(mapTopLeft.Y, TileCalculator.MinLatitude, TileCalculator.MaxLatitude);
            mapTopLeft.X = TileCalculator.Clip(mapTopLeft.X, TileCalculator.MinLongitude, TileCalculator.MaxLongitude);

            mapBottomRight.Y = TileCalculator.Clip(mapBottomRight.Y, TileCalculator.MinLatitude, TileCalculator.MaxLatitude);
            mapBottomRight.X = TileCalculator.Clip(mapBottomRight.X, TileCalculator.MinLongitude, TileCalculator.MaxLongitude);
            int minZoom = 0, maxZoom = 18;
            if (ServiceProvider is BrutileServiceProvider brutileServiceProvider)
            {
                var levels = brutileServiceProvider.TileSource.Schema.Resolutions.Keys;
                if (levels.Count > 0)
                {
                    minZoom = levels.First();
                    maxZoom = levels.Last();
                }
            }
            var zoom = TileCalculator.DetermineZoomLevel(wgs84Envelope, bounds, minZoom, maxZoom);
            var topLeftTileXy = TileCalculator.LatLongToTileXy(mapTopLeft, zoom);
            var btmRightTileXy = TileCalculator.LatLongToTileXy(mapBottomRight, zoom);

            var tileMatrix = new Bitmap[btmRightTileXy.X - topLeftTileXy.X + 1, btmRightTileXy.Y - topLeftTileXy.Y + 1];
            var po = new ParallelOptions
            {
                MaxDegreeOfParallelism = -1
            };
            Parallel.For(topLeftTileXy.Y, btmRightTileXy.Y + 1, po, (y, loopState) => Parallel.For(topLeftTileXy.X, btmRightTileXy.X + 1, po, (x, loopState2) =>
                {
                    if (bw.CancellationPending)
                    {
                        loopState.Stop();
                        loopState2.Stop();
                        return;
                    }

                    var currEnv = GetTileEnvelope(x, y, zoom);
                    tileMatrix[x - topLeftTileXy.X, y - topLeftTileXy.Y] = GetTile(x, y, currEnv, zoom);
                }));
            return new Tiles(
                zoom,
                tileMatrix,
                GetTileEnvelope(topLeftTileXy.X, topLeftTileXy.Y, zoom), // top left tile = tileMatrix[0,0]
                GetTileEnvelope(btmRightTileXy.X, btmRightTileXy.Y, zoom)); // bottom right tile = tileMatrix[last, last]
        }

        /// <summary>
        /// Get tile envelope in WGS-84 coordinates.
        /// </summary>
        /// <param name="x">x index</param>
        /// <param name="y">y index</param>
        /// <param name="zoom">zoom</param>
        /// <returns>Envelope in WGS-84</returns>
        private static Envelope GetTileEnvelope(int x, int y, int zoom)
        {
            var currTopLeftPixXy = TileCalculator.TileXyToTopLeftPixelXy(x, y);
            var currTopLeftCoord = TileCalculator.PixelXyToLatLong((int)currTopLeftPixXy.X, (int)currTopLeftPixXy.Y, zoom);

            var currBtmRightPixXy = TileCalculator.TileXyToBottomRightPixelXy(x, y);
            var currBtmRightCoord = TileCalculator.PixelXyToLatLong((int)currBtmRightPixXy.X, (int)currBtmRightPixXy.Y, zoom);
            return new Envelope(currTopLeftCoord, currBtmRightCoord);
        }

        private Bitmap GetTile(int x, int y, Envelope envelope, int zoom)
        {
            Bitmap bm;
            try
            {
                bm = ServiceProvider.GetBitmap(x, y, envelope, zoom) ?? Resources.nodata;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                bm = Resources.nodata;
            }

            return bm;
        }

        #endregion
    }
}