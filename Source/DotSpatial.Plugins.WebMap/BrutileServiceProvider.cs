﻿// Copyright (c) DotSpatial Team. All rights reserved.
// Licensed under the MIT license. See License.txt file in the project root for full license information.

using BruTile;
using BruTile.Cache;
using BruTile.Web;
using GeoAPI.Geometries;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;

namespace DotSpatial.Plugins.WebMap
{
    /// <summary>
    /// Brutile service provider.
    /// </summary>
    public class BrutileServiceProvider : ServiceProvider
    {
        #region  Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BrutileServiceProvider"/> class.
        /// </summary>
        /// <param name="name">The name of the service provider.</param>
        /// <param name="tileSource">The tile cache.</param>
        /// <param name="tileCache">The tile source.</param>
        public BrutileServiceProvider(string name, ITileSource tileSource, ITileCache<byte[]> tileCache)
            : base(name)
        {
            TileCache = tileCache;
            TileSource = tileSource;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the tile cache.
        /// </summary>
        public ITileCache<byte[]> TileCache { get; set; }

        /// <summary>
        /// Gets or sets the tile source.
        /// </summary>
        public ITileSource TileSource { get; set; }


        #endregion

        #region Methods

        /// <summary>
        /// Gets a bitmap.
        /// </summary>
        /// <param name="x">The tile number in x direction.</param>
        /// <param name="y">The tile number in y direction.</param>
        /// <param name="envelope">The envelope for which to get the tiles.</param>
        /// <param name="zoom">The zoom level for which to get the tile.</param>
        /// <returns>Null on error, otherwise the resulting bitmap.</returns>
        public override Bitmap GetBitmap(int x, int y, Envelope envelope, int zoom)
        {
            Bitmap bitMap = null;
            var ts = TileSource;
            if (ts == null) return bitMap;

            var extent = ToBrutileExtent(envelope);
            TileInfo tileInfo;
            try
            {
                tileInfo = ts.Schema.GetTileInfos(extent, zoom).FirstOrDefault();
            }
            catch(Exception e)
            {
                Debug.WriteLine($"未找到瓦片信息：{e}");
                return bitMap;
            }
            try
            {
                var index = new TileIndex(x, y, zoom);
                var tc = TileCache;
                var bytes = tc?.Find(index);
                if (bytes == null)
                {
                    if (tileInfo == null)
                    {
                        return bitMap;
                    }

                    tileInfo.Index = index;
                    bytes = ts.GetTile(tileInfo);
                    bitMap = new Bitmap(new MemoryStream(bytes));
                    tc?.Add(index, bytes);

                    return bitMap;
                }

                return new Bitmap(new MemoryStream(bytes));
            }
            catch (Exception ex)
            {
                if (ex is WebException || ex is TimeoutException)
                {
                    bitMap = ExceptionToBitmap(ex, TileSource.Schema.GetTileWidth(zoom), TileSource.Schema.GetTileHeight(zoom));
                }
                else
                {
                    Debug.WriteLine(ex);
                }
            }

            // Esri Hyro Base Map Fix, the server doesn't put image in the response header.
            var ts1 = ts as ArcGisTileSource;
            if (ts1 != null)
            {
                try
                {
                    string str = ts1.BaseUrl + "/tile/{zoom}/{y}/{x}";
                    if (!str.Contains("{key}"))
                    {
                        str = str.Replace("{zoom}", zoom.ToString());
                        str = str.Replace("{x}", x.ToString());
                        str = str.Replace("{y}", y.ToString());

                        Stream stream = new WebClient().OpenRead(str);
                        if (stream != null)
                        {
                            bitMap = new Bitmap(stream);
                        }
                    }
                }
                catch (Exception)
                {
                }
            }

            return bitMap;
        }

        /// <summary>
        /// Converts the envelope to a BruTile extent.
        /// </summary>
        /// <param name="extent">Envelope to convert.</param>
        /// <returns>The resulting BruTile extent.</returns>
        protected static Extent ToBrutileExtent(Envelope extent)
        {
            return new Extent(extent.MinX, extent.MinY, extent.MaxX, extent.MaxY);
        }

        #endregion
    }
}