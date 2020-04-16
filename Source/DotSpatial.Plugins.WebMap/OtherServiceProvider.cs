// Copyright (c) DotSpatial Team. All rights reserved.
// Licensed under the MIT license. See License.txt file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Net;
using BruTile;
using BruTile.Cache;
using DotSpatial.Plugins.WebMap.Tiling;
using GeoAPI.Geometries;
using Microsoft.VisualBasic;

namespace DotSpatial.Plugins.WebMap
{
    /// <summary>
    /// This can be used to work with services other than the ones already defined.
    /// </summary>
    [Serializable]
    public class OtherServiceProvider : ServiceProvider
    {
        #region Fields

        private string _url;

        #endregion

        #region  Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="OtherServiceProvider"/> class.
        /// </summary>
        /// <param name="name">Name of the service provider.</param>
        /// <param name="url">Url of the service provider.</param>
        public OtherServiceProvider(string name, string url)
            : base(name)
        {
            _url = url;
            Configure = () =>
                {
                    var dialogDefault = string.IsNullOrWhiteSpace(_url) ? "http://tiles.virtualearth.net/tiles/h{key}.jpeg?g=461&mkt=en-us&n=z" : _url;
                    var guiUrl = Interaction.InputBox("Please provide the Url for the service.", DefaultResponse: dialogDefault);
                    if (!string.IsNullOrWhiteSpace(guiUrl))
                    {
                        _url = guiUrl;
                        return true;
                    }

                    return false;
                };
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the tile cache.
        /// </summary>
        public ITileCache<byte[]> TileCache { get; set; }

        /// <inheritdoc />
        public override bool NeedConfigure => string.IsNullOrWhiteSpace(_url);

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override Bitmap GetBitmap(int x, int y, Envelope envelope, int zoom)
        {
            Bitmap bitMap = null;
            try
            {
                var zoomS = zoom.ToString(CultureInfo.InvariantCulture);
                var index = new TileIndex(x, y, zoomS);
                var bytes = TileCache?.Find(index);
                if (bytes == null)
                {
                    var url = _url;
                    if (url != null)
                    {
                        if (url.Contains("{key}"))
                        {
                            var quadKey = TileCalculator.TileXyToBingQuadKey(x, y, zoom);
                            url = url.Replace("{key}", quadKey);
                        }
                        else
                        {
                            url = url.Replace("{zoom}", zoom.ToString(CultureInfo.InvariantCulture));
                            url = url.Replace("{x}", x.ToString(CultureInfo.InvariantCulture));
                            url = url.Replace("{y}", y.ToString(CultureInfo.InvariantCulture));
                        }

                        using (var client = new WebClient())
                        {
                            var stream = client.OpenRead(url);
                            if (stream != null)
                            {
                                int count = 1024;
                                int readCount = count;
                                byte[] buffer = new byte[count];
                                var ms = new MemoryStream();
                                while ((readCount = stream.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    ms.Write(buffer, 0, readCount);
                                }
                                bytes = ms.GetBuffer();
                                ms.Seek(0, SeekOrigin.Begin);
                                bitMap = new Bitmap(ms);
                                TileCache?.Add(index, bytes);
                                stream.Flush();
                                stream.Close();
                            }
                        }
                    }
                }
                else
                {
                    bitMap = new Bitmap(new MemoryStream(bytes));
                }
            }
            catch (Exception ex)
            {
                if (ex is WebException || ex is TimeoutException)
                {
                    return ExceptionToBitmap(ex, 256, 256);
                }

                Debug.WriteLine(ex.Message);
            }

            return bitMap;
        }

        #endregion
    }
}