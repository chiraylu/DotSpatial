// Copyright (c) DotSpatial Team. All rights reserved.
// Licensed under the MIT license. See License.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DotSpatial.Controls;
using DotSpatial.Data;
using DotSpatial.Symbology;
using GeoAPI.Geometries;

namespace DotSpatial.Plugins.ShapeEditor
{
    /// <summary>
    /// This is an abtract class that provides functionality for snapping objects.
    /// </summary>
    public abstract class SnappableMapFunction : MapFunctionZoom
    {
        private Point _mousePosition;
        private Coordinate _mouseLocation;
        #region  Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SnappableMapFunction"/> class.
        /// </summary>
        /// <param name="map">The map.</param>
        protected SnappableMapFunction(IMap map)
            : base(map)
        {
            SnapMode = SnapMode.Point | SnapMode.End | SnapMode.Vertex | SnapMode.Edege;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value of Snap mode.
        /// </summary>
        public virtual SnapMode SnapMode { get; set; }

        /// <summary>
        /// Gets or sets SnapInfo
        /// </summary>
        protected SnapInfo SnapInfo { get; set; }

        /// <summary>
        /// Gets or sets a list of layers that will be snapped to.
        /// </summary>
        protected List<IFeatureLayer> SnapLayers { get; set; }

        /// <summary>
        /// Gets or sets the pen that will be used to draw the snapping circle.
        /// </summary>
        protected Pen SnapPen { get; set; } = new Pen(Color.HotPink, 2F);

        /// <summary>
        /// Gets or sets the snap tolerance. +/- N pixels around the mouse point.
        /// </summary>
        protected int SnapTol { get; set; } = 9;

        #endregion

        #region Methods

        /// <summary>
        /// Add the given layer to the snap list. This list determines which layers the current layer will be snapped to.
        /// </summary>
        /// <param name="layer">The layer that gets added to the list of layers that can be used for snapping.</param>
        public void AddLayerToSnap(IFeatureLayer layer)
        {
            if (SnapLayers == null)
                InitializeSnapLayers();
            if (!SnapLayers.Contains(layer))
            {
                SnapLayers.Add(layer);
            }
        }

        private Tuple<IFeature, Coordinate> ComputSnapPointModeFeature(IFeatureLayer layer, Extent extent)
        {
            Tuple<IFeature, Coordinate> tuple = null;
            if (SnapMode == SnapMode.None || layer == null || extent == null)
            {
                return tuple;
            }
            if ((SnapMode & SnapMode.Point) > 0 && (layer.DataSet.FeatureType == FeatureType.Point || layer.DataSet.FeatureType == FeatureType.MultiPoint))
            {
                var features = layer.DataSet.Select(extent);
                foreach (var feature in features)
                {
                    foreach (var coordinate in feature.Geometry.Coordinates)
                    {
                        if (extent.Contains(coordinate))
                        {
                            tuple = new Tuple<IFeature, Coordinate>(feature, coordinate);
                            break;
                        }
                    }
                    if (tuple != null)
                    {
                        break;
                    }
                }
            }
            return tuple;
        }

        private Tuple<IFeature, Coordinate> ComputSnapEndModeFeature(IFeatureLayer layer, Extent extent)
        {
            Tuple<IFeature, Coordinate> tuple = null;
            if (SnapMode == SnapMode.None || layer == null || extent == null)
            {
                return tuple;
            }
            if ((SnapMode & SnapMode.End) > 0 && (layer.DataSet.FeatureType == FeatureType.Line || layer.DataSet.FeatureType == FeatureType.Polygon))
            {
                var features = layer.DataSet.Select(extent);
                foreach (var feature in features)
                {
                    for (int i = 0; i < feature.Geometry.NumGeometries; i++)
                    {
                        var geoItem = feature.Geometry.GetGeometryN(i);
                        var firstCoord = geoItem.Coordinates.FirstOrDefault();
                        if (extent.Contains(firstCoord))
                        {
                            tuple = new Tuple<IFeature, Coordinate>(feature, firstCoord);
                        }
                        else
                        {
                            var lastCoord = geoItem.Coordinates.LastOrDefault();
                            if (extent.Contains(lastCoord))
                            {
                                tuple = new Tuple<IFeature, Coordinate>(feature, lastCoord);
                            }
                        }
                        if (tuple != null)
                        {
                            break;
                        }
                    }
                    if (tuple != null)
                    {
                        break;
                    }
                }
            }
            return tuple;
        }

        private Tuple<IFeature, Coordinate> ComputSnapVertexModeFeature(IFeatureLayer layer, Extent extent)
        {
            Tuple<IFeature, Coordinate> tuple = null;
            if (SnapMode == SnapMode.None || layer == null || extent == null)
            {
                return tuple;
            }
            if ((SnapMode & SnapMode.Vertex) > 0 && (layer.DataSet.FeatureType == FeatureType.Line || layer.DataSet.FeatureType == FeatureType.Polygon))
            {
                var features = layer.DataSet.Select(extent);
                foreach (var feature in features)
                {
                    for (int i = 0; i < feature.Geometry.NumGeometries; i++)
                    {
                        var geoItem = feature.Geometry.GetGeometryN(i);
                        for (int j = 1; j < geoItem.NumPoints - 1; j++)
                        {
                            var coord = geoItem.Coordinates[j];
                            if (extent.Contains(coord))
                            {
                                tuple = new Tuple<IFeature, Coordinate>(feature, coord);
                                break;
                            }
                        }
                        if (tuple != null)
                        {
                            break;
                        }
                    }
                    if (tuple != null)
                    {
                        break;
                    }
                }
            }
            return tuple;
        }

        private Tuple<IFeature, Coordinate> ComputSnapEdegeModeFeature(IFeatureLayer layer, Extent extent, Coordinate mouseCoord)
        {
            Tuple<IFeature, Coordinate> tuple = null;
            if (SnapMode == SnapMode.None || layer == null || mouseCoord == null)
            {
                return tuple;
            }
            var point0 = new NetTopologySuite.Geometries.Point(mouseCoord);
            if ((SnapMode & SnapMode.Edege) > 0 && (layer.DataSet.FeatureType == FeatureType.Line || layer.DataSet.FeatureType == FeatureType.Polygon))
            {
                var features = layer.DataSet.Select(extent);
                double minDistance = extent.Width / 2;
                foreach (var feature in features)
                {
                    IGeometry geo = feature.Geometry;
                    var distance = geo.Distance(point0);
                    if (distance < minDistance)
                    {
                        var coord = NetTopologySuite.Operation.Distance.DistanceOp.NearestPoints(geo, point0)?.FirstOrDefault();
                        if (coord != null)
                        {
                            tuple = new Tuple<IFeature, Coordinate>(feature, coord);
                            break;
                        }
                    }
                }
            }
            return tuple;
        }

        /// <summary>
        /// Computes a snapped coordinate.  If the mouse is near a snappable object, the output
        /// location of the mouse will be the coordinates of the object rather than the actual
        /// mouse coords.
        /// </summary>
        /// <param name="e">The event args.</param>
        /// <returns>SnapInfo</returns>
        protected virtual SnapInfo ComputeSnappedLocation(GeoMouseArgs e)
        {
            SnapInfo snapInfo = null;
            if (SnapLayers == null || e == null || Map == null || SnapMode == SnapMode.None)
                return snapInfo;

            Rectangle mouseRect = new Rectangle(e.X - SnapTol, e.Y - SnapTol, SnapTol * 2, SnapTol * 2);

            Extent extent = Map.PixelToProj(mouseRect);
            if (extent == null)
                return snapInfo;
            if (SnapMode == SnapMode.None)
            {
                return snapInfo;
            }
            Tuple<IFeature, Coordinate> tuple = null;
            SnapMode snapMode = SnapMode.None;
            if ((SnapMode & SnapMode.Point) > 0)
            {
                foreach (IFeatureLayer layer in SnapLayers.Where(_ => _.Snappable && _.VisibleAtExtent(_.Extent)))
                {
                    tuple = ComputSnapPointModeFeature(layer, extent);
                    if (tuple != null)
                    {
                        snapMode = SnapMode.Point;
                        goto Success;
                    }
                }
            }
            if ((SnapMode & SnapMode.End) > 0)
            {
                foreach (IFeatureLayer layer in SnapLayers.Where(_ => _.Snappable && _.VisibleAtExtent(_.Extent)))
                {
                    tuple = ComputSnapEndModeFeature(layer, extent);
                    if (tuple != null)
                    {
                        snapMode = SnapMode.End;
                        goto Success;
                    }
                }
            }
            if ((SnapMode & SnapMode.Vertex) > 0)
            {
                foreach (IFeatureLayer layer in SnapLayers.Where(_ => _.Snappable && _.VisibleAtExtent(_.Extent)))
                {
                    tuple = ComputSnapVertexModeFeature(layer, extent);
                    if (tuple != null)
                    {
                        snapMode = SnapMode.Vertex;
                        goto Success;
                    }
                }
            }
            if ((SnapMode & SnapMode.Edege) > 0)
            {
                foreach (IFeatureLayer layer in SnapLayers.Where(_ => _.Snappable && _.VisibleAtExtent(_.Extent)))
                {
                    tuple = ComputSnapEdegeModeFeature(layer, extent, e.GeographicLocation);
                    if (tuple != null)
                    {
                        snapMode = SnapMode.Edege;
                        goto Success;
                    }
                }
            }
        Success:
            if (tuple != null)
            {
                snapInfo = new SnapInfo()
                {
                    Feature = tuple.Item1,
                    Coordinate = tuple.Item2,
                    SnapMode = snapMode
                };
            }
            return snapInfo;
        }

        protected virtual SnapInfo ComputeSnappedLocation(GeoMouseArgs e, Coordinate coordinate)
        {
            SnapInfo snapInfo = null;
            if (coordinate == null)
            {
                return snapInfo;
            }
            snapInfo = ComputeSnappedLocation(e);
            if (snapInfo == null)
            {
                return snapInfo;
            }
            coordinate.X = snapInfo.Coordinate.X;
            coordinate.Y = snapInfo.Coordinate.Y;
            coordinate.Z = snapInfo.Coordinate.Z;
            coordinate.M = snapInfo.Coordinate.M;
            return snapInfo;
        }

        /// <summary>
        /// Perform any actions in the OnMouseMove event that are necessary for snap drawing.
        /// </summary>
        /// <param name="snapMode">snapMode</param>
        /// <param name="pos">Current position.</param>
        protected virtual void DoMouseMoveForSnapDrawing(SnapMode snapMode, Point pos)
        {
            if (snapMode != SnapMode.None)
            {
                Rectangle invalid = new Rectangle((int)(pos.X - SnapTol - SnapPen.Width), (int)(pos.Y - SnapTol - SnapPen.Width), (int)(SnapTol + SnapPen.Width) * 2, (int)((SnapTol + SnapPen.Width) * 2));
                Map.Invalidate(invalid);
            }
        }

        /// <summary>
        /// Initialize/Reinitialize the list of snap layers (i.e. when a layer has
        /// been selected or reselected).
        /// </summary>
        protected void InitializeSnapLayers()
        {
            SnapLayers = new List<IFeatureLayer>();
        }

        protected override void OnDraw(MapDrawArgs e)
        {
            if (SnapMode != SnapMode.None && SnapInfo != null)
            {
                int left, top, width, height;
                Rectangle rectangle;
                SnapPen.Color = Color.Red;
                switch (SnapInfo.SnapMode)
                {
                    case SnapMode.Point:
                    case SnapMode.End:
                    case SnapMode.Vertex:
                        left = _mousePosition.X - SnapTol;
                        top = _mousePosition.Y - SnapTol;
                        width = SnapTol * 2;
                        height = width;
                        rectangle = new Rectangle(left, top, width, height);
                        e.Graphics.DrawRectangle(SnapPen, rectangle);
                        e.Graphics.DrawLine(SnapPen, rectangle.Left, _mousePosition.Y, rectangle.Right, _mousePosition.Y);
                        e.Graphics.DrawLine(SnapPen, _mousePosition.X, rectangle.Top, _mousePosition.X, rectangle.Bottom);
                        break;
                    case SnapMode.Edege:
                        left = _mousePosition.X - 8;
                        top = _mousePosition.Y - 8;
                        width = 8 * 2;
                        height = width;
                        rectangle = new Rectangle(left, top, width, height);
                        e.Graphics.DrawRectangle(SnapPen, rectangle);
                        break;
                }
            }
            base.OnDraw(e);
        }
        protected override void OnMouseMove(GeoMouseArgs e)
        {
            Coordinate snappedCoord = e.GeographicLocation;
            var preSnapInfo = SnapInfo;
            SnapInfo = ComputeSnappedLocation(e, snappedCoord);
            if (preSnapInfo != null)
            {
                DoMouseMoveForSnapDrawing(preSnapInfo.SnapMode, _mousePosition);
            }

            _mousePosition = Map.ProjToPixel(snappedCoord);
            _mouseLocation = snappedCoord;

            if (SnapInfo != null)
            {
                DoMouseMoveForSnapDrawing(SnapInfo.SnapMode, _mousePosition);
            }

            base.OnMouseMove(e);
        }
        protected override void OnMouseUp(GeoMouseArgs e)
        {
            base.OnMouseUp(e);
        }
        #endregion
    }
}