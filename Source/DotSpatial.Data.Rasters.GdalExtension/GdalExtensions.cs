using OSGeo.GDAL;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace DotSpatial.Data.Rasters.GdalExtension
{
    /// <summary>
    /// Gdal extensions
    /// </summary>
    public static class GdalExtensions
    {
        private static object _lockObj;
        static GdalExtensions()
        {
            _lockObj = new object();
        }

        /// <summary>
        /// brief Compose two geotransforms.
        /// </summary>
        /// <param name="padfGT1">geoTransform1</param>
        /// <param name="padfGT2">geoTransform2</param> 
        /// <returns>geoTransform</returns>
        public static double[] GDALComposeGeoTransforms(double[] padfGT1, double[] padfGT2)
        {
            double[] gtwrk = new double[6];
            // We need to think of the geotransform in a more normal form to do
            // the matrix multiple:
            //
            //  __                     __
            //  | gt[1]   gt[2]   gt[0] |
            //  | gt[4]   gt[5]   gt[3] |
            //  |  0.0     0.0     1.0  |
            //  --                     --
            //
            // Then we can use normal matrix multiplication to produce the
            // composed transformation.  I don't actually reform the matrix
            // explicitly which is why the following may seem kind of spagettish.

            gtwrk[1] =
            padfGT2[1] * padfGT1[1]
            + padfGT2[2] * padfGT1[4];
            gtwrk[2] =
                padfGT2[1] * padfGT1[2]
                + padfGT2[2] * padfGT1[5];
            gtwrk[0] =
                padfGT2[1] * padfGT1[0]
                + padfGT2[2] * padfGT1[3]
                + padfGT2[0] * 1.0;

            gtwrk[4] =
                padfGT2[4] * padfGT1[1]
                + padfGT2[5] * padfGT1[4];
            gtwrk[5] =
                padfGT2[4] * padfGT1[2]
                + padfGT2[5] * padfGT1[5];
            gtwrk[3] =
                padfGT2[4] * padfGT1[0]
                + padfGT2[5] * padfGT1[3]
                + padfGT2[3] * 1.0;
            return gtwrk;
        }

        /// <summary>
        /// 读取栅格块为字节数组（自动拉伸）
        /// </summary>
        /// <param name="band"></param>
        /// <param name="xOffset"></param>
        /// <param name="yOffset"></param>
        /// <param name="xSize"></param>
        /// <param name="ySize"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static byte[] ReadBand(this Band band, int xOffset, int yOffset, int xSize, int ySize, int width, int height)
        {
            byte[] buffer = null;
            if (band == null || width == 0 || height == 0)
            {
                return buffer;
            }
            int length = width * height;
            DataType dataType = band.DataType;
            IntPtr bufferPtr;
            // Percentage truncation
            double minPercent = 0.5;
            double maxPercent = 0.5;
            band.GetMaximum(out double maxValue, out int hasvalue);
            band.GetMinimum(out double minValue, out hasvalue);
            double dValue = maxValue - minValue;
            double highValue = maxValue - dValue * maxPercent / 100;
            double lowValue = minValue + dValue * minPercent / 100;
            double factor = 255 / (highValue - lowValue); // 系数
            CPLErr err = CPLErr.CE_None;
            lock (_lockObj)
            {
                switch (dataType)
                {
                    case DataType.GDT_Unknown:
                        throw new Exception("Unknown datatype");
                    case DataType.GDT_Byte:
                        {
                            buffer = new byte[length];
                            bufferPtr = GCHandleHelper.GetIntPtr(buffer);
                            err = band.ReadRaster(xOffset, yOffset, xSize, ySize, bufferPtr, width, height, dataType, 0, 0);
                            //for (int i = 0; i < length; i++)
                            //{
                            //    buffer[i] = buffer[i].StretchToByteValue(highValue, lowValue, factor);//做拉伸时才需要
                            //}
                        }
                        break;
                    case DataType.GDT_UInt16:
                        {
                            ushort[] tmpBuffer = new ushort[length];
                            bufferPtr = GCHandleHelper.GetIntPtr(tmpBuffer);
                            err = band.ReadRaster(xOffset, yOffset, xSize, ySize, bufferPtr, width, height, dataType, 0, 0);
                            buffer = new byte[length];
                            for (int i = 0; i < length; i++)
                            {
                                buffer[i] = tmpBuffer[i].StretchToByteValue(highValue, lowValue, factor);
                            }
                        }
                        break;
                    case DataType.GDT_Int16:
                        {
                            short[] tmpBuffer = new short[length];
                            bufferPtr = GCHandleHelper.GetIntPtr(tmpBuffer);
                            err = band.ReadRaster(xOffset, yOffset, xSize, ySize, bufferPtr, width, height, dataType, 0, 0);
                            buffer = new byte[length];
                            for (int i = 0; i < length; i++)
                            {
                                buffer[i] = tmpBuffer[i].StretchToByteValue(highValue, lowValue, factor);
                            }
                        }
                        break;
                    case DataType.GDT_UInt32:
                        {
                            uint[] tmpBuffer = new uint[length];
                            bufferPtr = GCHandleHelper.GetIntPtr(tmpBuffer);
                            err = band.ReadRaster(xOffset, yOffset, xSize, ySize, bufferPtr, width, height, dataType, 0, 0);
                            buffer = new byte[length];
                            for (int i = 0; i < length; i++)
                            {
                                buffer[i] = tmpBuffer[i].StretchToByteValue(highValue, lowValue, factor);
                            }
                        }
                        break;
                    case DataType.GDT_Int32:
                        {
                            int[] tmpBuffer = new int[length];
                            bufferPtr = GCHandleHelper.GetIntPtr(tmpBuffer);
                            err = band.ReadRaster(xOffset, yOffset, xSize, ySize, bufferPtr, width, height, dataType, 0, 0);
                            buffer = new byte[length];
                            for (int i = 0; i < length; i++)
                            {
                                buffer[i] = tmpBuffer[i].StretchToByteValue(highValue, lowValue, factor);
                            }
                        }
                        break;
                    case DataType.GDT_Float32:
                        {
                            float[] tmpBuffer = new float[length];
                            bufferPtr = GCHandleHelper.GetIntPtr(tmpBuffer);
                            err = band.ReadRaster(xOffset, yOffset, xSize, ySize, bufferPtr, width, height, dataType, 0, 0);
                            buffer = new byte[length];
                            for (int i = 0; i < length; i++)
                            {
                                buffer[i] = tmpBuffer[i].StretchToByteValue(highValue, lowValue, factor);
                            }
                        }
                        break;
                    case DataType.GDT_Float64:
                        {
                            double[] tmpBuffer = new double[length];
                            bufferPtr = GCHandleHelper.GetIntPtr(tmpBuffer);
                            err = band.ReadRaster(xOffset, yOffset, xSize, ySize, bufferPtr, width, height, dataType, 0, 0);
                            buffer = new byte[length];
                            for (int i = 0; i < length; i++)
                            {
                                buffer[i] = tmpBuffer[i].StretchToByteValue(highValue, lowValue, factor);
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
            }
            return buffer;
        }

        /// <summary>
        /// 根据数据类型查找对应类型
        /// </summary>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public static Type ToType(this DataType dataType)
        {
            Type type = null;
            switch (dataType)
            {
                case DataType.GDT_Byte:
                    type = typeof(byte);
                    break;
                case DataType.GDT_UInt16:
                    type = typeof(ushort);
                    break;
                case DataType.GDT_Int16:
                    type = typeof(short);
                    break;
                case DataType.GDT_UInt32:
                    type = typeof(uint);
                    break;
                case DataType.GDT_Int32:
                    type = typeof(int);
                    break;
                case DataType.GDT_Float32:
                    type = typeof(float);
                    break;
                case DataType.GDT_Float64:
                    type = typeof(double);
                    break;
            }
            return type;
        }
        /// <summary>
        /// 类型转数据类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static DataType ToDataType(this Type type)
        {
            DataType dataType = DataType.GDT_Unknown;
            if (type == null)
            {
                return dataType;
            }
            if (type == typeof(byte)) dataType = DataType.GDT_Byte;
            else if (type == typeof(ushort)) dataType = DataType.GDT_UInt16;
            else if (type == typeof(short)) dataType = DataType.GDT_Int16;
            else if (type == typeof(uint)) dataType = DataType.GDT_UInt32;
            else if (type == typeof(int)) dataType = DataType.GDT_Int32;
            else if (type == typeof(float)) dataType = DataType.GDT_Float32;
            else if (type == typeof(double)) dataType = DataType.GDT_Float64;
            return dataType;
        }


        public static void NormalizeSizeToBand(int rasterXSize, int rasterYSize, int xOffset, int yOffset, int xSize, int ySize, out int width, out int height)
        {
            width = xSize;
            height = ySize;

            if (xOffset + width > rasterXSize)
            {
                width = rasterXSize - xOffset;
            }

            if (yOffset + height > rasterYSize)
            {
                height = rasterYSize - yOffset;
            }
        }

        /// <summary>
        /// 根据像素值获取图片
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="rBuffer"></param>
        /// <param name="gBuffer"></param>
        /// <param name="bBuffer"></param>
        /// <param name="aBuffer"></param>
        /// <param name="noDataValue"></param>
        /// <returns></returns>
        public static unsafe Bitmap GetBitmap(int width, int height, byte[] rBuffer, byte[] gBuffer, byte[] bBuffer, byte[] aBuffer = null, double? noDataValue = null)
        {
            Bitmap result = null;
            int bufferLength = width * height;
            if (width <= 0 || height <= 0 || rBuffer == null || rBuffer.Length != bufferLength || gBuffer == null || gBuffer.Length != bufferLength || bBuffer == null || bBuffer.Length != bufferLength)
            {
                return null;
            }
            PixelFormat pixelFormat;
            int bytesPerPixel;
            if (aBuffer == null)
            {
                if (noDataValue.HasValue && noDataValue.Value >= byte.MinValue && noDataValue.Value <= byte.MaxValue)
                {
                    pixelFormat = PixelFormat.Format32bppArgb;
                    bytesPerPixel = 4;
                }
                else
                {
                    pixelFormat = PixelFormat.Format24bppRgb;
                    bytesPerPixel = 3;
                }
            }
            else
            {
                pixelFormat = PixelFormat.Format32bppArgb;
                bytesPerPixel = 4;
            }
            result = new Bitmap(width, height, pixelFormat);
            BitmapData bData = result.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, pixelFormat);
            byte* scan0 = (byte*)bData.Scan0;
            int stride = bData.Stride;
            int dWidth = stride - width * bytesPerPixel;
            int ptrIndex = 0;
            int bufferIndex = 0;
            if (aBuffer == null)
            {
                if (noDataValue.HasValue && noDataValue.Value >= byte.MinValue && noDataValue.Value <= byte.MaxValue)
                {
                    for (int row = 0; row < height; row++)
                    {
                        for (int col = 0; col < width; col++)
                        {
                            scan0[ptrIndex] = bBuffer[bufferIndex];
                            scan0[ptrIndex + 1] = gBuffer[bufferIndex];
                            scan0[ptrIndex + 2] = rBuffer[bufferIndex];
                            if (rBuffer[bufferIndex] == noDataValue.Value || gBuffer[bufferIndex] == noDataValue.Value || bBuffer[bufferIndex] == noDataValue.Value)
                            {
                                scan0[ptrIndex + 3] = 0;
                            }
                            else
                            {
                                scan0[ptrIndex + 3] = 255;
                            }
                            ptrIndex += bytesPerPixel;
                            bufferIndex++;
                        }
                        ptrIndex += dWidth;
                    }
                }
                else
                {
                    for (int row = 0; row < height; row++)
                    {
                        for (int col = 0; col < width; col++)
                        {
                            scan0[ptrIndex] = bBuffer[bufferIndex];
                            scan0[ptrIndex + 1] = gBuffer[bufferIndex];
                            scan0[ptrIndex + 2] = rBuffer[bufferIndex];
                            ptrIndex += bytesPerPixel;
                            bufferIndex++;
                        }
                        ptrIndex += dWidth;
                    }
                }
            }
            else
            {
                if (noDataValue.HasValue && noDataValue.Value >= byte.MinValue && noDataValue.Value <= byte.MaxValue)
                {
                    for (int row = 0; row < height; row++)
                    {
                        for (int col = 0; col < width; col++)
                        {
                            scan0[ptrIndex] = bBuffer[bufferIndex];
                            scan0[ptrIndex + 1] = gBuffer[bufferIndex];
                            scan0[ptrIndex + 2] = rBuffer[bufferIndex];
                            if (rBuffer[bufferIndex] == noDataValue.Value || gBuffer[bufferIndex] == noDataValue.Value || bBuffer[bufferIndex] == noDataValue.Value)
                            {
                                scan0[ptrIndex + 3] = 0;
                            }
                            else
                            {
                                scan0[ptrIndex + 3] = aBuffer[bufferIndex];
                            }
                            ptrIndex += bytesPerPixel;
                            bufferIndex++;
                        }
                        ptrIndex += dWidth;
                    }
                }
                else
                {
                    for (int row = 0; row < height; row++)
                    {
                        for (int col = 0; col < width; col++)
                        {
                            scan0[ptrIndex] = bBuffer[bufferIndex];
                            scan0[ptrIndex + 1] = gBuffer[bufferIndex];
                            scan0[ptrIndex + 2] = rBuffer[bufferIndex];
                            scan0[ptrIndex + 3] = aBuffer[bufferIndex];
                            ptrIndex += bytesPerPixel;
                            bufferIndex++;
                        }
                        ptrIndex += dWidth;
                    }
                }
            }
            result.UnlockBits(bData);
            return result;
        }

        /// <summary>
        /// 获取灰度图
        /// </summary>
        /// <param name="band"></param>
        /// <param name="xOffset"></param>
        /// <param name="yOffset"></param>
        /// <param name="xSize"></param>
        /// <param name="ySize"></param>
        /// <param name="noDataValue"></param>
        /// <returns></returns>
        public static Bitmap GetGrayBitmap(this Band band, int xOffset, int yOffset, int xSize, int ySize, double noDataValue)
        {
            Bitmap result = null;
            if (band != null)
            {
                NormalizeSizeToBand(band.XSize, band.YSize, xOffset, yOffset, xSize, ySize, out int width, out int height);
                byte[] rBuffer = band.ReadBand(xOffset, yOffset, width, height, width, height);
                result = GetBitmap(width, height, rBuffer, rBuffer, rBuffer, noDataValue: noDataValue);
            }
            return result;
        }

        /// <summary>
        /// 获取彩色图
        /// </summary>
        /// <param name="rBand"></param>
        /// <param name="gBand"></param>
        /// <param name="bBand"></param>
        /// <param name="xOffset"></param>
        /// <param name="yOffset"></param>
        /// <param name="xSize"></param>
        /// <param name="ySize"></param>
        /// <param name="noDataValue"></param>
        /// <returns></returns>
        public static Bitmap GetRgbBitmap(Band rBand, Band gBand, Band bBand, int xOffset, int yOffset, int xSize, int ySize, double noDataValue)
        {
            Bitmap result = null;
            if (rBand != null && gBand != null && bBand != null)
            {
                NormalizeSizeToBand(rBand.XSize, rBand.YSize, xOffset, yOffset, xSize, ySize, out int width, out int height);
                byte[] rBuffer = rBand.ReadBand(xOffset, yOffset, width, height, width, height);
                byte[] gBuffer = gBand.ReadBand(xOffset, yOffset, width, height, width, height);
                byte[] bBuffer = bBand.ReadBand(xOffset, yOffset, width, height, width, height);
                result = GetBitmap(width, height, rBuffer, gBuffer, bBuffer, noDataValue: noDataValue);
            }
            return result;
        }

        /// <summary>
        /// 获取透明的彩色图
        /// </summary>
        /// <param name="rBand"></param>
        /// <param name="gBand"></param>
        /// <param name="bBand"></param>
        /// <param name="aBand"></param>
        /// <param name="xOffset"></param>
        /// <param name="yOffset"></param>
        /// <param name="xSize"></param>
        /// <param name="ySize"></param>
        /// <param name="noDataValue"></param>
        /// <returns></returns>
        public static Bitmap GetRgbaBitmap(Band rBand, Band gBand, Band bBand, Band aBand, int xOffset, int yOffset, int xSize, int ySize, double noDataValue)
        {
            Bitmap result = null;
            if (rBand != null && gBand != null && bBand != null && aBand != null)
            {
                NormalizeSizeToBand(rBand.XSize, rBand.YSize, xOffset, yOffset, xSize, ySize, out int width, out int height);
                byte[] rBuffer = rBand.ReadBand(xOffset, yOffset, width, height, width, height);
                byte[] gBuffer = gBand.ReadBand(xOffset, yOffset, width, height, width, height);
                byte[] bBuffer = bBand.ReadBand(xOffset, yOffset, width, height, width, height);
                byte[] aBuffer = aBand.ReadBand(xOffset, yOffset, width, height, width, height);
                result = GetBitmap(width, height, rBuffer, gBuffer, bBuffer, aBuffer, noDataValue);
            }
            return result;
        }

        /// <summary>
        /// 获取调色板图
        /// </summary>
        /// <param name="band"></param>
        /// <param name="xOffset"></param>
        /// <param name="yOffset"></param>
        /// <param name="xSize"></param>
        /// <param name="ySize"></param>
        /// <param name="noDataValue"></param>
        /// <returns></returns>
        public static Bitmap GetPaletteBitmap(this Band band, int xOffset, int yOffset, int xSize, int ySize, double noDataValue)
        {
            Bitmap result = null;
            if (band == null)
            {
                return result;
            }
            ColorTable ct = band.GetRasterColorTable();
            if (ct == null)
            {
                throw new GdalException("Image was stored with a palette interpretation but has no color table.");
            }

            if (ct.GetPaletteInterpretation() != PaletteInterp.GPI_RGB)
            {
                throw new GdalException("Only RGB palette interpretation is currently supported by this " + " plug-in, " + ct.GetPaletteInterpretation() + " is not supported.");
            }

            int count = ct.GetCount();
            byte[][] colorTable = new byte[ct.GetCount()][];
            for (int i = 0; i < count; i++)
            {
                using (ColorEntry ce = ct.GetColorEntry(i))
                {
                    colorTable[i] = new[] { (byte)ce.c4, (byte)ce.c1, (byte)ce.c2, (byte)ce.c3 };
                }
            }
            ct.Dispose();

            NormalizeSizeToBand(band.XSize, band.YSize, xOffset, yOffset, xSize, ySize, out int width, out int height);
            byte[] indexBuffer = band.ReadBand(xOffset, yOffset, width, height, width, height);
            byte[] rBuffer = new byte[indexBuffer.Length];
            byte[] gBuffer = new byte[indexBuffer.Length];
            byte[] bBuffer = new byte[indexBuffer.Length];
            byte[] aBuffer = new byte[indexBuffer.Length];
            for (int i = 0; i < indexBuffer.Length; i++)
            {
                int index = indexBuffer[i];
                aBuffer[i] = colorTable[index][0];
                rBuffer[i] = colorTable[index][1];
                gBuffer[i] = colorTable[index][2];
                bBuffer[i] = colorTable[index][3];
            }
            result = GetBitmap(width, height, rBuffer, gBuffer, gBuffer, aBuffer, noDataValue);
            return result;
        }
        public static Dataset ToMemDataset(this Image image, string memPath = "/vsimem/inmemfile")
        {
            Dataset dataset = null;
            if (image == null)
            {
                throw new Exception("image不能为空");
            }
            if (memPath?.StartsWith("/vsimem/") != true)
            {
                throw new Exception("memPath必须以“/vsimem/”开头");
            }
            byte[] buffer = null;
            using (MemoryStream ms = new MemoryStream())
            {
                var format = image.RawFormat.ToString();
                if (format.Contains("[ImageFormat:"))
                {
                    throw new Exception("image格式不正确");
                }
                image.Save(ms, image.RawFormat);
                buffer = ms.GetBuffer();
            }
            Gdal.FileFromMemBuffer(memPath, buffer);
            dataset = Gdal.Open(memPath, Access.GA_ReadOnly);
            return dataset;
        }

        /// <summary>
        /// 按照BGR(A)顺序读取栅格并返回位图的字节数组
        /// </summary>
        /// <param name="dataset">栅格数据集</param>
        /// <param name="xOffset">X偏移</param>
        /// <param name="yOffset">Y偏移</param>
        /// <param name="xSize">X宽度</param>
        /// <param name="ySize">Y宽度</param>
        /// <param name="width">位图长度</param>
        /// <param name="height">位图宽度</param>
        /// <param name="bandCount">波段数</param>
        /// <param name="bandMap">波段映射</param>
        /// <param name="pixelSpace">像素间隔</param>
        /// <param name="lineSpace">行间隔</param>
        /// <param name="bandSpace">波段间隔</param>
        /// <param name="readABand">是否读取透明波段</param>
        /// <returns>位图的字节数组</returns>
        public static byte[] ReadBmpBytes(this Dataset dataset, int xOffset, int yOffset, int xSize, int ySize, int width, int height, int bandCount, int[] bandMap, int pixelSpace, int lineSpace, int bandSpace, bool readABand = false)
        {
            byte[] buffer = null;
            if (dataset == null || xOffset < 0 || yOffset < 0 || xSize <= 0 || ySize <= 0 || width <= 0 || height <= 0 || dataset.RasterXSize < (xOffset + xSize) || dataset.RasterYSize < (yOffset + ySize))
            {
                return buffer;
            }
            int destBandCount;
            if (readABand)
            {
                destBandCount = 4;
            }
            else
            {
                destBandCount = 3;
            }
            if (dataset.RasterCount < destBandCount || bandCount != destBandCount || bandMap?.Length != destBandCount)
            {
                return buffer;
            }
            int length = width * height * bandCount;
            DataType dataType;
            double maxValue, minValue;
            using (var band = dataset.GetRasterBand(1))
            {
                dataType = band.DataType;
                band.GetMaximum(out maxValue, out int hasvalue);
                band.GetMinimum(out minValue, out hasvalue);
            }
            IntPtr bufferPtr;
            // Percentage truncation
            double minPercent = 0.5;
            double maxPercent = 0.5;
            double dValue = maxValue - minValue;
            double highValue = maxValue - dValue * maxPercent / 100;
            double lowValue = minValue + dValue * minPercent / 100;
            double factor = 255 / (highValue - lowValue); // 系数
            CPLErr err = CPLErr.CE_None;
            lock (_lockObj)
            {
                switch (dataType)
                {
                    case DataType.GDT_Unknown:
                        throw new Exception("Unknown datatype");
                    case DataType.GDT_Byte:
                        {
                            buffer = new byte[length];
                            bufferPtr = GCHandleHelper.GetIntPtr(buffer);
                            err = dataset.ReadRaster(xOffset, yOffset, xSize, ySize, bufferPtr, width, height, dataType, bandCount, bandMap, pixelSpace, lineSpace, bandSpace);
                            //for (int i = 0; i < length; i++)
                            //{
                            //    buffer[i] = buffer[i].StretchToByteValue(highValue, lowValue, factor);//做拉伸时才需要
                            //}
                        }
                        break;
                    case DataType.GDT_UInt16:
                        {
                            ushort[] tmpBuffer = new ushort[length];
                            bufferPtr = GCHandleHelper.GetIntPtr(tmpBuffer);
                            err = dataset.ReadRaster(xOffset, yOffset, xSize, ySize, bufferPtr, width, height, dataType, bandCount, bandMap, pixelSpace, lineSpace, bandSpace);
                            buffer = new byte[length];
                            for (int i = 0; i < length; i++)
                            {
                                buffer[i] = tmpBuffer[i].StretchToByteValue(highValue, lowValue, factor);
                            }
                        }
                        break;
                    case DataType.GDT_Int16:
                        {
                            short[] tmpBuffer = new short[length];
                            bufferPtr = GCHandleHelper.GetIntPtr(tmpBuffer);
                            err = dataset.ReadRaster(xOffset, yOffset, xSize, ySize, bufferPtr, width, height, dataType, bandCount, bandMap, pixelSpace, lineSpace, bandSpace);
                            buffer = new byte[length];
                            for (int i = 0; i < length; i++)
                            {
                                buffer[i] = tmpBuffer[i].StretchToByteValue(highValue, lowValue, factor);
                            }
                        }
                        break;
                    case DataType.GDT_UInt32:
                        {
                            uint[] tmpBuffer = new uint[length];
                            bufferPtr = GCHandleHelper.GetIntPtr(tmpBuffer);
                            err = dataset.ReadRaster(xOffset, yOffset, xSize, ySize, bufferPtr, width, height, dataType, bandCount, bandMap, pixelSpace, lineSpace, bandSpace);
                            buffer = new byte[length];
                            for (int i = 0; i < length; i++)
                            {
                                buffer[i] = tmpBuffer[i].StretchToByteValue(highValue, lowValue, factor);
                            }
                        }
                        break;
                    case DataType.GDT_Int32:
                        {
                            int[] tmpBuffer = new int[length];
                            bufferPtr = GCHandleHelper.GetIntPtr(tmpBuffer);
                            err = dataset.ReadRaster(xOffset, yOffset, xSize, ySize, bufferPtr, width, height, dataType, bandCount, bandMap, pixelSpace, lineSpace, bandSpace);
                            buffer = new byte[length];
                            for (int i = 0; i < length; i++)
                            {
                                buffer[i] = tmpBuffer[i].StretchToByteValue(highValue, lowValue, factor);
                            }
                        }
                        break;
                    case DataType.GDT_Float32:
                        {
                            float[] tmpBuffer = new float[length];
                            bufferPtr = GCHandleHelper.GetIntPtr(tmpBuffer);
                            err = dataset.ReadRaster(xOffset, yOffset, xSize, ySize, bufferPtr, width, height, dataType, bandCount, bandMap, pixelSpace, lineSpace, bandSpace);
                            buffer = new byte[length];
                            for (int i = 0; i < length; i++)
                            {
                                buffer[i] = tmpBuffer[i].StretchToByteValue(highValue, lowValue, factor);
                            }
                        }
                        break;
                    case DataType.GDT_Float64:
                        {
                            double[] tmpBuffer = new double[length];
                            bufferPtr = GCHandleHelper.GetIntPtr(tmpBuffer);
                            err = dataset.ReadRaster(xOffset, yOffset, xSize, ySize, bufferPtr, width, height, dataType, bandCount, bandMap, pixelSpace, lineSpace, bandSpace);
                            buffer = new byte[length];
                            for (int i = 0; i < length; i++)
                            {
                                buffer[i] = tmpBuffer[i].StretchToByteValue(highValue, lowValue, factor);
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
            }
            return buffer;
        }

        /// <summary>
        /// 计算读取块大小
        /// </summary>
        /// <param name="band">波段</param>
        /// <param name="blockXsize">块宽度</param>
        /// <param name="blockYsize">块高度</param>
        public static void ComputeBlockSize(this Band band, out int blockXsize, out int blockYsize)
        {
            if (band == null)
            {
                blockXsize = 0;
                blockYsize = 0;
            }
            else
            {
                int minSize = 1024;
                int maxSize = 4096;
                band.GetBlockSize(out blockXsize, out blockYsize);
                if (blockXsize > maxSize)
                {
                    blockXsize = Math.Min(maxSize, blockXsize);
                }
                else if (blockXsize < minSize)
                {
                    blockXsize = Math.Min(minSize, band.XSize);
                }
                if (blockYsize > maxSize)
                {
                    blockYsize = Math.Min(maxSize, blockYsize);
                }
                else if (blockYsize < minSize)
                {
                    blockYsize = Math.Min(minSize, band.YSize);
                }
            }
        }

        private static unsafe void BufferToScan0(BitmapData bData, int width, int height, int bytesPerPixel, byte[] bmpBuffer)
        {
            byte* scan0 = (byte*)bData.Scan0;
            int stride = bData.Stride;
            int dWidth = stride - width * bytesPerPixel;
            int bufferIndex = 0;
            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    for (int i = 0; i < bytesPerPixel; i++)
                    {
                        scan0[i] = bmpBuffer[bufferIndex];
                        bufferIndex++;
                        scan0++;
                    }
                }
                scan0 += dWidth;
            }
        }
        private static unsafe void BufferToScan0(BitmapData bData, int width, int height, int bytesPerPixel, byte[] bmpBuffer, double noDataValue)
        {
            byte* scan0 = (byte*)bData.Scan0;
            int stride = bData.Stride;
            int dWidth = stride - width * bytesPerPixel;
            int bufferIndex = 0;
            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    scan0[0] = bmpBuffer[bufferIndex];
                    scan0[1] = bmpBuffer[bufferIndex + 1];
                    scan0[2] = bmpBuffer[bufferIndex + 2];
                    if (scan0[0] == noDataValue && scan0[1] == noDataValue && scan0[2] == noDataValue)
                    {
                        scan0[3] = 0;
                    }
                    else
                    {
                        //scan0[3] = bmpBuffer[bufferIndex + 3];
                        scan0[3] = 255;
                    }
                    scan0 += bytesPerPixel;
                    bufferIndex += 3;
                }
                scan0 += dWidth;
            }
        }


    }
}
