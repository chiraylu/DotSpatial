using DotSpatial.Controls;
using DotSpatial.Data;
using DotSpatial.Plugins.WebMap.Properties;
using DotSpatial.Plugins.WebMap.Tiling;
using DotSpatial.Projections;
using DotSpatial.Serialization;
using DotSpatial.Symbology;
using GeoAPI.Geometries;
using System;
using System.ComponentModel;
using System.Linq;

namespace DotSpatial.Plugins.WebMap
{
    [Serializable]
    public class WebMapImageLayer : MapImageLayer
    {
        private BackgroundWorker _bw;
        private int _extendBufferCoeff = 3;
        private short _opacity = 100;
        private object _lockObj = new object();

        private int _busyCount;
        private bool BwIsBusy
        {
            get => _busyCount > 0;
            set
            {
                lock (_lockObj)
                {
                    if (value)
                    {
                        _busyCount++;
                    }
                    else
                    {
                        _busyCount--;
                    }
                    if (_busyCount < 0)
                    {
                        _busyCount = 0;
                    }
                }
            }
        }
        private string _webMapName;
        /// <summary>
        /// 在线地图名称，若设置为配置的已知地图，则自动设置TileManager，反之需自行设置TileManager
        /// </summary>
        [Serialize("WebMapName")]
        public string WebMapName
        {
            get { return _webMapName; }
            set { _webMapName = value; OnWebMapNameChanged(); }
        }

        private TileManager _tileManager;

        public TileManager TileManager
        {
            get { return _tileManager; }
            set { _tileManager = value; OnTileManagerChanged(); }
        }

        [Serialize("WebMapExtent")]
        public Extent WebMapExtent { get; set; }

        public override Extent Extent => WebMapExtent;

        public override IFrame MapFrame
        {
            get => base.MapFrame;
            set
            {
                OnMapFrameChanging(base.MapFrame, value);
                base.MapFrame = value;
                RunOrCancelBw();
            }
        }

        private IMapFrame RealMapFrame => MapFrame as IMapFrame;

        private Map Map => RealMapFrame?.Parent as Map;

        public WebMapImageLayer()
        {
            LegendTextReadOnly = true;
            var xmin = TileCalculator.MinWebMercX;
            var ymin = TileCalculator.MinWebMercY;
            var xmax = TileCalculator.MaxWebMercX;
            var ymax = TileCalculator.MaxWebMercY;
            WebMapExtent = new Extent(xmin, ymin, xmax, ymax);
            // Setup the background worker
            _bw = new BackgroundWorker
            {
                WorkerSupportsCancellation = true,
                WorkerReportsProgress = true
            };
            _bw.DoWork += BwDoWork;
            _bw.RunWorkerCompleted += BwRunWorkerCompleted;
            _bw.ProgressChanged += BwProgressChanged;
            RemoveItem += WebMapImageLayer_RemoveItem;
        }
        private void OnWebMapNameChanged()
        {
            var providers = ServiceProviderFactory.GetDefaultServiceProviders();
            var provider = providers.FirstOrDefault(p => p.Name.Equals(WebMapName, StringComparison.InvariantCultureIgnoreCase));
            if (provider == null)
            {
                TileManager = null;
            }
            else
            {
                TileManager = new TileManager(provider);
            }
        }

        private void OnTileManagerChanged()
        {
            RunOrCancelBw();
        }

        private void WebMapImageLayer_RemoveItem(object sender, EventArgs e)
        {
            MapFrame.ViewExtentsChanged -= MapFrameExtentsChanged;
        }

        private void OnMapFrameChanging(IFrame oldFrame, IFrame newFrame)
        {
            if (oldFrame != null)
            {
                oldFrame.ViewExtentsChanged -= MapFrameExtentsChanged;
            }
            if (newFrame != null)
            {
                newFrame.ViewExtentsChanged += MapFrameExtentsChanged;
            }
        }
        private void MapFrameExtentsChanged(object sender, ExtentArgs e)
        {
            RunOrCancelBw();
        }

        private void RunOrCancelBw()
        {
            if (Map != null && !BwIsBusy)
            {
                Map.IsBusy = true;
            }
            BwIsBusy = true;
            if (!_bw.IsBusy)
                _bw.RunWorkerAsync();
            else
                _bw.CancelAsync();
        }

        private void BwDoWork(object sender, DoWorkEventArgs e)
        {
            if (sender is BackgroundWorker worker)
            {
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                }
                else
                {
                    worker.ReportProgress(10);
                    UpdateStichedBasemap(e);
                }
            }
        }

        /// <summary>
        /// Main method of this plugin: gets the tiles from the TileManager, stitches them together, and adds the layer to the map.
        /// </summary>
        /// <param name="e">The event args.</param>
        private void UpdateStichedBasemap(DoWorkEventArgs e)
        {
            var bwProgress = (Func<int, bool>)(p =>
            {
                _bw.ReportProgress(p);
                if (_bw.CancellationPending)
                {
                    e.Cancel = true;
                    return false;
                }

                return true;
            });
            if (Map != null&&TileManager != null)
            {

                var rectangle = Map.Bounds;
                var webMercExtent = Map.ViewExtents.Clone() as Extent;

                if (webMercExtent == null) return;

                // If ExtendBuffer, correct the displayed extent
                if (Map.ExtendBuffer)
                    webMercExtent.ExpandBy(-webMercExtent.Width / _extendBufferCoeff, -webMercExtent.Height / _extendBufferCoeff);

                // Clip the reported Web Merc Envelope to be within possible Web Merc extents
                //  This fixes an issue with Reproject returning bad results for very large (impossible) web merc extents reported from the Map
                var webMercTopLeftX = TileCalculator.Clip(webMercExtent.MinX, TileCalculator.MinWebMercX, TileCalculator.MaxWebMercX);
                var webMercTopLeftY = TileCalculator.Clip(webMercExtent.MaxY, TileCalculator.MinWebMercY, TileCalculator.MaxWebMercY);
                var webMercBtmRightX = TileCalculator.Clip(webMercExtent.MaxX, TileCalculator.MinWebMercX, TileCalculator.MaxWebMercX);
                var webMercBtmRightY = TileCalculator.Clip(webMercExtent.MinY, TileCalculator.MinWebMercY, TileCalculator.MaxWebMercY);

                if (!bwProgress(25)) return;

                // Get the web mercator vertices of the current map view
                var mapVertices = new[] { webMercTopLeftX, webMercTopLeftY, webMercBtmRightX, webMercBtmRightY };
                double[] z = { 0, 0 };

                // Reproject from web mercator to WGS1984 geographic
                Projections.Reproject.ReprojectPoints(mapVertices, z, ServiceProviderFactory.WebMercProj.Value, ServiceProviderFactory.Wgs84Proj.Value, 0, mapVertices.Length / 2);
                var geogEnv = new Envelope(mapVertices[0], mapVertices[2], mapVertices[1], mapVertices[3]);

                if (!bwProgress(40)) return;

                // Grab the tiles
                var tiles = TileManager.GetTiles(geogEnv, rectangle, _bw);
                if (!bwProgress(50)) return;

                // Stitch them into a single image
                var stitchedBasemap = TileCalculator.StitchTiles(tiles.Bitmaps, _opacity);
                var tileImage = new InRamImageData(stitchedBasemap)
                {
                    Name = WebMapName,
                    Projection = Projection
                };

                // report progress and check for cancel
                if (!bwProgress(70)) return;

                // Tiles will have often slightly different bounds from what we are displaying on screen
                // so we need to get the top left and bottom right tiles' bounds to get the proper extent
                // of the tiled image
                var tileVertices = new[] { tiles.TopLeftTile.MinX, tiles.TopLeftTile.MaxY, tiles.BottomRightTile.MaxX, tiles.BottomRightTile.MinY };

                // Reproject from WGS1984 geographic coordinates to web mercator so we can show on the map
                Projections.Reproject.ReprojectPoints(tileVertices, z, ServiceProviderFactory.Wgs84Proj.Value, ServiceProviderFactory.WebMercProj.Value, 0, tileVertices.Length / 2);

                tileImage.Bounds = new RasterBounds(stitchedBasemap.Height, stitchedBasemap.Width, new Extent(tileVertices[0], tileVertices[3], tileVertices[2], tileVertices[1]));

                // report progress and check for cancel
                if (!bwProgress(90)) return;

                Image = tileImage;
            }
            // report progress and check for cancel
            bwProgress(99);
        }
        private void BwProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // Do we know what what our progress completion percent is (instead of 50)?
            ProgressHandler.Progress("Loading Basemap ...", e.ProgressPercentage, "Loading Basemap ...");
        }

        private void BwRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (sender is BackgroundWorker bw)
            {
                BwIsBusy = false;
                if (BwIsBusy)
                {
                    bw.RunWorkerAsync();
                    return;
                }

                if (Map != null)
                {
                    Map.IsBusy = false;
                    MapFrame.Invalidate();
                }
                ProgressHandler.Progress(string.Empty, 0, string.Empty);
            }
        }
        
    }
}
