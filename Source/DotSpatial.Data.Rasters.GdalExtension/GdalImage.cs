// Copyright (c) DotSpatial Team. All rights reserved.
// Licensed under the MIT license. See License.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using DotSpatial.Serialization;
using OSGeo.GDAL;

namespace DotSpatial.Data.Rasters.GdalExtension
{
    /// <summary>
    /// An image based on GDAL data.
    /// </summary>
    public class GdalImage : ImageData
    {
        #region Fields

        private Band _alpha;
        private Band _blue;
        private Dataset _dataset;
        private Band _green;
        private Bitmap _image;
        private bool _isOpened;
        private int _overview;
        private Band _red;

        #endregion

        #region Constructors

        static GdalImage()
        {
            GdalConfiguration.ConfigureGdal();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GdalImage"/> class, and gets much of the header
        /// information without actually reading any values from the file.
        /// </summary>
        /// <param name="fileName">File containing the GDAL image.</param>
        public GdalImage(string fileName)
        {
            Filename = fileName;
            WorldFile = new WorldFile
            {
                Affine = new double[6]
            };
            ReadHeader();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GdalImage"/> class.
        /// </summary>
        public GdalImage()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GdalImage"/> class.
        /// </summary>
        /// <param name="fileName">File containing the GDAL image.</param>
        /// <param name="ds">The dataset.</param>
        /// <param name="band">The band type of the image.</param>
        internal GdalImage(string fileName, Dataset ds, ImageBandType band)
        {
            var setRsColor = (Action<int, ColorInterp>)((i, ci) =>
            {
                using (var bnd = ds.GetRasterBand(i))
                {
                    bnd.SetRasterColorInterpretation(ci);
                }
            });

            _dataset = ds;
            switch (band)
            {
                case ImageBandType.ARGB:
                    setRsColor(1, ColorInterp.GCI_AlphaBand);
                    setRsColor(2, ColorInterp.GCI_RedBand);
                    setRsColor(3, ColorInterp.GCI_GreenBand);
                    setRsColor(4, ColorInterp.GCI_BlueBand);
                    break;
                case ImageBandType.RGB:
                    setRsColor(1, ColorInterp.GCI_RedBand);
                    setRsColor(2, ColorInterp.GCI_GreenBand);
                    setRsColor(3, ColorInterp.GCI_BlueBand);
                    break;
                case ImageBandType.PalletCoded:
                    setRsColor(3, ColorInterp.GCI_PaletteIndex);
                    break;
                default:
                    setRsColor(3, ColorInterp.GCI_GrayIndex);
                    break;
            }

            Filename = fileName;
            WorldFile = new WorldFile
            {
                Affine = new double[6]
            };
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether the file is open. This is a simliar to calling Open/Close methods.
        /// </summary>
        [Serialize("IsOpened")]
        public bool IsOpened
        {
            get
            {
                return _isOpened;
            }

            set
            {
                if (_isOpened == value) return;

                _isOpened = value;
                if (value)
                {
                    Open();
                }
                else
                {
                    Close();
                }
            }
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override void Close()
        {
            _dataset.Dispose();
            _dataset = null;
            _isOpened = false;
        }

        /// <summary>
        /// Reads the actual image values from the image file into the array
        /// of Values, which can be accessed by the Color property.
        /// </summary>
        public override void CopyBitmapToValues()
        {
            BitmapData bData = _image.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            Stride = bData.Stride;
            Marshal.Copy(bData.Scan0, Values, 0, bData.Height * bData.Stride);
            _image.UnlockBits(bData);
        }

        /// <summary>
        /// Writes the byte values stored in the Bytes array into the bitmap image.
        /// </summary>
        public override void CopyValuesToBitmap()
        {
            Rectangle bnds = new Rectangle(0, 0, Width, Height);
            BitmapData bData = _image.LockBits(bnds, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(Values, 0, bData.Scan0, Values.Length);
            _image.UnlockBits(bData);
        }

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

            var result = new Bitmap(window.Width, window.Height);
            using (var g = Graphics.FromImage(result))
            {
                DrawGraphics(g, envelope, window);
            }

            return result;
        }

        /// <summary>
        /// This is only used in the palette indexed band type.
        /// </summary>
        /// <returns>The color palette.</returns>
        public override IEnumerable<Color> GetColorPalette()
        {
            if (ColorPalette == null)
            {
                _dataset = Helpers.Open(Filename);
                using (var first = _dataset.GetRasterBand(1))
                using (var ct = first.GetRasterColorTable())
                {
                    int count = ct.GetCount();
                    var result = new List<Color>(count);
                    for (int i = 0; i < count; i++)
                    {
                        using (var ce = ct.GetColorEntry(i)) result.Add(Color.FromArgb(ce.c4, ce.c1, ce.c2, ce.c3));
                    }

                    ColorPalette = result;
                }
            }

            return ColorPalette;
        }

        /// <summary>
        /// Attempts to open the specified file into memory.
        /// </summary>
        public override void Open()
        {
            _dataset = Helpers.Open(Filename);
            int numBands = _dataset.RasterCount;
            _red = _dataset.GetRasterBand(1);
            BandType = ImageBandType.RGB;
            for (int i = 1; i <= numBands; i++)
            {
                var tempBand = _dataset.GetRasterBand(i);
                switch (tempBand.GetRasterColorInterpretation())
                {
                    case ColorInterp.GCI_GrayIndex:
                        BandType = ImageBandType.Gray;
                        _red = tempBand;
                        break;
                    case ColorInterp.GCI_PaletteIndex:
                        BandType = ImageBandType.PalletCoded;
                        _red = tempBand;
                        break;
                    case ColorInterp.GCI_RedBand:
                        _red = tempBand;
                        break;
                    case ColorInterp.GCI_AlphaBand:
                        _alpha = tempBand;
                        BandType = ImageBandType.ARGB;
                        break;
                    case ColorInterp.GCI_BlueBand:
                        _blue = tempBand;
                        break;
                    case ColorInterp.GCI_GreenBand:
                        _green = tempBand;
                        break;
                }
            }

            _isOpened = true;
        }

        /// <summary>
        /// Gets a block of data directly, converted into a bitmap.  This always writes
        /// to the base layer, not the overviews.
        /// </summary>
        /// <param name="xOffset">The zero based integer column offset from the left</param>
        /// <param name="yOffset">The zero based integer row offset from the top</param>
        /// <param name="xSize">The integer number of pixel columns in the block. </param>
        /// <param name="ySize">The integer number of pixel rows in the block.</param>
        /// <returns>A Bitmap that is xSize, ySize.</returns>
        public override Bitmap ReadBlock(int xOffset, int yOffset, int xSize, int ySize)
        {
            if (_dataset == null)
            {
                _dataset = Helpers.Open(Filename);
            }

            Bitmap result = null;
            using (var first = _dataset.GetRasterBand(1))
            {
                Action action = new Action(() =>
                {
                    switch (NumBands)
                    {
                        case 0:
                            break;
                        case 1:
                        case 2:
                            result = ReadGrayIndex(xOffset, yOffset, xSize, ySize, first);
                            break;
                        case 3:
                            result = ReadRgb(xOffset, yOffset, xSize, ySize, first, _dataset);
                            break;
                        default:
                            result = ReadArgb(xOffset, yOffset, xSize, ySize, first, _dataset);
                            break;
                    }
                });
                switch (first.GetRasterColorInterpretation())
                {
                    case ColorInterp.GCI_PaletteIndex:
                        result = ReadPaletteBuffered(xOffset, yOffset, xSize, ySize, first);
                        break;
                    case ColorInterp.GCI_GrayIndex:
                        result = ReadGrayIndex(xOffset, yOffset, xSize, ySize, first);
                        break;
                    case ColorInterp.GCI_RedBand:
                        result = ReadRgb(xOffset, yOffset, xSize, ySize, first, _dataset);
                        break;
                    case ColorInterp.GCI_AlphaBand:
                        result = ReadArgb(xOffset, yOffset, xSize, ySize, first, _dataset);
                        break;
                    default:
                        action.Invoke();
                        break;
                }
            }

            // data set disposed on disposing this image
            return result;
        }

        /// <summary>
        /// This should update the palette cached and in the file.
        /// </summary>
        /// <param name="value">The color palette.</param>
        public override void SetColorPalette(IEnumerable<Color> value)
        {
            var colorPalette = value as IList<Color> ?? value.ToList();
            ColorPalette = colorPalette;
            _dataset = Gdal.Open(Filename, Access.GA_Update);

            ColorTable ct = new ColorTable(PaletteInterp.GPI_RGB);
            int index = 0;
            foreach (var c in colorPalette)
            {
                ColorEntry ce = new ColorEntry
                {
                    c4 = c.A,
                    c3 = c.B,
                    c2 = c.G,
                    c1 = c.R
                };
                ct.SetColorEntry(index, ce);
                index++;
            }

            using (Band first = _dataset.GetRasterBand(1))
            {
                first.SetRasterColorTable(ct);
            }
        }

        /// <summary>
        /// Saves a bitmap of data as a continuous block into the specified location.
        /// This always writes to the base image, and not the overviews.
        /// </summary>
        /// <param name="value">The bitmap value to save.</param>
        /// <param name="xOffset">The zero based integer column offset from the left</param>
        /// <param name="yOffset">The zero based integer row offset from the top</param>
        public override void WriteBlock(Bitmap value, int xOffset, int yOffset)
        {
            if (_dataset == null)
            {
                // This will fail if write access is not allowed, but just pass the
                // exception back up the stack.
                _dataset = Gdal.Open(Filename, Access.GA_Update);
            }

            using (var first = _dataset.GetRasterBand(1))
            {
                switch (BandType)
                {
                    case ImageBandType.PalletCoded:
                        WritePaletteBuffered(value, xOffset, yOffset, first);
                        break;
                    case ImageBandType.Gray:
                        WriteGrayIndex(value, xOffset, yOffset, first);
                        break;
                    case ImageBandType.RGB:
                        WriteRgb(value, xOffset, yOffset, first, _dataset);
                        break;
                    case ImageBandType.ARGB:
                        WriteArgb(value, xOffset, yOffset, first, _dataset);
                        break;
                }
            }
        }

        /// <summary>
        /// CreateOverview
        /// </summary>
        /// <param name="resampling">resampling</param>
        /// <param name="overviewlist">overviewlist</param>
        /// <returns>overviews count</returns>
        public int CreateOverview(string resampling = "NEAREST", int[] overviewlist = null)
        {
            int value = -1;
            if (_dataset == null)
            {
                return value;
            }

            if (overviewlist == null)
            {
                List<int> intList = new List<int>();
                int width = Width;
                int height = Height;
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

        /// <inheritdoc />
        protected override void Dispose(bool disposeManagedResources)
        {
            // All class variables are unmanaged.
            if (_dataset != null)
            {
                try
                {
                    _dataset.FlushCache();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                _dataset.Dispose();
            }

            _dataset = null;

            _image?.Dispose();
            _image = null;

            _red?.Dispose();
            _red = null;

            _blue?.Dispose();
            _blue = null;

            _green?.Dispose();
            _green = null;

            _alpha?.Dispose();
            _alpha = null;

            base.Dispose(disposeManagedResources);
        }

        /// <summary>
        /// Finds the closest color in the table based on the hamming distance.
        /// </summary>
        /// <param name="vals">The values.</param>
        /// <param name="offset">The offset</param>
        /// <param name="colorTable">The color table.</param>
        /// <returns>The closest color.</returns>
        private static byte MatchColor(byte[] vals, int offset, byte[][] colorTable)
        {
            int shortestDistance = int.MaxValue;
            int index = 0;
            int result = 0;
            foreach (byte[] color in colorTable)
            {
                int dist = Math.Abs(vals[offset] - color[0]);
                dist += Math.Abs(vals[offset + 1] - color[1]);
                dist += Math.Abs(vals[offset + 2] - color[2]);
                dist += Math.Abs(vals[offset + 3] - color[0]);
                if (dist < shortestDistance)
                {
                    shortestDistance = dist;
                    result = index;
                }

                index++;
            }

            return Convert.ToByte(result);
        }

        private static void NormalizeSizeToBand(int xOffset, int yOffset, int xSize, int ySize, Band band, out int width, out int height)
        {
            width = xSize;
            height = ySize;

            if (xOffset + width > band.XSize)
            {
                width = band.XSize - xOffset;
            }

            if (yOffset + height > band.YSize)
            {
                height = band.YSize - yOffset;
            }
        }

        private static void WriteArgb(Bitmap value, int xOffset, int yOffset, Band first, Dataset set)
        {
            if (set.RasterCount < 4)
            {
                throw new GdalException("ARGB Format was indicated but there are only " + set.RasterCount + " bands!");
            }

            int width = value.Width;
            int height = value.Height;

            BitmapData bData = value.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            int stride = Math.Abs(bData.Stride);
            byte[] vals = new byte[stride * height];
            Marshal.Copy(bData.Scan0, vals, 0, vals.Length);
            value.UnlockBits(bData);
            byte[] a = new byte[width * height];
            byte[] r = new byte[width * height];
            byte[] g = new byte[width * height];
            byte[] b = new byte[width * height];

            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    b[(row * width) + col] = vals[(row * stride) + (col * 4)];
                    g[(row * width) + col] = vals[(row * stride) + (col * 4) + 1];
                    r[(row * width) + col] = vals[(row * stride) + (col * 4) + 2];
                    a[(row * width) + col] = vals[(row * stride) + (col * 4) + 3];
                }
            }

            Band red = set.GetRasterBand(2);
            Band green = set.GetRasterBand(3);
            Band blue = set.GetRasterBand(4);
            first.WriteRaster(xOffset, yOffset, width, height, a, width, height, 0, 0);
            red.WriteRaster(xOffset, yOffset, width, height, r, width, height, 0, 0);
            green.WriteRaster(xOffset, yOffset, width, height, g, width, height, 0, 0);
            blue.WriteRaster(xOffset, yOffset, width, height, b, width, height, 0, 0);

            // first disposed in caller
            green.Dispose();
            blue.Dispose();
        }

        private static void WriteGrayIndex(Bitmap value, int xOffset, int yOffset, Band first)
        {
            int width = value.Width;
            int height = value.Height;
            BitmapData bData = value.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            int stride = Math.Abs(bData.Stride);

            byte[] r = new byte[stride * height * 4];

            Marshal.Copy(bData.Scan0, r, 0, r.Length);

            byte[] vals = new byte[height * stride];

            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    byte blue = r[(row * stride) + (col * 4)];
                    byte green = r[(row * stride) + (col * 4) + 1];
                    byte red = r[(row * stride) + (col * 4) + 2];
                    int gray = Convert.ToInt32((.3 * red) + (.59 * green) + (.11 * blue));
                    if (gray > 255) gray = 255;
                    if (gray < 0) gray = 0;
                    vals[(row * width) + col] = Convert.ToByte(gray);
                }
            }

            first.WriteRaster(xOffset, yOffset, width, height, vals, width, height, 0, 0);
        }

        private static void WritePaletteBuffered(Bitmap value, int xOffset, int yOffset, Band first)
        {
            ColorTable ct = first.GetRasterColorTable();
            if (ct == null)
            {
                throw new GdalException("Image was stored with a palette interpretation but has no color table.");
            }

            if (ct.GetPaletteInterpretation() != PaletteInterp.GPI_RGB)
            {
                throw new GdalException("Only RGB palette interpretation is currently supported by this " + " plug-in, " + ct.GetPaletteInterpretation() + " is not supported.");
            }

            int width = value.Width;
            int height = value.Height;
            BitmapData bData = value.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            int stride = Math.Abs(bData.Stride);
            byte[] r = new byte[stride * height];
            Marshal.Copy(bData.Scan0, r, 0, r.Length);
            value.UnlockBits(bData);
            byte[] vals = new byte[width * height];
            byte[][] colorTable = new byte[ct.GetCount()][];
            for (int i = 0; i < ct.GetCount(); i++)
            {
                ColorEntry ce = ct.GetColorEntry(i);
                colorTable[i] = new[] { (byte)ce.c3, (byte)ce.c2, (byte)ce.c1, (byte)ce.c4 };
            }

            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    vals[(row * width) + col] = MatchColor(r, (row * stride) + (col * 4), colorTable);
                }
            }

            first.WriteRaster(xOffset, yOffset, width, height, vals, width, height, 0, 0);
        }

        private static void WriteRgb(Bitmap value, int xOffset, int yOffset, Band first, Dataset set)
        {
            if (set.RasterCount < 3)
            {
                throw new GdalException("RGB Format was indicated but there are only " + set.RasterCount + " bands!");
            }

            int width = value.Width;
            int height = value.Height;

            BitmapData bData = value.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int stride = Math.Abs(bData.Stride);
            byte[] vals = new byte[stride * height];
            Marshal.Copy(bData.Scan0, vals, 0, vals.Length);
            value.UnlockBits(bData);
            byte[] r = new byte[width * height];
            byte[] g = new byte[width * height];
            byte[] b = new byte[width * height];

            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    b[(row * width) + col] = vals[(row * stride) + (col * 4)];
                    g[(row * width) + col] = vals[(row * stride) + (col * 4) + 1];
                    r[(row * width) + col] = vals[(row * stride) + (col * 4) + 2];
                }
            }

            Band green = set.GetRasterBand(2);
            Band blue = set.GetRasterBand(3);
            first.WriteRaster(xOffset, yOffset, width, height, r, width, height, 0, 0);
            first.FlushCache();
            green.WriteRaster(xOffset, yOffset, width, height, g, width, height, 0, 0);
            green.FlushCache();
            blue.WriteRaster(xOffset, yOffset, width, height, b, width, height, 0, 0);
            blue.FlushCache();

            // first disposed in caller
            green.Dispose();
            blue.Dispose();
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

            int overviewCount = _red.GetOverviewCount();
            float xRatio = 1, yRatio = 1;
            if (overviewCount > 0)
            {
                using (Band firstOverview = _red.GetOverview(0))
                {
                    xRatio = (float)firstOverview.XSize / _red.XSize;
                    yRatio = (float)firstOverview.YSize / _red.XSize;
                }
            }
            if (m11 > xRatio || m22 > yRatio)
            {
                // out of pyramids
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                _overview = -1; // don't use overviews when zooming behind the max res.
            }
            else
            {
                // estimate the pyramids that we need.
                // when using unreferenced images m11 or m22 can be negative resulting on inf logarithm.
                // so the Math.abs
                _overview = (int)Math.Min(Math.Log(Math.Abs(1 / m11), 2), Math.Log(Math.Abs(1 / m22), 2));

                // limit it to the available pyramids
                _overview = Math.Min(_overview, overviewCount - 1);

                // additional test but probably not needed
                if (_overview < 0)
                {
                    _overview = -1;
                }
            }

            var overviewPow = Math.Pow(2, _overview + 1);

            // witdh and height of the image
            var w = (bRx - tLx) / overviewPow;
            var h = (bRy - tLy) / overviewPow;

            using (var matrix = new Matrix(m11 * (float)overviewPow, m12 * (float)overviewPow, m21 * (float)overviewPow, m22 * (float)overviewPow, xShift, yShift))
            {
                g.Transform = matrix;
            }

            int blockXsize, blockYsize;

            // get the optimal block size to request gdal.
            // if the image is stored line by line then ask for a 100px stripe.
            if (_overview >= 0 && overviewCount > 0)
            {
                using (var overview = _red.GetOverview(_overview))
                {
                    overview.GetBlockSize(out blockXsize, out blockYsize);
                    if (blockYsize == 1)
                    {
                        blockYsize = Math.Min(100, overview.YSize);
                    }
                }
            }
            else
            {
                _red.GetBlockSize(out blockXsize, out blockYsize);
                if (blockYsize == 1)
                {
                    blockYsize = Math.Min(100, _red.YSize);
                }
            }

            int nbX, nbY;

            // limit the block size to the viewable image.
            if (w < blockXsize)
            {
                blockXsize = (int)w;
                nbX = 1;
            }
            else
            {
                nbX = (int)(w / blockXsize) + 1;
            }

            if (h < blockYsize)
            {
                blockYsize = (int)h;
                nbY = 1;
            }
            else
            {
                nbY = (int)(h / blockYsize) + 1;
            }

            for (var i = 0; i < nbX; i++)
            {
                for (var j = 0; j < nbY; j++)
                {
                    // The +1 is to remove the white stripes artifacts
                    int xOffset = (int)(tLx / overviewPow) + (i * blockXsize);
                    int yOffset = (int)(tLy / overviewPow) + (j * blockYsize);
                    int xSize = blockXsize + 1;
                    int ySize = blockYsize + 1;
                    using (var bitmap = ReadBlock(xOffset, yOffset, xSize, ySize))
                    {
                        if (bitmap != null)
                        {
                            g.DrawImage(bitmap, xOffset, yOffset);
                        }
                    }
                }
            }
        }

        private Bitmap ReadArgb(int xOffset, int yOffset, int xSize, int ySize, Band first, Dataset set)
        {
            if (set.RasterCount < 4)
            {
                throw new GdalException("ARGB Format was indicated but there are only " + set.RasterCount + " bands!");
            }

            Band firstO;
            Band redO;
            Band greenO;
            Band blueO;
            var disposeO = false;

            Band red = set.GetRasterBand(2);
            Band green = set.GetRasterBand(3);
            Band blue = set.GetRasterBand(4);

            if (_overview >= 0 && first.GetOverviewCount() > 0)
            {
                firstO = first.GetOverview(_overview);
                redO = red.GetOverview(_overview);
                greenO = green.GetOverview(_overview);
                blueO = blue.GetOverview(_overview);
                disposeO = true;
            }
            else
            {
                firstO = first;
                redO = red;
                greenO = green;
                blueO = blue;
            }

            int width, height;
            NormalizeSizeToBand(xOffset, yOffset, xSize, ySize, firstO, out width, out height);
            Bitmap result = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            byte[] a = new byte[width * height];
            byte[] r = new byte[width * height];
            byte[] g = new byte[width * height];
            byte[] b = new byte[width * height];

            firstO.ReadRaster(0, 0, width, height, a, width, height, 0, 0);
            redO.ReadRaster(0, 0, width, height, r, width, height, 0, 0);
            greenO.ReadRaster(0, 0, width, height, g, width, height, 0, 0);
            blueO.ReadRaster(0, 0, width, height, b, width, height, 0, 0);

            if (disposeO)
            {
                firstO.Dispose();
                redO.Dispose();
                greenO.Dispose();
                blueO.Dispose();
            }

            // Alpha disposed in caller
            red.Dispose();
            green.Dispose();
            blue.Dispose();

            BitmapData bData = result.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            int stride = Math.Abs(bData.Stride);

            byte[] vals = new byte[height * stride];

            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    vals[(row * stride) + (col * 4)] = b[(row * width) + col];
                    vals[(row * stride) + (col * 4) + 1] = g[(row * width) + col];
                    vals[(row * stride) + (col * 4) + 2] = r[(row * width) + col];
                    vals[(row * stride) + (col * 4) + 3] = a[(row * width) + col];
                }
            }

            Marshal.Copy(vals, 0, bData.Scan0, vals.Length);
            result.UnlockBits(bData);
            return result;
        }

        private byte[] ReadBand(Band band, int xOffset, int yOffset, int xSize, int ySize, int width, int height)
        {
            int length = width * height;
            byte[] buffer = new byte[length];
            DataType dataType = band.DataType;
            IntPtr bufferPtr = IntPtr.Zero;
            // Percentage truncation
            double minPercent = 0.5;
            double maxPercent = 0.5;
            band.GetMaximum(out double maxValue, out int hasvalue);
            band.GetMinimum(out double minValue, out hasvalue);
            double dValue = maxValue - minValue;
            double highValue = maxValue - dValue * maxPercent / 100;
            double lowValue = minValue + dValue * minPercent / 100;
            switch (dataType)
            {
                case DataType.GDT_Unknown:
                    throw new Exception("Unknown datatype");
                case DataType.GDT_Byte:
                    {
                        byte[] tmpBuffer = new byte[length];
                        bufferPtr = GCHandleHelper.GetIntPtr(tmpBuffer);
                        band.ReadRaster(xOffset, yOffset, width, height, bufferPtr, width, height, dataType, 0, 0);
                        dValue = (byte)(highValue - lowValue);
                        for (int i = 0; i < length; i++)
                        {
                            byte value = tmpBuffer[i];
                            if (value >= highValue)
                            {
                                buffer[i] = 255;
                            }
                            else if (value <= lowValue)
                            {
                                buffer[i] = 0;
                            }
                            else
                            {
                                buffer[i] = (byte)((value - lowValue) / dValue * 255);
                            }
                        }
                    }
                    break;
                case DataType.GDT_UInt16:
                    {
                        ushort[] tmpBuffer = new ushort[length];
                        bufferPtr = GCHandleHelper.GetIntPtr(tmpBuffer);
                        band.ReadRaster(xOffset, yOffset, width, height, bufferPtr, width, height, dataType, 0, 0);
                        dValue = (ushort)(highValue - lowValue);
                        for (int i = 0; i < length; i++)
                        {
                            ushort value = tmpBuffer[i];
                            if (value >= highValue)
                            {
                                buffer[i] = 255;
                            }
                            else if (value <= lowValue)
                            {
                                buffer[i] = 0;
                            }
                            else
                            {
                                buffer[i] = (byte)((value - lowValue) / dValue * 255);
                            }
                        }
                    }
                    break;
                case DataType.GDT_Int16:
                    {
                        short[] tmpBuffer = new short[length];
                        bufferPtr = GCHandleHelper.GetIntPtr(tmpBuffer);
                        band.ReadRaster(xOffset, yOffset, width, height, bufferPtr, width, height, dataType, 0, 0);
                        dValue = (short)(highValue - lowValue);
                        for (int i = 0; i < length; i++)
                        {
                            short value = tmpBuffer[i];
                            if (value >= highValue)
                            {
                                buffer[i] = 255;
                            }
                            else if (value <= lowValue)
                            {
                                buffer[i] = 0;
                            }
                            else
                            {
                                buffer[i] = (byte)((value - lowValue) / dValue * 255);
                            }
                        }
                    }
                    break;
                case DataType.GDT_UInt32:
                    {
                        uint[] tmpBuffer = new uint[length];
                        bufferPtr = GCHandleHelper.GetIntPtr(tmpBuffer);
                        band.ReadRaster(xOffset, yOffset, width, height, bufferPtr, width, height, dataType, 0, 0);
                        dValue = (uint)(highValue - lowValue);
                        for (int i = 0; i < length; i++)
                        {
                            uint value = tmpBuffer[i];
                            if (value >= highValue)
                            {
                                buffer[i] = 255;
                            }
                            else if (value <= lowValue)
                            {
                                buffer[i] = 0;
                            }
                            else
                            {
                                buffer[i] = (byte)((value - lowValue) / dValue * 255);
                            }
                        }
                    }
                    break;
                case DataType.GDT_Int32:
                    {
                        int[] tmpBuffer = new int[length];
                        bufferPtr = GCHandleHelper.GetIntPtr(tmpBuffer);
                        band.ReadRaster(xOffset, yOffset, width, height, bufferPtr, width, height, dataType, 0, 0);
                        dValue = (int)(highValue - lowValue);
                        for (int i = 0; i < length; i++)
                        {
                            int value = tmpBuffer[i];
                            if (value >= highValue)
                            {
                                buffer[i] = 255;
                            }
                            else if (value <= lowValue)
                            {
                                buffer[i] = 0;
                            }
                            else
                            {
                                buffer[i] = (byte)((value - lowValue) / dValue * 255);
                            }
                        }
                    }
                    break;
                case DataType.GDT_Float32:
                    {
                        float[] tmpBuffer = new float[length];
                        bufferPtr = GCHandleHelper.GetIntPtr(tmpBuffer);
                        band.ReadRaster(xOffset, yOffset, width, height, bufferPtr, width, height, dataType, 0, 0);
                        dValue = (float)(highValue - lowValue);
                        for (int i = 0; i < length; i++)
                        {
                            float value = tmpBuffer[i];
                            if (value >= highValue)
                            {
                                buffer[i] = 255;
                            }
                            else if (value <= lowValue)
                            {
                                buffer[i] = 0;
                            }
                            else
                            {
                                buffer[i] = (byte)((value - lowValue) / dValue * 255);
                            }
                        }
                    }
                    break;
                case DataType.GDT_Float64:
                    {
                        double[] tmpBuffer = new double[length];
                        bufferPtr = GCHandleHelper.GetIntPtr(tmpBuffer);
                        band.ReadRaster(xOffset, yOffset, width, height, bufferPtr, width, height, dataType, 0, 0);
                        dValue = (double)(highValue - lowValue);
                        for (int i = 0; i < length; i++)
                        {
                            double value = tmpBuffer[i];
                            if (value >= highValue)
                            {
                                buffer[i] = 255;
                            }
                            else if (value <= lowValue)
                            {
                                buffer[i] = 0;
                            }
                            else
                            {
                                buffer[i] = (byte)((value - lowValue) / dValue * 255);
                            }
                        }
                    }
                    break;
                case DataType.GDT_CInt16:
                case DataType.GDT_CInt32:
                case DataType.GDT_CFloat32:
                case DataType.GDT_CFloat64:
                case DataType.GDT_TypeCount:
                    throw new NotImplementedException();
            }
            return buffer;
        }
        private Bitmap ReadGrayIndex(int xOffset, int yOffset, int xSize, int ySize, Band first)
        {
            Band firstO;
            var disposeO = false;

            if (_overview >= 0 && first.GetOverviewCount() > 0)
            {
                firstO = first.GetOverview(_overview);
                disposeO = true;
            }
            else
            {
                firstO = first;
            }

            int width, height;
            NormalizeSizeToBand(xOffset, yOffset, xSize, ySize, firstO, out width, out height);

            Bitmap result = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            byte[] r = ReadBand(firstO, xOffset, yOffset, xSize, ySize, width, height);
            if (disposeO)
            {
                firstO.Dispose();
            }

            BitmapData bData = result.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            int stride = Math.Abs(bData.Stride);

            byte[] vals = new byte[height * stride];

            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    byte value = r[(row * width) + col];
                    vals[(row * stride) + (col * 4)] = value;
                    vals[(row * stride) + (col * 4) + 1] = value;
                    vals[(row * stride) + (col * 4) + 2] = value;
                    vals[(row * stride) + (col * 4) + 3] = 255;
                }
            }

            Marshal.Copy(vals, 0, bData.Scan0, vals.Length);
            result.UnlockBits(bData);
            return result;
        }

        /// <summary>
        /// Gets the size of the whole image, but doesn't keep the image open
        /// unless it was already open.
        /// </summary>
        private void ReadHeader()
        {
            _dataset = Helpers.Open(Filename);
            Width = _dataset.RasterXSize;
            Height = _dataset.RasterYSize;
            NumBands = _dataset.RasterCount;
            var test = new double[6];
            _dataset.GetGeoTransform(test);
            test = new AffineTransform(test).TransfromToCorner(0.5, 0.5); // shift origin by half a cell
            ProjectionString = _dataset.GetProjection();
            Bounds = new RasterBounds(Height, Width, test);
            WorldFile.Affine = test;
            Close();
        }

        private Bitmap ReadPaletteBuffered(int xOffset, int yOffset, int xSize, int ySize, Band first)
        {
            ColorTable ct = first.GetRasterColorTable();
            if (ct == null)
            {
                throw new GdalException("Image was stored with a palette interpretation but has no color table.");
            }

            if (ct.GetPaletteInterpretation() != PaletteInterp.GPI_RGB)
            {
                throw new GdalException("Only RGB palette interpretation is currently supported by this " + " plug-in, " + ct.GetPaletteInterpretation() + " is not supported.");
            }

            Band firstO;
            bool disposeO = false;
            if (_overview >= 0 && first.GetOverviewCount() > 0)
            {
                firstO = first.GetOverview(_overview);
                disposeO = true;
            }
            else
            {
                firstO = first;
            }

            int width, height;
            NormalizeSizeToBand(xOffset, yOffset, xSize, ySize, firstO, out width, out height);
            byte[] r = new byte[width * height];

            firstO.ReadRaster(xOffset, yOffset, width, height, r, width, height, 0, 0);
            if (disposeO)
            {
                firstO.Dispose();
            }

            Bitmap result = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            BitmapData bData = result.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            int stride = Math.Abs(bData.Stride);

            const int Bpp = 4;
            byte[] vals = new byte[stride * height];
            byte[][] colorTable = new byte[ct.GetCount()][];
            for (int i = 0; i < ct.GetCount(); i++)
            {
                ColorEntry ce = ct.GetColorEntry(i);
                colorTable[i] = new[] { (byte)ce.c3, (byte)ce.c2, (byte)ce.c1, (byte)ce.c4 };
            }

            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    Array.Copy(colorTable[r[col + (row * width)]], 0, vals, (row * stride) + (col * Bpp), 4);
                }
            }

            Marshal.Copy(vals, 0, bData.Scan0, vals.Length);
            result.UnlockBits(bData);
            return result;
        }

        private Bitmap ReadRgb(int xOffset, int yOffset, int xSize, int ySize, Band first, Dataset set)
        {
            if (set.RasterCount < 3)
            {
                throw new GdalException("RGB Format was indicated but there are only " + set.RasterCount + " bands!");
            }

            Band firstO;
            Band greenO;
            Band blueO;
            var disposeO = false;

            var green = set.GetRasterBand(2);
            var blue = set.GetRasterBand(3);
            if (_overview >= 0 && first.GetOverviewCount() > 0)
            {
                firstO = first.GetOverview(_overview);
                greenO = green.GetOverview(_overview);
                blueO = blue.GetOverview(_overview);
                disposeO = true;
            }
            else
            {
                firstO = first;
                greenO = green;
                blueO = blue;
            }

            int width, height;
            NormalizeSizeToBand(xOffset, yOffset, xSize, ySize, firstO, out width, out height);
            if (xOffset >= firstO.XSize || yOffset >= firstO.YSize)
            {
                return null;
            }
            Bitmap result = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            byte[] r = new byte[width * height];
            byte[] g = new byte[width * height];
            byte[] b = new byte[width * height];

            firstO.ReadRaster(xOffset, yOffset, width, height, r, width, height, 0, 0);
            greenO.ReadRaster(xOffset, yOffset, width, height, g, width, height, 0, 0);
            blueO.ReadRaster(xOffset, yOffset, width, height, b, width, height, 0, 0);

            if (disposeO)
            {
                // It's local copies, dispose them
                firstO.Dispose();
                greenO.Dispose();
                blueO.Dispose();
            }

            green.Dispose();
            blue.Dispose();

            var bData = result.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            int stride = Math.Abs(bData.Stride);

            var vals = new byte[width * height * 4];
            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    vals[(row * stride) + (col * 4)] = b[(row * width) + col];
                    vals[(row * stride) + (col * 4) + 1] = g[(row * width) + col];
                    vals[(row * stride) + (col * 4) + 2] = r[(row * width) + col];
                    vals[(row * stride) + (col * 4) + 3] = 255;
                }
            }

            Marshal.Copy(vals, 0, bData.Scan0, vals.Length);
            result.UnlockBits(bData);
            return result;
        }

        #endregion

    }
}