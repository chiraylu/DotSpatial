using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using DotSpatial.Controls;
using DotSpatial.Data;
using DotSpatial.Plugins.WebMap.Tiling;
using DotSpatial.Projections;
using DotSpatial.Serialization;
using DotSpatial.Symbology;
using GeoAPI.Geometries;

namespace DotSpatial.Plugins.WebMap
{
    /// <summary>
    /// 在线地图图层类
    /// </summary>
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

        private Map Map => (MapFrame as IMapFrame)?.Parent as Map;

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
        protected override void OnDataSetChanged(IImageData value)
        {
            base.OnDataSetChanged(value);
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
            if (IsVisible)
            {
                RunOrCancelBw();
            }
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
        private Rectangle GetRectangle(Extent extent, Rectangle rectangle, double xmin, double ymin, double xmax, double ymax)
        {
            Rectangle destRect = Rectangle.Empty;
            if (extent == null || extent.Width == 0 || extent.Height == 0)
            {
                return destRect;
            }
            double dx = rectangle.Width / extent.Width;
            double dy = rectangle.Height / extent.Height;
            destRect.X = (int)Math.Floor(rectangle.X + (xmin - extent.MinX) * dx);
            destRect.X = (int)Math.Floor(rectangle.Y + (extent.MaxY - ymax) * dy);
            destRect.Width = (int)Math.Ceiling((xmax - xmin) * dx);
            destRect.Height = (int)Math.Ceiling((ymax - ymin) * dx);
            return destRect;
        }
        private Rectangle GetRectangle(Extent extent, Rectangle rectangle, Extent newExtent)
        {
            return GetRectangle(extent, rectangle, newExtent.MinX, newExtent.MinY, newExtent.MaxX, newExtent.MaxY);
        }
        private Bitmap GetNewBitmap(Bitmap bmp, int width, int height, RectangleF destRectangle)
        {
            Bitmap destBmp = null;
            if (bmp != null && width > 0 && height > 0)
            {
                destBmp = new Bitmap(width, height);
                using (Graphics g = Graphics.FromImage(destBmp))
                {
                    g.Clear(Color.Transparent);
                    GraphicsUnit unit = GraphicsUnit.Pixel;
                    var srcRect = bmp.GetBounds(ref unit);
                    g.DrawImage(bmp, destRectangle, srcRect, unit);
                }
            }
            return destBmp;
        }
        private Bitmap GetNewBitmap(Bitmap bmp, int width, int height)
        {
            Bitmap destBmp = null;
            if (bmp != null && width > 0 && height > 0)
            {
                destBmp = new Bitmap(width, height);
                using (Graphics g = Graphics.FromImage(destBmp))
                {
                    g.Clear(Color.Transparent);
                    GraphicsUnit unit = GraphicsUnit.Pixel;
                    var srcRect = bmp.GetBounds(ref unit);
                    g.DrawImage(bmp, new RectangleF(0, 0, width, height), srcRect, unit);
                }
            }
            return destBmp;
        }
        private InRamImageData GetTilesFromMapView0(Func<int, bool> bwProgress)
        {
            InRamImageData tileImage = null;
            if (Map != null)
            {
                Extent mapViewExtent = Map.ViewExtents?.Clone() as Extent;
                Rectangle rectangle = Map.Bounds;
                if (Map.Projection != null)
                {
                    if (mapViewExtent == null)
                    {
                        return tileImage;
                    }
                    // If ExtendBuffer, correct the displayed extent
                    if (Map.ExtendBuffer)
                    {
                        mapViewExtent.ExpandBy(-mapViewExtent.Width / _extendBufferCoeff, -mapViewExtent.Height / _extendBufferCoeff);
                    }
                    var xmin = mapViewExtent.MinX;
                    var xmax = mapViewExtent.MaxX;
                    var ymin = mapViewExtent.MinY;
                    var ymax = mapViewExtent.MaxY;
                    Tiles tiles = null;
                    double[] z = { 0, 0 };
                    if (Map.Projection.Equals(ServiceProviderFactory.WebMercProj.Value))
                    {
                        xmin = TileCalculator.Clip(xmin, TileCalculator.MinWebMercX, TileCalculator.MaxWebMercX);
                        xmax = TileCalculator.Clip(xmax, TileCalculator.MinWebMercX, TileCalculator.MaxWebMercX);
                        ymin = TileCalculator.Clip(ymin, TileCalculator.MinWebMercY, TileCalculator.MaxWebMercY);
                        ymax = TileCalculator.Clip(ymax, TileCalculator.MinWebMercY, TileCalculator.MaxWebMercY);
                        rectangle = GetRectangle(mapViewExtent, rectangle, xmin, ymin, xmax, ymax);
                        if (!bwProgress(25)) return tileImage;

                        // Get the web mercator vertices of the current map view
                        var mapVertices = new[] { xmin, ymax, xmax, ymin };

                        // Reproject from web mercator to WGS1984 geographic
                        Projections.Reproject.ReprojectPoints(mapVertices, z, ServiceProviderFactory.WebMercProj.Value, ServiceProviderFactory.Wgs84Proj.Value, 0, mapVertices.Length / 2);
                        var geogEnv = new Envelope(mapVertices[0], mapVertices[2], mapVertices[1], mapVertices[3]);

                        if (!bwProgress(40)) return tileImage;

                        // Grab the tiles
                        tiles = TileManager.GetTiles(geogEnv, rectangle, _bw);
                        if (!bwProgress(50)) return tileImage;
                    }
                    else if (Map.Projection.Equals(ServiceProviderFactory.Wgs84Proj.Value))
                    {
                        xmin = TileCalculator.Clip(xmin, TileCalculator.MinLongitude, TileCalculator.MaxLongitude);
                        xmax = TileCalculator.Clip(xmax, TileCalculator.MinLongitude, TileCalculator.MaxLongitude);
                        ymin = TileCalculator.Clip(ymin, TileCalculator.MinLatitude, TileCalculator.MaxLatitude);
                        ymax = TileCalculator.Clip(ymax, TileCalculator.MinLatitude, TileCalculator.MaxLatitude);
                        rectangle = GetRectangle(mapViewExtent, rectangle, xmin, ymin, xmax, ymax);
                        if (!bwProgress(25)) return tileImage;

                        var geogEnv = new Envelope(xmin, xmax, ymin, ymax);

                        if (!bwProgress(40)) return tileImage;

                        // Grab the tiles
                        tiles = TileManager.GetTiles(geogEnv, rectangle, _bw);
                        if (!bwProgress(50)) return tileImage;
                    }
                    // Stitch them into a single image
                    var stitchedBasemap = TileCalculator.StitchTiles(tiles.Bitmaps, _opacity);
                    var bmpExtent = new Extent(tiles.TopLeftTile.MinX, tiles.BottomRightTile.MinY, tiles.BottomRightTile.MaxX, tiles.TopLeftTile.MaxY);
                    if (Map.Projection.Equals(ServiceProviderFactory.Wgs84Proj.Value) && TileManager.ServiceProvider.Projection.Equals(ServiceProviderFactory.WebMercProj.Value))
                    {
                        double dx0 = mapViewExtent.Width / Map.Bounds.Width;
                        double dy0 = mapViewExtent.Height / Map.Bounds.Height;

                        var destWidth = bmpExtent.Width / dx0;
                        var destHeight = bmpExtent.Height / dy0;
                        double srcRatio = (double)stitchedBasemap.Width / stitchedBasemap.Height;
                        double destRatio = destWidth / destHeight;
                        int widthOfPixel = stitchedBasemap.Width;
                        int heightOfPixel = stitchedBasemap.Height;
                        if (srcRatio > destRatio)
                        {
                            widthOfPixel = (int)Math.Ceiling(heightOfPixel * destRatio);
                        }
                        else
                        {
                            heightOfPixel = (int)Math.Ceiling(widthOfPixel / destRatio);
                        }
                        var destExtent = new Extent();
                        destExtent.SetCenter(bmpExtent.Center, widthOfPixel * dx0, heightOfPixel * dy0);
                        double dx = destExtent.Width / widthOfPixel;
                        double dy = destExtent.Height / heightOfPixel;
                        RectangleF destRectangleF = new RectangleF()
                        {
                            X = Convert.ToSingle((bmpExtent.MinX - destExtent.MinX) / dx),
                            Y = Convert.ToSingle((destExtent.MaxY - bmpExtent.MaxY) / dy),
                            Width = Convert.ToSingle(bmpExtent.Width / dx),
                            Height = Convert.ToSingle(bmpExtent.Height / dy)
                        };
                        stitchedBasemap = GetNewBitmap(stitchedBasemap, widthOfPixel, heightOfPixel);
                        //stitchedBasemap = GetNewBitmap(stitchedBasemap, widthOfPixel, heightOfPixel, destRectangleF);
                        //bmpExtent = destExtent;
                    }
                    else if (Map.Projection.Equals(ServiceProviderFactory.WebMercProj.Value))
                    {
                        if (TileManager.ServiceProvider.Projection.Equals(ServiceProviderFactory.WebMercProj.Value))
                        {
                            var tileVertices = new[] { tiles.TopLeftTile.MinX, tiles.TopLeftTile.MaxY, tiles.BottomRightTile.MaxX, tiles.BottomRightTile.MinY };
                            Projections.Reproject.ReprojectPoints(tileVertices, z, ServiceProviderFactory.Wgs84Proj.Value, ServiceProviderFactory.WebMercProj.Value, 0, tileVertices.Length / 2);
                            bmpExtent = new Extent(tileVertices[0], tileVertices[3], tileVertices[2], tileVertices[1]);
                        }
                    }

                    tileImage = new InRamImageData(stitchedBasemap)
                    {
                        Name = WebMapName,
                        Projection = Map.Projection,
                        Bounds = new RasterBounds(stitchedBasemap.Height, stitchedBasemap.Width, bmpExtent)
                    };

                    if (!bwProgress(90)) return tileImage;
                }
            }
            return tileImage;
        }
        private InRamImageData GetTilesFromMapView(Func<int, bool> bwProgress)
        {
            InRamImageData tileImage = null;
            if (Map != null)
            {
                Extent mapViewExtent = Map.ViewExtents?.Clone() as Extent;
                Rectangle rectangle = Map.Bounds;
                if (Map.Projection != null)
                {
                    if (mapViewExtent == null)
                    {
                        return tileImage;
                    }
                    // If ExtendBuffer, correct the displayed extent
                    if (Map.ExtendBuffer)
                    {
                        mapViewExtent.ExpandBy(-mapViewExtent.Width / _extendBufferCoeff, -mapViewExtent.Height / _extendBufferCoeff);
                    }
                    var xmin = mapViewExtent.MinX;
                    var xmax = mapViewExtent.MaxX;
                    var ymin = mapViewExtent.MinY;
                    var ymax = mapViewExtent.MaxY;
                    double[] z = { 0, 0 };
                    Envelope geogEnv = new Envelope(xmin, xmax, ymin, ymax);
                    if (Map.Projection.Equals(ServiceProviderFactory.WebMercProj.Value))
                    {
                        xmin = TileCalculator.Clip(xmin, TileCalculator.MinWebMercX, TileCalculator.MaxWebMercX);
                        xmax = TileCalculator.Clip(xmax, TileCalculator.MinWebMercX, TileCalculator.MaxWebMercX);
                        ymin = TileCalculator.Clip(ymin, TileCalculator.MinWebMercY, TileCalculator.MaxWebMercY);
                        ymax = TileCalculator.Clip(ymax, TileCalculator.MinWebMercY, TileCalculator.MaxWebMercY);
                        rectangle = GetRectangle(mapViewExtent, rectangle, xmin, ymin, xmax, ymax);
                        if (!bwProgress(25)) return tileImage;

                        // Get the web mercator vertices of the current map view
                        var mapVertices = new[] { xmin, ymax, xmax, ymin };

                        // Reproject from web mercator to WGS1984 geographic
                        Projections.Reproject.ReprojectPoints(mapVertices, z, ServiceProviderFactory.WebMercProj.Value, ServiceProviderFactory.Wgs84Proj.Value, 0, mapVertices.Length / 2);
                        geogEnv = new Envelope(mapVertices[0], mapVertices[2], mapVertices[1], mapVertices[3]);
                    }
                    else if (Map.Projection.Equals(ServiceProviderFactory.Wgs84Proj.Value))
                    {
                        xmin = TileCalculator.Clip(xmin, TileCalculator.MinLongitude, TileCalculator.MaxLongitude);
                        xmax = TileCalculator.Clip(xmax, TileCalculator.MinLongitude, TileCalculator.MaxLongitude);
                        ymin = TileCalculator.Clip(ymin, TileCalculator.MinLatitude, TileCalculator.MaxLatitude);
                        ymax = TileCalculator.Clip(ymax, TileCalculator.MinLatitude, TileCalculator.MaxLatitude);
                        rectangle = GetRectangle(mapViewExtent, rectangle, xmin, ymin, xmax, ymax);
                        if (!bwProgress(25)) return tileImage;

                        geogEnv = new Envelope(xmin, xmax, ymin, ymax);
                    }
                    if (!bwProgress(40)) return tileImage;

                    // Grab the tiles
                    Tiles tiles = TileManager.GetTiles(geogEnv, rectangle, _bw);
                    if (!bwProgress(50)) return tileImage;

                    // Stitch them into a single image
                    var stitchedBasemap = TileCalculator.StitchTiles(tiles.Bitmaps, _opacity);
                    var bmpExtent = new Extent(tiles.TopLeftTile.MinX, tiles.BottomRightTile.MinY, tiles.BottomRightTile.MaxX, tiles.TopLeftTile.MaxY);
                    int width = stitchedBasemap.Width;
                    int height = stitchedBasemap.Height;
                    if (Map.Projection.Equals(ServiceProviderFactory.WebMercProj.Value))
                    {
                        if (TileManager.ServiceProvider.Projection.Equals(ServiceProviderFactory.WebMercProj.Value))
                        {
                            var tileVertices = new[] { tiles.TopLeftTile.MinX, tiles.TopLeftTile.MaxY, tiles.BottomRightTile.MaxX, tiles.BottomRightTile.MinY };
                            Projections.Reproject.ReprojectPoints(tileVertices, z, ServiceProviderFactory.Wgs84Proj.Value, ServiceProviderFactory.WebMercProj.Value, 0, tileVertices.Length / 2);
                            bmpExtent = new Extent(tileVertices[0], tileVertices[3], tileVertices[2], tileVertices[1]);
                        }
                    }
                    else if (Map.Projection.Equals(ServiceProviderFactory.Wgs84Proj.Value))
                    {
                        #region 测试
                        if (TileManager.ServiceProvider.Projection.Equals(ServiceProviderFactory.WebMercProj.Value))
                        {
                            //var r = ServiceProviderFactory.WebMercProj.Value.GeographicInfo.Datum.Spheroid.EquatorialRadius;
                            //var metersPerDegree = Math.PI * r / 180;
                            //var tileVertices = new[] { tiles.TopLeftTile.MinX, tiles.TopLeftTile.MaxY, tiles.BottomRightTile.MaxX, tiles.BottomRightTile.MinY };
                            //Projections.Reproject.ReprojectPoints(tileVertices, z, ServiceProviderFactory.Wgs84Proj.Value, ServiceProviderFactory.WebMercProj.Value, 0, tileVertices.Length / 2);
                            //var metersWidth = tileVertices[2] - tileVertices[0];
                            //var metersPerPixel = metersWidth / width;

                            ////metersPerPixel = TileCalculator.GroundResolution(Map.ViewExtents.Center.Y,tiles.Zoom);

                            //var degreesPerPixel = metersPerPixel / metersPerDegree;
                            //width = (int)Math.Ceiling(bmpExtent.Width / degreesPerPixel);
                            //height = (int)Math.Ceiling(bmpExtent.Height / degreesPerPixel);
                        }
                        #endregion
                    }

                    tileImage = new InRamImageData(stitchedBasemap)
                    {
                        Name = WebMapName,
                        Projection = Map.Projection,
                        Bounds = new RasterBounds(height, width, bmpExtent)
                    };

                    if (!bwProgress(90)) return tileImage;
                }
            }
            return tileImage;
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
            if (Map != null && TileManager != null)
            {
                var imageData = GetTilesFromMapView(bwProgress);
                if (!_bw.CancellationPending)
                {
                    Image = imageData;
                }
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
