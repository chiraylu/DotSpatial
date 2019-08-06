// Copyright (c) DotSpatial Team. All rights reserved.
// Licensed under the MIT license. See License.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using GeoAPI.Geometries;

namespace DotSpatial.Data
{
    /// <summary>
    /// This class is strictly the vector access code. This does not handle
    /// the attributes, which must be handled independently.
    /// </summary>
    public class PointShapefileFeatureSource : ShapefileFeatureSource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PointShapefileFeatureSource"/> class from the specified file.
        /// </summary>
        /// <param name="fileName">The fileName to work with.</param>
        public PointShapefileFeatureSource(string fileName)
            : base(fileName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PointShapefileFeatureSource"/> class from the specified file and builds spatial index if requested.
        /// </summary>
        /// <param name="fileName">The fileName to work with.</param>
        /// <param name="useSpatialIndexing">Indicates whether the spatial index should be build.</param>
        /// <param name="trackDeletedRows">Indicates whether deleted records should be tracked.</param>
        public PointShapefileFeatureSource(string fileName, bool useSpatialIndexing, bool trackDeletedRows)
            : base(fileName, useSpatialIndexing, trackDeletedRows)
        {
        }

        /// <inheritdoc />
        public override FeatureType FeatureType => FeatureType.Point;

        /// <inheritdoc />
        public override ShapeType ShapeType => ShapeType.Point;

        /// <inheritdoc />
        public override ShapeType ShapeTypeM => ShapeType.PointM;

        /// <inheritdoc />
        public override ShapeType ShapeTypeZ => ShapeType.PointZ;

        /// <inheritdoc />
        public override IShapeSource CreateShapeSource()
        {
            return new PointShapefileShapeSource(Filename, Quadtree, null);
        }

        /// <inheritdoc />
        public override void UpdateExtents()
        {
            UpdateExtents(new PointShapefileShapeSource(Filename));
        }

        /// <inheritdoc/>
        public override IFeatureSet Select(string filterExpression, Envelope envelope, ref int startIndex, int maxCount)
        {
            return Select(new PointShapefileShapeSource(Filename, Quadtree, null), filterExpression, envelope, ref startIndex, maxCount);
        }

        /// <inheritdoc/>
        public override void SearchAndModifyAttributes(Envelope envelope, int chunkSize, FeatureSourceRowEditEvent rowCallback)
        {
            SearchAndModifyAttributes(new PointShapefileShapeSource(Filename, Quadtree, null), envelope, chunkSize, rowCallback);
        }

        /// <inheritdoc />
        protected override void AppendGeometry(ShapefileHeader header, IGeometry feature, int numFeatures)
        {
            var fi = new FileInfo(Filename);
            int offset = Convert.ToInt32(fi.Length / 2);

            var shpStream = new FileStream(Filename, FileMode.Append, FileAccess.Write, FileShare.None, 10000);
            var shxStream = new FileStream(header.ShxFilename, FileMode.Append, FileAccess.Write, FileShare.None, 100);

            Coordinate point = feature.Coordinates[0];
            int contentLength = 10;
            if (header.ShapeType == ShapeType.PointM)
            {
                contentLength += 4; // one additional value (m)
            }

            if (header.ShapeType == ShapeType.PointZ)
            {
                contentLength += 8; // 2 additional values (m, z)
            }

            ////                                            Index File
            //                                              ---------------------------------------------------------
            //                                              Position     Value               Type        Number      Byte Order
            //                                              ---------------------------------------------------------
            shxStream.WriteBe(offset);                      // Byte 0     Offset             Integer     1           Big
            shxStream.WriteBe(contentLength);               // Byte 4    Content Length      Integer     1           Big
            shxStream.Flush();
            shxStream.Close();
            ////                                            X Y Points
            //                                              ---------------------------------------------------------
            //                                              Position     Value               Type        Number      Byte Order
            //                                              ---------------------------------------------------------
            shpStream.WriteBe(numFeatures);                 // Byte 0       Record Number       Integer     1           Big
            shpStream.WriteBe(contentLength);               // Byte 4       Content Length      Integer     1           Big
            shpStream.WriteLe((int)header.ShapeType);       // Byte 8       Shape Type 3        Integer     1           Little
            if (header.ShapeType == ShapeType.NullShape)
            {
                return;
            }

            shpStream.WriteLe(point.X);                     // Byte 12      X                   Double      1           Little
            shpStream.WriteLe(point.Y);                     // Byte 20      Y                   Double      1           Little

            if (header.ShapeType == ShapeType.PointM)
            {
                shpStream.WriteLe(point.M);                 // Byte 28      M                   Double      1           Little
            }
            else if (header.ShapeType == ShapeType.PointZ)
            {
                shpStream.WriteLe(point.Z);                 // Byte 28      Z                   Double      1           Little
                shpStream.WriteLe(point.M);                 // Byte 36      M                   Double      1           Little
            }

            shpStream.Flush();
            shpStream.Close();
            offset += contentLength;
            Shapefile.WriteFileLength(Filename, offset + 4); // Add 4 for the record header
            Shapefile.WriteFileLength(header.ShxFilename, 50 + numFeatures * 4);
        }
        private int GetContentLength(ShapeType shapeType)
        {
            int contentLength = 10;
            if (shapeType == ShapeType.PointM)
            {
                contentLength += 4; // one additional value (m)
            }
            else if (shapeType == ShapeType.PointZ)
            {
                contentLength += 8; // 2 additional values (m, z)
            }
            return contentLength;
        }

        private  void EditGeometryContent(FileStream tmpShpStream,  ShapeType shapeType, IGeometry geometry)
        {
            Coordinate point = geometry.Coordinates[0];
            ////                                            X Y Points
            //                                              ---------------------------------------------------------
            //                                              Position     Value               Type        Number      Byte Order
            //                                              ---------------------------------------------------------
            tmpShpStream.WriteLe(point.X);                  // Byte 12      X                   Double      1           Little
            tmpShpStream.WriteLe(point.Y);                  // Byte 20      Y                   Double      1           Little

            if (shapeType == ShapeType.PointM)
            {
                tmpShpStream.WriteLe(point.M);              // Byte 28      M                   Double      1           Little
            }
            else if (shapeType == ShapeType.PointZ)
            {
                tmpShpStream.WriteLe(point.Z);              // Byte 28      Z                   Double      1           Little
                tmpShpStream.WriteLe(point.M);              // Byte 36      M                   Double      1           Little
            }
        }

        protected override void EditGeometry(ShapefileHeader header, int fid, IGeometry geometry)
        {
            var shapeHeaders = ReadIndexFile(header.ShxFilename);
            if (fid < shapeHeaders.Count)
            {
                var tmpShpPath = Path.GetTempFileName();
                FileStream tmpShpStream = new FileStream(tmpShpPath, FileMode.Create, FileAccess.ReadWrite);
                FileStream shpStream = new FileStream(Filename, FileMode.Open, FileAccess.ReadWrite, FileShare.Read, 10000);
                FileStream shxStream = new FileStream(header.ShxFilename, FileMode.Open, FileAccess.Write, FileShare.Read, 100);

                int shpOffset = shapeHeaders[fid].Offset;
                long shpByteOffset = shapeHeaders[fid].ByteOffset;
                CopyTo(shpStream, tmpShpStream, 0, shpByteOffset);
                tmpShpStream.Seek(shpByteOffset, SeekOrigin.Begin);
                shxStream.Seek(100 + fid * 8, SeekOrigin.Begin);
                int recordNumber = fid + 1;

                int contentLength = GetContentLength(header.ShapeType);

                ////                                            Index File
                //                                              ---------------------------------------------------------
                //                                              Position     Value               Type        Number      Byte Order
                //                                              ---------------------------------------------------------
                shxStream.WriteBe(shpOffset);                   // Byte 0     Offset             Integer     1           Big
                shxStream.WriteBe(contentLength);               // Byte 4    Content Length      Integer     1           Big

                ////                                            X Y Points
                //                                              ---------------------------------------------------------
                //                                              Position     Value               Type        Number      Byte Order
                //                                              ---------------------------------------------------------
                tmpShpStream.WriteBe(recordNumber);             // Byte 0       Record Number       Integer     1           Big
                tmpShpStream.WriteBe(contentLength);            // Byte 4       Content Length      Integer     1           Big
                tmpShpStream.WriteLe((int)header.ShapeType);    // Byte 8       Shape Type 3        Integer     1           Little
                if (header.ShapeType != ShapeType.NullShape)
                {
                    EditGeometryContent(tmpShpStream, header.ShapeType, geometry);
                }

                int dOffset = contentLength - shapeHeaders[fid].ContentLength;
                if (dOffset != 0)
                {
                    for (int i = fid + 1; i < shapeHeaders.Count; i++)
                    {
                        shxStream.Seek(100 + i * 8, SeekOrigin.Begin);
                        shxStream.WriteBe(shapeHeaders[i].Offset + dOffset);
                    }
                }
                shxStream.Flush();
                shxStream.Dispose();

                if (fid < shapeHeaders.Count - 1)
                {
                    ShapeHeader nextShapeHeader = shapeHeaders[fid + 1];
                    long afterByteOffset = nextShapeHeader.ByteOffset;
                    ShapeHeader lastShapeHeader = shapeHeaders.LastOrDefault();
                    long afterCount = lastShapeHeader.ByteOffset - nextShapeHeader.ByteOffset + 8 + lastShapeHeader.ByteLength;
                    CopyTo(shpStream, tmpShpStream, afterByteOffset, afterCount);
                }

                shpStream.Seek(0, SeekOrigin.Begin);
                tmpShpStream.Seek(0, SeekOrigin.Begin);
                CopyTo(tmpShpStream, shpStream, 0, tmpShpStream.Length);
                shpStream.SetLength(tmpShpStream.Length);

                shpStream.Flush();
                shpStream.Dispose();
                int offset = Convert.ToInt32(tmpShpStream.Length / 2);
                Shapefile.WriteFileLength(Filename, offset);
                int numFeatures = shapeHeaders.Count;
                Shapefile.WriteFileLength(header.ShxFilename, 50 + numFeatures * 4);
                tmpShpStream.Dispose();
                File.Delete(tmpShpPath);
            }
        }
    }
}