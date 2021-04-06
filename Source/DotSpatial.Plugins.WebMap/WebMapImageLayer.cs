using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using DotSpatial.Controls;
using DotSpatial.Data;
using DotSpatial.Data.Rasters.GdalExtension;
using DotSpatial.Plugins.WebMap.Properties;
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
            set { _webMapName = value; OnWebMapNameOrUrlChanged(); }
        }
        private string _webMapUrl;
        [Serialize("WebMapUrl")]
        public string WebMapUrl
        {
            get { return _webMapUrl; }
            set { _webMapUrl = value; OnWebMapNameOrUrlChanged(); }
        }

        private TileManager _tileManager;

        public TileManager TileManager
        {
            get { return _tileManager; }
            set { _tileManager = value; OnTileManagerChanged(); }
        }

        [Serialize("WebMapExtent")]
        public Extent WebMapExtent { get; set; }

        public override Extent Extent
        {
            get
            {
                Extent extent = WebMapExtent;
                if (TileManager?.ServiceProvider?.Projection?.Equals(Map.Projection) == false)
                {
                    extent = WebMapExtent.Reproject(TileManager.ServiceProvider.Projection, Map.Projection);
                }
                return extent;
            }
        }

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
        public WebMapImageLayer(KnownMaps knownMap) : this()
        {
            WebMapName = Resources.ResourceManager.GetString(knownMap.ToString());
        }
        private void OnWebMapNameOrUrlChanged()
        {
            ServiceProvider provider = null;
            if (!string.IsNullOrEmpty(WebMapName))
            {
                if (!string.IsNullOrEmpty(WebMapUrl))
                {
                    provider = ServiceProviderFactory.Create(WebMapName, WebMapUrl);
                }
                if (provider == null)
                {
                    WebMapExtent = new Extent();
                }
                else
                {
                    if (provider is BrutileServiceProvider brutileServiceProvider)
                    {
                        if (brutileServiceProvider.TileSource != null)
                        {
                            var extent = brutileServiceProvider.TileSource.Schema.Extent;
                            WebMapExtent = new Extent(extent.MinX, extent.MinY, extent.MaxX, extent.MaxY);
                        }
                    }
                    else
                    {
                        var xmin = TileCalculator.MinWebMercX;
                        var ymin = TileCalculator.MinWebMercY;
                        var xmax = TileCalculator.MaxWebMercX;
                        var ymax = TileCalculator.MaxWebMercY;
                        WebMapExtent = new Extent(xmin, ymin, xmax, ymax);
                    }
                }
            }
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
            destRect.Y = (int)Math.Floor(rectangle.Y + (extent.MaxY - ymax) * dy);
            destRect.Width = (int)Math.Ceiling((xmax - xmin) * dx);
            destRect.Height = (int)Math.Ceiling((ymax - ymin) * dy);
            return destRect;
        }

        private IImageData GetImageData(Extent extent, Rectangle rectangle, Func<int, bool> bwProgress)
        {
            IImageData tileImage = null;
            if (Map != null)
            {
                Extent mapViewExtent = extent.Clone() as Extent;
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
                        var clipExtent = new Extent(xmin, ymin, xmax, ymax);
                        geogEnv = clipExtent.Reproject(Map.Projection, ServiceProviderFactory.Wgs84Proj.Value).ToEnvelope();
                        if (bwProgress?.Invoke(25) == false) return tileImage;
                    }
                    else if (Map.Projection.Equals(ServiceProviderFactory.Wgs84Proj.Value))
                    {
                        xmin = TileCalculator.Clip(xmin, TileCalculator.MinLongitude, TileCalculator.MaxLongitude);
                        xmax = TileCalculator.Clip(xmax, TileCalculator.MinLongitude, TileCalculator.MaxLongitude);
                        ymin = TileCalculator.Clip(ymin, TileCalculator.MinLatitude, TileCalculator.MaxLatitude);
                        ymax = TileCalculator.Clip(ymax, TileCalculator.MinLatitude, TileCalculator.MaxLatitude);
                        rectangle = GetRectangle(mapViewExtent, rectangle, xmin, ymin, xmax, ymax);
                        if (bwProgress?.Invoke(25) == false) return tileImage;

                        geogEnv = new Envelope(xmin, xmax, ymin, ymax);
                    }
                    if (bwProgress?.Invoke(40) == false) return tileImage;

                    // Grab the tiles
                    Tiles tiles = TileManager.GetTiles(geogEnv, rectangle, _bw);
                    if (bwProgress?.Invoke(50) == false) return tileImage;

                    // Stitch them into a single image
                    var stitchedBasemap = TileCalculator.StitchTiles(tiles.Bitmaps, _opacity);
                    var bmpExtent = new Extent(tiles.TopLeftTile.MinX, tiles.BottomRightTile.MinY, tiles.BottomRightTile.MaxX, tiles.TopLeftTile.MaxY);
                    int width = stitchedBasemap.Width;
                    int height = stitchedBasemap.Height;
                    if (Map.Projection.Equals(ServiceProviderFactory.WebMercProj.Value))
                    {
                        if (TileManager.ServiceProvider.Projection?.Equals(ServiceProviderFactory.WebMercProj.Value) == true)
                        {
                            var tileExtent = new Extent(tiles.TopLeftTile.MinX, tiles.BottomRightTile.MinY, tiles.BottomRightTile.MaxX, tiles.TopLeftTile.MaxY);
                            bmpExtent = tileExtent.Reproject(ServiceProviderFactory.Wgs84Proj.Value, ServiceProviderFactory.WebMercProj.Value);
                        }

                        //tileImage = new GdalImage(stitchedBasemap, TileManager.ServiceProvider.Projection, bmpExtent)
                        //{
                        //    Name = WebMapName
                        //};
                        tileImage = new InRamImageData(stitchedBasemap)
                        {
                            Name = WebMapName,
                            Projection = TileManager.ServiceProvider.Projection,
                            Bounds = new RasterBounds(height, width, bmpExtent)
                        };
                        if (TileManager.ServiceProvider.Projection?.Equals(Map.Projection) == false)
                        {
                            tileImage.Reproject(Map.Projection);
                            tileImage.Projection = Map.Projection;
                        }
                    }
                    else if (Map.Projection.Equals(ServiceProviderFactory.Wgs84Proj.Value) == true)
                    {
                        var tileExtent = new Extent(tiles.TopLeftTile.MinX, tiles.BottomRightTile.MinY, tiles.BottomRightTile.MaxX, tiles.TopLeftTile.MaxY);
                        if (TileManager.ServiceProvider.Projection?.Equals(ServiceProviderFactory.WebMercProj.Value) == true)
                        {
                            bmpExtent = tileExtent.Reproject(ServiceProviderFactory.Wgs84Proj.Value, ServiceProviderFactory.WebMercProj.Value);
                        }
                        //tileImage = new GdalImage(stitchedBasemap, TileManager.ServiceProvider.Projection, bmpExtent)
                        //{
                        //    Name = WebMapName
                        //};
                        tileImage = new InRamImageData(stitchedBasemap)
                        {
                            Name = WebMapName,
                            Projection = TileManager.ServiceProvider.Projection,
                            Bounds = new RasterBounds(height, width, bmpExtent)
                        };
                        if (TileManager.ServiceProvider.Projection?.Equals(Map.Projection) == false)
                        {
                            tileImage.Reproject(Map.Projection);
                            tileImage.Projection = Map.Projection;
                        }
                    }
                    tiles.Dispose();

                    if (bwProgress?.Invoke(90) == false) return tileImage;
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
                var imageData = GetImageData(Map.ViewExtents, Map.Bounds, bwProgress);
                if (!_bw.CancellationPending)
                {
                    if (IsVisible)
                    {
                        Image = imageData;
                    }
                }
            }
            // report progress and check for cancel
            bwProgress(99);
        }
        private void BwProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // Do we know what what our progress completion percent is (instead of 50)?
            ProgressHandler?.Progress("Loading Basemap ...", e.ProgressPercentage, "Loading Basemap ...");
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
                ProgressHandler?.Progress(string.Empty, 0, string.Empty);
            }
        }
        public override void Print(MapArgs args, List<Extent> regions, bool selected)
        {
            if (selected) return;
            var dataset = GetImageData(args.GeographicExtents, args.ImageRectangle, null);
            DrawRegions(dataset, args, regions, selected);
        }
        private void DrawRegions(IImageData dataset, MapArgs args, List<Extent> regions, bool selected)
        {
            if (selected) return;
            List<Rectangle> clipRects = args.ProjToPixel(regions);
            for (int i = clipRects.Count - 1; i >= 0; i--)
            {
                if (clipRects[i].Width != 0 && clipRects[i].Height != 0) continue;
                regions.RemoveAt(i);
                clipRects.RemoveAt(i);
            }
            DrawWindows(dataset, args, regions, clipRects);
        }
        private void DrawWindows(IImageData dataSet, MapArgs args, List<Extent> regions, List<Rectangle> clipRectangles)
        {
            if (dataSet == null)
            {
                return;
            }
            Graphics g = null;
            if (args.Device != null)
            {
                g = args.Device; // A device on the MapArgs is optional, but overrides the normal buffering behaviors.
            }
            int numBounds = Math.Min(regions.Count, clipRectangles.Count);
            Matrix originMatrix = g.Transform;
            for (int i = 0; i < numBounds; i++)
            {
                // For panning tiles, the region needs to be expanded.
                // This is not always 1 pixel. When very zoomed in, this could be many pixels,
                // but should correspond to 1 pixel in the source image.
                int dx = (int)Math.Ceiling(dataSet.Bounds.AffineCoefficients[1] * clipRectangles[i].Width / regions[i].Width);
                int dy = (int)Math.Ceiling(-dataSet.Bounds.AffineCoefficients[5] * clipRectangles[i].Height / regions[i].Height);

                Rectangle r = clipRectangles[i].ExpandBy(dx * 2, dy * 2);
                if (r.X < 0) r.X = 0;
                if (r.Y < 0) r.Y = 0;
                if (r.Width > 2 * clipRectangles[i].Width) r.Width = 2 * clipRectangles[i].Width;
                if (r.Height > 2 * clipRectangles[i].Height) r.Height = 2 * clipRectangles[i].Height;
                Extent env = regions[i].Reproportion(clipRectangles[i], r);

                Bitmap bmp = null;
                try
                {
                    bmp = dataSet.GetBitmap(env, r);
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"绘制失败：{e}");
                    bmp?.Dispose();
                    continue;
                }

                if (bmp == null) continue;

                if (Symbolizer != null && Symbolizer.Opacity < 1)
                {
                    ColorMatrix matrix = new ColorMatrix
                    {
                        Matrix33 = Symbolizer.Opacity // draws the image not completely opaque
                    };
                    using (var attributes = new ImageAttributes())
                    {
                        attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                        g.DrawImage(bmp, new Rectangle(0, 0, r.Width, r.Height), 0, 0, bmp.Width, bmp.Height, GraphicsUnit.Pixel, attributes);
                    }
                }
                else
                {
                    g.DrawImage(bmp, new Rectangle(0, 0, r.Width, r.Height));
                }
                bmp.Dispose();
            }
            g.Transform = originMatrix;
            if (args.Device == null) g.Dispose();
        }
    }
}
