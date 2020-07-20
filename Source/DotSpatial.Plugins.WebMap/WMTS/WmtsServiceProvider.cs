using BruTile;
using BruTile.Cache;
using BruTile.Web;
using BruTile.Wmts;
using DotSpatial.Projections;
using GeoAPI.Geometries;
using Microsoft.VisualBasic;
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
    /// Wmts service provider.
    /// </summary>
    public class WmtsServiceProvider : BrutileServiceProvider
    {
        #region Fields

        private static readonly ProjectionInfo Wgs84Proj = ProjectionInfo.FromEsriString(KnownCoordinateSystems.Geographic.World.WGS1984.ToEsriString());

        #endregion
        private string _capabilitiesUrl;

        public string CapabilitiesUrl
        {
            get { return _capabilitiesUrl; }
            set
            {
                _capabilitiesUrl = value;
                OnNameOrCapabilitiesUrlChanged();
            }
        }
        public override string Name
        {
            get => base.Name;
            set
            {
                base.Name = value;
                OnNameOrCapabilitiesUrlChanged();
            }
        }
        #region  Constructors

        public WmtsServiceProvider(string name, string capabilitiesUrl, ITileCache<byte[]> tileCache)
               : base(name, null, tileCache)
        {
            CapabilitiesUrl = capabilitiesUrl;
            Configure = () =>
            {
                var dialogDefault = string.IsNullOrEmpty(capabilitiesUrl) ? "http://someservice/WMTS/1.0.0/WMTSCapabilities.xml" : capabilitiesUrl;
                var guiUrl = Interaction.InputBox("Please provide the Url for the service.", DefaultResponse: dialogDefault);
                if (!string.IsNullOrWhiteSpace(guiUrl))
                {
                    var capabilities = WmtsExtensions.GetCapabilities(guiUrl);
                    if (capabilities != null)
                    {
                        var layerType = capabilities.Contents.Layers?.FirstOrDefault();
                        if (layerType != null)
                        {
                            Name = layerType.Identifier.Value;
                            CapabilitiesUrl = guiUrl;
                            return true;
                        }
                    }
                }
                return false;
            };
        }
        private void OnNameOrCapabilitiesUrlChanged()
        {
            HttpTileSource tileSource = null;
            if (string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(CapabilitiesUrl))
            {
                return;
            }
            try
            {
                var myRequest = WebRequest.Create(CapabilitiesUrl);
                using (var myResponse = myRequest.GetResponse())
                using (var stream = myResponse.GetResponseStream())
                {
                    var tileSources = WmtsParser.Parse(stream);
                    tileSource = tileSources.FirstOrDefault(s => s.Name == Name);
                    Projection = tileSource?.Schema.Srs.ToProjectionInfo();
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
            }
            TileSource = tileSource;
        }
        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether configuration is needed.
        /// </summary>
        public override bool NeedConfigure => TileSource == null;

        #endregion

        #region Methods

        /// <inheritdoc />
        public override Bitmap GetBitmap(int x, int y, Envelope envelope, int zoom)
        {
            var ts = TileSource;
            if (ts == null) return null;

            var zoomS = zoom.ToString(CultureInfo.InvariantCulture);
            try
            {
                var index = new TileIndex(x, y, zoom);
                var tc = TileCache;
                var bytes = tc?.Find(index);
                if (bytes == null)
                {
                    var mapVertices = new[] { envelope.MinX, envelope.MaxY, envelope.MaxX, envelope.MinY };
                    double[] viewExtentZ = { 0.0, 0.0 };
                    Reproject.ReprojectPoints(mapVertices, viewExtentZ, Wgs84Proj, Projection, 0, mapVertices.Length / 2);
                    var geogEnv = new Envelope(mapVertices[0], mapVertices[2], mapVertices[1], mapVertices[3]);
                    bytes = ts.GetTile(new TileInfo
                    {
                        Extent = ToBrutileExtent(geogEnv),
                        Index = index
                    });
                    var bm = new Bitmap(new MemoryStream(bytes));
                    tc?.Add(index, bytes);

                    return bm;
                }

                return new Bitmap(new MemoryStream(bytes));
            }
            catch (Exception ex)
            {
                if (ex is WebException || ex is TimeoutException)
                {
                    return ExceptionToBitmap(ex, TileSource.Schema.GetTileWidth(zoom), TileSource.Schema.GetTileHeight(zoom));
                }

                Debug.WriteLine(ex.Message);
            }

            return null;
        }

        #endregion
    }
}
