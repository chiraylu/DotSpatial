// Copyright (c) DotSpatial Team. All rights reserved.
// Licensed under the MIT license. See License.txt file in the project root for full license information.

using DotSpatial.Projections;
using OSGeo.GDAL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;

namespace DotSpatial.Data.Rasters.GdalExtension
{
    /// <summary>
    /// A GDAL raster.
    /// </summary>
    /// <typeparam name="T">Type of the contained items.</typeparam>
    public class GdalRaster<T> : Raster<T>
        where T : IEquatable<T>, IComparable<T>
    {
        #region Fields
        private readonly Band _band;
        private readonly Dataset _dataset;
        private int _overviewCount;
        private ColorInterp _colorInterp;
        private ImageBuffer _imageBuffer;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="GdalRaster{T}"/> class.
        /// This can be a raster with multiple bands.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <param name="fromDataset">The dataset.</param>
        public GdalRaster(string fileName, Dataset fromDataset)
            : base(fromDataset.RasterYSize, fromDataset.RasterXSize)
        {
            _dataset = fromDataset;
            Filename = fileName;
            Name = Path.GetFileNameWithoutExtension(fileName);
            int numBands = _dataset.RasterCount;
            for (int i = 1; i <= numBands; i++)
            {
                Band band = _dataset.GetRasterBand(i);
                if (i == 1)
                {
                    _band = band;
                }
                Bands.Add(new GdalRaster<T>(fileName, fromDataset, band));
            }
            ReadHeader();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GdalRaster{T}"/> class.
        /// Creates a new raster from the specified band.
        /// </summary>
        /// <param name="fileName">The string path of the file if any.</param>
        /// <param name="fromDataset">The dataset.</param>
        /// <param name="fromBand">The band.</param>
        public GdalRaster(string fileName, Dataset fromDataset, Band fromBand)
            : base(fromDataset.RasterYSize, fromDataset.RasterXSize)
        {
            _dataset = fromDataset;
            _band = fromBand;
            Filename = fileName;
            Name = Path.GetFileNameWithoutExtension(fileName);
            ReadHeader();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the dataset
        /// </summary>
        public Dataset Dataset => _dataset;

        /// <summary>
        /// Gets the GDAL data type.
        /// </summary>
        public DataType GdalDataType => _band.DataType;

        /// <summary>
        /// Gets or sets the maximum.
        /// </summary>
        public override double Maximum
        {
            get
            {
                return base.Maximum;
            }

            protected set
            {
                base.Maximum = value;
                if (_band != null)
                {
                    _band.SetStatistics(Minimum, value, Mean, StdDeviation);
                    _band.SetMetadataItem("STATISTICS_MAXIMUM", Maximum.ToString(), string.Empty);
                }
                else
                {
                    foreach (GdalRaster<T> raster in Bands)
                    {
                        raster.Maximum = value;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the mean.
        /// </summary>
        public override double Mean
        {
            get
            {
                return base.Mean;
            }

            protected set
            {
                base.Mean = value;
                if (_band != null)
                {
                    _band.SetStatistics(Minimum, Maximum, value, StdDeviation);
                    _band.SetMetadataItem("STATISTICS_MEAN", Mean.ToString(), string.Empty);
                }
                else
                {
                    foreach (GdalRaster<T> raster in Bands)
                    {
                        raster.Mean = value;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the minimum.
        /// </summary>
        public override double Minimum
        {
            get
            {
                return base.Minimum;
            }

            protected set
            {
                base.Minimum = value;
                if (_band != null)
                {
                    _band.SetStatistics(value, Maximum, Mean, StdDeviation);
                    _band.SetMetadataItem("STATISTICS_MINIMUM", Minimum.ToString(), string.Empty);
                }
                else
                {
                    foreach (GdalRaster<T> raster in Bands)
                    {
                        raster.Minimum = value;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the NoDataValue.
        /// </summary>
        public override double NoDataValue
        {
            get
            {
                return base.NoDataValue;
            }

            set
            {
                base.NoDataValue = value;
                if (_band != null)
                {
                    _band.SetNoDataValue(value);
                }
                else
                {
                    foreach (var raster in Bands)
                    {
                        raster.NoDataValue = value;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the standard deviation.
        /// </summary>
        public override double StdDeviation
        {
            get
            {
                return base.StdDeviation;
            }

            protected set
            {
                base.StdDeviation = value;
                if (_band != null)
                {
                    _band.SetStatistics(Minimum, Maximum, Mean, value);
                    _band.SetMetadataItem("STATISTICS_STDDEV", StdDeviation.ToString(), string.Empty);
                }
                else
                {
                    foreach (GdalRaster<T> raster in Bands)
                    {
                        raster.StdDeviation = value;
                    }
                }
            }
        }

        #endregion

        #region Methods
        /// <summary>
        /// This needs to return the actual image and override the base
        /// behavior that handles the internal variables only.
        /// </summary>
        /// <param name="envelope">The envelope to grab image data for.</param>
        /// <param name="window">A Rectangle</param>
        /// <returns>The image.</returns>
        public override Bitmap GetBitmap(Extent envelope, Rectangle window)
        {
            if (window.Width == 0 || window.Height == 0)
            {
                return null;
            }

            if (_imageBuffer == null)
            {
                _imageBuffer = new ImageBuffer();
            }
            if (_imageBuffer.Bitmap == null || !envelope.Equals(_imageBuffer.Extent) || !window.Equals(_imageBuffer.Rectangle)) // 相同范围的直接使用缓存图片
            {
                _imageBuffer.Bitmap = new Bitmap(window.Width, window.Height);
                _imageBuffer.Extent = envelope;
                _imageBuffer.Rectangle = window;
                using (var g = Graphics.FromImage(_imageBuffer.Bitmap))
                {
                    try
                    {
                        DrawGraphics(g, envelope, window);
                    }
                    catch (Exception e)
                    {
                        Trace.WriteLine(e);
                    }
                }
            }
            Bitmap result = _imageBuffer.Bitmap.Copy();
            return result;
        }

        private void DrawGraphics(Graphics g, Extent envelope, Rectangle window)
        {
            // Gets the scaling factor for converting from geographic to pixel coordinates
            double dx = window.Width / envelope.Width;
            double dy = window.Height / envelope.Height;

            double[] a = Bounds.AffineCoefficients;

            // calculate inverse
            double p = 1 / ((a[1] * a[5]) - (a[2] * a[4]));
            double[] aInv = new double[4];
            aInv[0] = a[5] * p;
            aInv[1] = -a[2] * p;
            aInv[2] = -a[4] * p;
            aInv[3] = a[1] * p;

            // estimate rectangle coordinates
            double tlx = ((envelope.MinX - a[0]) * aInv[0]) + ((envelope.MaxY - a[3]) * aInv[1]);
            double tly = ((envelope.MinX - a[0]) * aInv[2]) + ((envelope.MaxY - a[3]) * aInv[3]);
            double trx = ((envelope.MaxX - a[0]) * aInv[0]) + ((envelope.MaxY - a[3]) * aInv[1]);
            double trY = ((envelope.MaxX - a[0]) * aInv[2]) + ((envelope.MaxY - a[3]) * aInv[3]);
            double blx = ((envelope.MinX - a[0]) * aInv[0]) + ((envelope.MinY - a[3]) * aInv[1]);
            double bly = ((envelope.MinX - a[0]) * aInv[2]) + ((envelope.MinY - a[3]) * aInv[3]);
            double brx = ((envelope.MaxX - a[0]) * aInv[0]) + ((envelope.MinY - a[3]) * aInv[1]);
            double bry = ((envelope.MaxX - a[0]) * aInv[2]) + ((envelope.MinY - a[3]) * aInv[3]);

            // get absolute maximum and minimum coordinates to make a rectangle on projected coordinates
            // that overlaps all the visible area.
            double tLx = Math.Min(Math.Min(Math.Min(tlx, trx), blx), brx);
            double tLy = Math.Min(Math.Min(Math.Min(tly, trY), bly), bry);
            double bRx = Math.Max(Math.Max(Math.Max(tlx, trx), blx), brx);
            double bRy = Math.Max(Math.Max(Math.Max(tly, trY), bly), bry);

            // limit it to the available image
            // todo: why we compare NumColumns\Rows and X,Y coordinates??
            if (tLx > Bounds.NumColumns) tLx = Bounds.NumColumns;
            if (tLy > Bounds.NumRows) tLy = Bounds.NumRows;
            if (bRx > Bounds.NumColumns) bRx = Bounds.NumColumns;
            if (bRy > Bounds.NumRows) bRy = Bounds.NumRows;

            if (tLx < 0) tLx = 0;
            if (tLy < 0) tLy = 0;
            if (bRx < 0) bRx = 0;
            if (bRy < 0) bRy = 0;

            // gets the affine scaling factors.
            float m11 = Convert.ToSingle(a[1] * dx);
            float m22 = Convert.ToSingle(a[5] * -dy);
            float m21 = Convert.ToSingle(a[2] * dx);
            float m12 = Convert.ToSingle(a[4] * -dy);
            double l = a[0] - (.5 * (a[1] + a[2])); // Left of top left pixel
            double t = a[3] - (.5 * (a[4] + a[5])); // top of top left pixel
            float xShift = (float)((l - envelope.MinX) * dx);
            float yShift = (float)((envelope.MaxY - t) * dy);
            g.PixelOffsetMode = PixelOffsetMode.Half;

            float xRatio = 1, yRatio = 1;
            if (_overviewCount > 0)
            {
                using (Band firstOverview = _band.GetOverview(0))
                {
                    xRatio = (float)firstOverview.XSize / _band.XSize;
                    yRatio = (float)firstOverview.YSize / _band.YSize;
                }
            }
            int overview;
            if (m11 > xRatio || m22 > yRatio)
            {
                // out of pyramids
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                overview = -1; // don't use overviews when zooming behind the max res.
            }
            else
            {
                // estimate the pyramids that we need.
                // when using unreferenced images m11 or m22 can be negative resulting on inf logarithm.
                // so the Math.abs
                overview = (int)Math.Min(Math.Log(Math.Abs(1 / m11), 2), Math.Log(Math.Abs(1 / m22), 2));

                // limit it to the available pyramids
                overview = Math.Min(overview, _overviewCount - 1);

                // additional test but probably not needed
                if (overview < 0)
                {
                    overview = -1;
                }
            }

            var overviewPow = Math.Pow(2, overview + 1);
            m11 *= (float)overviewPow;
            m12 *= (float)overviewPow;
            m21 *= (float)overviewPow;
            m22 *= (float)overviewPow;
            g.Transform = new Matrix(m11, m12, m21, m22, xShift, yShift);

            int blockXsize = 0, blockYsize = 0;

            if (overview >= 0 && _overviewCount > 0)
            {
                using (var overviewBand = _band.GetOverview(overview))
                {
                    overviewBand.ComputeBlockSize(out blockXsize, out blockYsize);
                }
            }
            else
            {
                _band.ComputeBlockSize(out blockXsize, out blockYsize);
            }

            int nbX, nbY;

            // witdh and height of the image
            var w = (bRx - tLx) / overviewPow;
            var h = (bRy - tLy) / overviewPow;

            // limit the block size to the viewable image.
            if (w < blockXsize)
            {
                blockXsize = (int)Math.Ceiling(w);
                if (blockXsize <= 0)
                {
                    return;
                }
                nbX = 1;
            }
            else if (w == blockXsize)
            {
                nbX = 1;
            }
            else
            {
                nbX = (int)Math.Ceiling(w / blockXsize);
            }

            if (h < blockYsize)
            {
                blockYsize = (int)Math.Ceiling(h);
                nbY = 1;
                if (blockYsize <= 0)
                {
                    return;
                }
            }
            else if (h == blockYsize)
            {
                nbY = 1;
            }
            else
            {
                nbY = (int)Math.Ceiling(h / blockYsize);
            }
            int redundancy = (int)Math.Ceiling(Math.Abs(1 / Math.Min(m11, m22)));

            Func<int, int, int, int, Bitmap> getBitmapFunc = null;
            Band rBand = null, gBand = null, bBand = null, aBand = null;
            switch (_colorInterp)
            {
                case ColorInterp.GCI_PaletteIndex:
                    rBand = overview > 0 ? _band.GetOverview(overview) : _band;
                    getBitmapFunc = (xOffset, yOffset, xSize, ySize) => rBand.GetPaletteBitmap(xOffset, yOffset, xSize, ySize, NoDataValue);
                    break;
                case ColorInterp.GCI_GrayIndex:
                    rBand = overview > 0 ? _band.GetOverview(overview) : _band;
                    getBitmapFunc = (xOffset, yOffset, xSize, ySize) => rBand.GetGrayBitmap(xOffset, yOffset, xSize, ySize, NoDataValue);
                    break;
                default:
                    switch (NumBands)
                    {
                        case 0:
                            break;
                        case 1:
                        case 2:
                            rBand = overview > 0 ? _band.GetOverview(overview) : _band;
                            getBitmapFunc = (xOffset, yOffset, xSize, ySize) => rBand.GetGrayBitmap(xOffset, yOffset, xSize, ySize, NoDataValue);
                            break;
                        case 3:
                            rBand = overview > 0 ? (Bands[0] as GdalRaster<T>)._band.GetOverview(overview) : (Bands[0] as GdalRaster<T>)._band;
                            gBand = overview > 0 ? (Bands[1] as GdalRaster<T>)._band.GetOverview(overview) : (Bands[1] as GdalRaster<T>)._band;
                            bBand = overview > 0 ? (Bands[2] as GdalRaster<T>)._band.GetOverview(overview) : (Bands[2] as GdalRaster<T>)._band;
                            getBitmapFunc = (xOffset, yOffset, xSize, ySize) => GdalExtensions.GetRgbBitmap(rBand, gBand, bBand, xOffset, yOffset, xSize, ySize, NoDataValue);
                            break;
                        default:
                            switch (_colorInterp)
                            {
                                case ColorInterp.GCI_RedBand:
                                    rBand = overview > 0 ? (Bands[0] as GdalRaster<T>)._band.GetOverview(overview) : (Bands[0] as GdalRaster<T>)._band;
                                    gBand = overview > 0 ? (Bands[1] as GdalRaster<T>)._band.GetOverview(overview) : (Bands[1] as GdalRaster<T>)._band;
                                    bBand = overview > 0 ? (Bands[2] as GdalRaster<T>)._band.GetOverview(overview) : (Bands[2] as GdalRaster<T>)._band;
                                    aBand = overview > 0 ? (Bands[3] as GdalRaster<T>)._band.GetOverview(overview) : (Bands[3] as GdalRaster<T>)._band;
                                    getBitmapFunc = (xOffset, yOffset, xSize, ySize) => GdalExtensions.GetRgbaBitmap(rBand, gBand, bBand, aBand, xOffset, yOffset, xSize, ySize, NoDataValue);
                                    break;
                                case ColorInterp.GCI_AlphaBand:
                                    aBand = overview > 0 ? (Bands[0] as GdalRaster<T>)._band.GetOverview(overview) : (Bands[0] as GdalRaster<T>)._band;
                                    rBand = overview > 0 ? (Bands[1] as GdalRaster<T>)._band.GetOverview(overview) : (Bands[1] as GdalRaster<T>)._band;
                                    gBand = overview > 0 ? (Bands[2] as GdalRaster<T>)._band.GetOverview(overview) : (Bands[2] as GdalRaster<T>)._band;
                                    bBand = overview > 0 ? (Bands[3] as GdalRaster<T>)._band.GetOverview(overview) : (Bands[3] as GdalRaster<T>)._band;
                                    getBitmapFunc = (xOffset, yOffset, xSize, ySize) => GdalExtensions.GetRgbaBitmap(rBand, gBand, bBand, aBand, xOffset, yOffset, xSize, ySize, NoDataValue);
                                    break;
                            }
                            break;
                    }
                    break;
            }

            // 绘制图片
            for (var i = 0; i < nbX; i++)
            {
                for (var j = 0; j < nbY; j++)
                {
                    // The +1 is to remove the white stripes artifacts
                    double xOffsetD = (tLx / overviewPow) + (i * blockXsize);
                    double yOffsetD = (tLy / overviewPow) + (j * blockYsize);
                    int xOffsetI = (int)Math.Floor(xOffsetD);
                    int yOffsetI = (int)Math.Floor(yOffsetD);
                    int xSize = blockXsize + redundancy;
                    int ySize = blockYsize + redundancy;
                    try
                    {
                        using (var bitmap = getBitmapFunc(xOffsetI, yOffsetI, xSize, ySize))
                        {
                            if (bitmap != null)
                            {
                                g.DrawImage(bitmap, xOffsetI, yOffsetI);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Trace.WriteLine($"获取图片失败，文件:{FilePath},xOffset:{xOffsetI},yOffset:{yOffsetI},xSize:{xSize},ySize:{ySize}，异常：{e}");
                    }
                }
            }

            if (overview > 0)
            {
                rBand?.Dispose();
                gBand?.Dispose();
                bBand?.Dispose();
                aBand?.Dispose();
            }
        }

        /// <summary>
        /// Gets the category colors.
        /// </summary>
        /// <returns>The category colors.</returns>
        public override Color[] CategoryColors()
        {
            Color[] colors = null;
            ColorTable table = GetColorTable();
            if (table != null)
            {
                int colorCount = table.GetCount();
                if (colorCount > 0)
                {
                    colors = new Color[colorCount];
                    for (int colorIndex = 0; colorIndex < colorCount; colorIndex += 1)
                    {
                        colors[colorIndex] = Color.DimGray;
                        ColorEntry entry = table.GetColorEntry(colorIndex);
                        switch (table.GetPaletteInterpretation())
                        {
                            case PaletteInterp.GPI_RGB:
                                colors[colorIndex] = Color.FromArgb(entry.c4, entry.c1, entry.c2, entry.c3);
                                break;
                            case PaletteInterp.GPI_Gray:
                                colors[colorIndex] = Color.FromArgb(255, entry.c1, entry.c1, entry.c1);
                                break;

                                // TODO: do any files use these types?
                                // case PaletteInterp.GPI_HLS
                                // case PaletteInterp.GPI_CMYK
                        }
                    }
                }
            }

            return colors;
        }

        /// <summary>
        /// Gets the category names.
        /// </summary>
        /// <returns>The category names.</returns>
        public override string[] CategoryNames()
        {
            if (_band != null)
            {
                return _band.GetCategoryNames();
            }

            foreach (GdalRaster<T> raster in Bands)
            {
                return raster._band.GetCategoryNames();
            }

            return null;
        }

        /// <summary>
        /// Closes the raster.
        /// </summary>
        public override void Close()
        {
            base.Close();
            if (_band != null)
            {
                _band.Dispose();
            }
            else
            {
                foreach (IRaster raster in Bands)
                {
                    raster.Close();
                    raster.Dispose();
                }
            }

            if (_dataset != null)
            {
                _dataset.FlushCache();
                _dataset.Dispose();
            }
        }

        /// <summary>
        /// Copies the fileName.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <param name="copyValues">Indicates whether the values should be copied.</param>
        public override void Copy(string fileName, bool copyValues)
        {
            using (Driver d = _dataset.GetDriver())
            {
                DataType myType = OSGeo.GDAL.DataType.GDT_Int32;
                if (_band != null)
                {
                    myType = _band.DataType;
                }
                else
                {
                    GdalRaster<T> r = Bands[0] as GdalRaster<T>;
                    if (r != null)
                    {
                        myType = r.GdalDataType;
                    }
                }

                if (copyValues)
                {
                    d.CreateCopy(fileName, _dataset, 1, Options, GdalProgressFunc, "Copy Progress");
                }
                else
                {
                    d.Create(fileName, NumColumnsInFile, NumRowsInFile, NumBands, myType, Options);
                }
            }
        }

        /// <summary>
        /// Gets the mean, standard deviation, minimum and maximum
        /// </summary>
        public override void GetStatistics()
        {
            if (IsInRam && this.IsFullyWindowed())
            {
                base.GetStatistics();
                return;
            }

            if (_band != null)
            {
                double min, max, mean, std;
                CPLErr err;
                try
                {
                    if (Value.Updated) err = _band.ComputeStatistics(false, out min, out max, out mean, out std, null, null);
                    else err = _band.GetStatistics(0, 1, out min, out max, out mean, out std);

                    Value.Updated = false;
                    Minimum = min;
                    Maximum = max;
                    Mean = mean;
                    StdDeviation = std;
                }
                catch (Exception ex)
                {
                    err = CPLErr.CE_Failure;
                    max = min = std = mean = 0;
                    Trace.WriteLine(ex);
                }

                Value.Updated = false;

                // http://dotspatial.codeplex.com/workitem/22221
                // GetStatistics didn't return anything, so try use the raster default method.
                if (err != CPLErr.CE_None || (max == 0 && min == 0 && std == 0 && mean == 0)) base.GetStatistics();
            }
            else
            {
                // ?? doesn't this mean the stats get overwritten several times.
                foreach (IRaster raster in Bands)
                {
                    raster.GetStatistics();
                }
            }
        }
        public override void SaveAs(string fileName, string driverCode, string[] options)
        {
            // Create a new raster file
            IRaster newRaster = DataManager.DefaultDataManager.CreateRaster(fileName, driverCode, NumColumns, NumRows, NumBands, DataType, options);

            // Copy the file based values
            // newRaster.Copy(Filename, true);
            newRaster.Projection = Projection;

            newRaster.Extent = Extent;

            // Copy the in memory value
            newRaster.SetData(this);

            newRaster.ProgressHandler = ProgressHandler;
            newRaster.NoDataValue = NoDataValue;
            newRaster.GetStatistics();
            newRaster.Bounds = Bounds;

            // Save the in-memory values.
            if (newRaster is GdalRaster<T>)
            {
                int count = 1024;
                int xSize, ySize;
                for (int i = 0; i < Bands.Count; i++)
                {
                    var newBand = newRaster.Bands[i];
                    for (int row = 0; row < NumRows; row += count)
                    {
                        ySize = Math.Min(count, NumRows - row);
                        for (int col = 0; col < NumColumns; col += count)
                        {
                            xSize = Math.Min(count, NumColumns - col);
                            IRaster raster = Bands[i].ReadBlock(col, row, xSize, ySize);
                            newBand.WriteBlock(raster, col, row, xSize, ySize);
                        }
                    }
                }
                newRaster.Save();
            }
            else
            {
                newRaster.Save();
            }
            newRaster.Close();
        }
        /// <inheritdoc/>
        public override void Save()
        {
            if (IsDisposed)
            {
                return;
            }
            UpdateHeader();
            _dataset.FlushCache();
        }
        /// <summary>
        /// Most reading is optimized to read in a block at a time and process it. This method is designed
        /// for seeking through the file.  It should work faster than the buffered methods in cases where
        /// an unusually arranged collection of values are required.  Sorting the list before calling
        /// this should significantly improve performance.
        /// </summary>
        /// <param name="indices">A list or array of long values that are (Row * NumRowsInFile + Column)</param>
        /// <returns>The values.</returns>
        public override List<T> GetValuesT(IEnumerable<long> indices)
        {
            if (IsInRam) return base.GetValuesT(indices);

            if (_band == null)
            {
                Raster<T> ri = Bands[CurrentBand] as Raster<T>;

                return ri?.GetValuesT(indices);
            }
#if DEBUG
            var sw = new Stopwatch();
            sw.Start();
#endif
            List<T> result = new List<T>();
            foreach (long index in indices)
            {
                int row = (int)(index / NumColumnsInFile);
                int col = (int)(index % NumColumnsInFile);

                T[] data = new T[1];

                // http://trac.osgeo.org/gdal/wiki/GdalOgrCsharpRaster
                GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
                try
                {
                    IntPtr ptr = handle.AddrOfPinnedObject();
                    _band.ReadRaster(col, row, 1, 1, ptr, 1, 1, GdalDataType, PixelSpace, LineSpace);
                }
                finally
                {
                    if (handle.IsAllocated)
                    {
                        handle.Free();
                    }
                }

                result.Add(data[0]);
            }

#if DEBUG
            sw.Stop();
            Debug.WriteLine("Time to read values from file:" + sw.ElapsedMilliseconds);
#endif
            return result;
        }

        /// <summary>
        /// Reads values from the raster to the jagged array of values
        /// </summary>
        /// <param name="xOff">The horizontal offset from the left to start reading from</param>
        /// <param name="yOff">The vertical offset from the top to start reading from</param>
        /// <param name="sizeX">The number of cells to read horizontally</param>
        /// <param name="sizeY">The number of cells ot read vertically</param>
        /// <returns>A jagged array of values from the raster</returns>
        public override T[][] ReadRaster(int xOff, int yOff, int sizeX, int sizeY)
        {
            T[][] result = new T[sizeY][];
            T[] rawData = new T[sizeY * sizeX];

            if (_band == null)
            {
                Raster<T> ri = Bands[CurrentBand] as Raster<T>;
                if (ri != null)
                {
                    return ri.ReadRaster(xOff, yOff, sizeX, sizeY);
                }
            }
            else
            {
                GCHandle handle = GCHandle.Alloc(rawData, GCHandleType.Pinned);
                try
                {
                    IntPtr ptr = handle.AddrOfPinnedObject();
                    _band.ReadRaster(xOff, yOff, sizeX, sizeY, ptr, sizeX, sizeY, GdalDataType, PixelSpace, LineSpace);
                }
                finally
                {
                    if (handle.IsAllocated)
                    {
                        handle.Free();
                    }
                }

                for (int row = 0; row < sizeY; row++)
                {
                    result[row] = new T[sizeX];
                    Array.Copy(rawData, row * sizeX, result[row], 0, sizeX);
                }

                return result;
            }

            return null;
        }
        protected override void Dispose(bool disposeManagedResources)
        {
            if (disposeManagedResources)
            {
                if (_imageBuffer?.Bitmap != null)
                {
                    _imageBuffer.Bitmap.Dispose();
                    _imageBuffer = null;
                }
                _band?.Dispose();
                _dataset?.Dispose();
            }
            base.Dispose(disposeManagedResources);
        }
        /// <summary>
        /// Writes values from the jagged array to the raster at the specified location
        /// </summary>
        /// <param name="buffer">A jagged array of values to write to the raster</param>
        /// <param name="xOff">The horizontal offset from the left to start reading from</param>
        /// <param name="yOff">The vertical offset from the top to start reading from</param>
        /// <param name="xSize">The number of cells to write horizontally</param>
        /// <param name="ySize">The number of cells ot write vertically</param>
        public override void WriteRaster(T[][] buffer, int xOff, int yOff, int xSize, int ySize)
        {
            if (_band == null)
            {
                Raster<T> ri = Bands[CurrentBand] as Raster<T>;
                if (ri != null)
                {
                    ri.NoDataValue = NoDataValue;

                    ri.WriteRaster(buffer, xOff, yOff, xSize, ySize);
                }
            }
            else
            {
                T[] rawValues = new T[xSize * ySize];
                for (int row = 0; row < ySize; row++)
                {
                    Array.Copy(buffer[row], 0, rawValues, row * xSize, xSize);
                }

                GCHandle handle = GCHandle.Alloc(rawValues, GCHandleType.Pinned);
                try
                {
                    IntPtr ptr = handle.AddrOfPinnedObject();

                    // int stride = ((xSize * sizeof(T) + 7) / 8);
                    _band.WriteRaster(xOff, yOff, xSize, ySize, ptr, xSize, ySize, GdalDataType, PixelSpace, 0);
                    _band.FlushCache();
                    _dataset.FlushCache();
                }
                finally
                {
                    if (handle.IsAllocated)
                    {
                        handle.Free();
                    }
                }
            }
        }

        /// <summary>
        /// Updates the header information about the projection and the affine coefficients
        /// </summary>
        protected override void UpdateHeader()
        {
            if (Bounds != null)
            {
                _dataset.SetGeoTransform(Bounds.AffineCoefficients);
            }

            if (Projection != null)
            {
                _dataset.SetProjection(Projection.ToEsriString());
            }
        }

        /// <summary>
        /// Handles the callback progress content.
        /// </summary>
        /// <param name="complete">Percent of completeness.</param>
        /// <param name="message">Message is not used.</param>
        /// <param name="data">Data is not used.</param>
        /// <returns>0</returns>
        private int GdalProgressFunc(double complete, IntPtr message, IntPtr data)
        {
            ProgressHandler.Progress("Copy Progress", Convert.ToInt32(complete), "Copy Progress");
            return 0;
        }

        private ColorTable GetColorTable()
        {
            if (_band != null)
            {
                return _band.GetColorTable();
            }

            foreach (GdalRaster<T> raster in Bands)
            {
                return raster._band.GetColorTable();
            }

            return null;
        }

        private void ReadHeader()
        {
            DataType = typeof(T);
            NumColumnsInFile = _dataset.RasterXSize;
            NumColumns = NumColumnsInFile;
            NumRowsInFile = _dataset.RasterYSize;
            NumRows = NumRowsInFile;

            // Todo: look for prj file if GetProjection returns null.
            // Do we need to read this as an Esri string if we don't get a proj4 string?
            string projString = _dataset.GetProjection();
            Projection = ProjectionInfo.FromEsriString(projString);
            if (_band != null)
            {
                double val;
                _band.GetNoDataValue(out val, out _);
                base.NoDataValue = val;
                _overviewCount = _band.GetOverviewCount();
                _colorInterp = _band.GetColorInterpretation();
                int maxPixels = 2048 * 2048;
                if (_overviewCount <= 0 && NumColumnsInFile * NumRowsInFile > maxPixels)
                {
                    int ret = Helpers.CreateOverview(_dataset);
                    _overviewCount = _band.GetOverviewCount();
                }
            }

            double[] affine = new double[6];
            _dataset.GetGeoTransform(affine);
            if (_dataset.GetGCPCount() > 0)
            {
                var gcps = _dataset.GetGCPs();
                var ret = Gdal.GCPsToGeoTransform(gcps, affine, 1);
                projString = _dataset.GetGCPProjection();
                Projection = ProjectionInfo.FromEsriString(projString);
            }
            else
            {
                // 解决无投影的栅格影像的反转显示问题
                if (affine[5] > 0)
                {
                    affine[5] = -affine[5];
                }
            }

            // in gdal (row,col) coordinates are defined relative to the top-left corner of the top-left cell
            // shift them by half a cell to give coordinates relative to the center of the top-left cell
            affine = new AffineTransform(affine).TransfromToCorner(0.5, 0.5);
            ProjectionString = projString;
            Bounds = new RasterBounds(NumRows, NumColumns, affine);
            PixelSpace = Marshal.SizeOf(typeof(T));
        }

        #endregion

    }
}