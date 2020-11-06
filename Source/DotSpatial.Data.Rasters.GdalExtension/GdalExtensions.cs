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

        /// <summary>
        /// 根据波段值获取图片
        /// </summary>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="rBuffer">红色波段</param>
        /// <param name="gBuffer">绿色波段</param>
        /// <param name="bBuffer">蓝色波段</param>
        /// <param name="aBuffer">透明波段</param>
        /// <param name="noDataValue">无数据值</param>
        /// <returns>图片</returns>
        public static unsafe Bitmap GetBitmap(int width, int height, byte[] rBuffer, byte[] gBuffer, byte[] bBuffer, byte[] aBuffer = null, double noDataValue = 256)
        {
            if (width <= 0 || height <= 0)
            {
                return null;
            }
            Bitmap result = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            BitmapData bData = result.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            byte* scan0 = (byte*)bData.Scan0;
            int stride = bData.Stride;
            int dWidth = stride - width * 4;
            int ptrIndex = -1;
            int bufferIndex = -1;
            if (aBuffer == null)
            {
                for (int row = 0; row < height; row++)
                {
                    ptrIndex = row * stride;
                    bufferIndex = row * width;
                    for (int col = 0; col < width; col++)
                    {
                        byte bValue = bBuffer[bufferIndex];
                        byte gValue = gBuffer[bufferIndex];
                        byte rValue = rBuffer[bufferIndex];
                        byte aValue = 255;
                        if (rValue == noDataValue || gValue == noDataValue || bValue == noDataValue)
                        {
                            aValue = 0;
                        }
                        scan0[ptrIndex] = bValue;
                        scan0[ptrIndex + 1] = gValue;
                        scan0[ptrIndex + 2] = rValue;
                        scan0[ptrIndex + 3] = aValue;
                        ptrIndex += 4;
                        bufferIndex++;
                    }
                }
            }
            else
            {
                for (int row = 0; row < height; row++)
                {
                    ptrIndex = row * stride;
                    bufferIndex = row * width;
                    for (int col = 0; col < width; col++)
                    {
                        byte bValue = bBuffer[bufferIndex];
                        byte gValue = gBuffer[bufferIndex];
                        byte rValue = rBuffer[bufferIndex];
                        byte aValue = aBuffer[bufferIndex];
                        if (rValue == noDataValue && gValue == noDataValue && bValue == noDataValue)
                        {
                            aValue = 0;
                        }
                        scan0[ptrIndex] = bValue;
                        scan0[ptrIndex + 1] = gValue;
                        scan0[ptrIndex + 2] = rValue;
                        scan0[ptrIndex + 3] = aValue;
                        ptrIndex += 4;
                        bufferIndex++;
                    }
                }
            }
            result.UnlockBits(bData);
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
    }
}
