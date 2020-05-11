// Copyright (c) DotSpatial Team. All rights reserved.
// Licensed under the MIT license. See License.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;
using DotSpatial.Data;
using DotSpatial.Serialization;
using DotSpatial.Symbology;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace DotSpatial.Controls
{
    /// <summary>
    /// This is a specialized FeatureLayer that specifically handles label drawing.
    /// </summary>
    public class MapLabelLayer : LabelLayer, IMapLabelLayer
    {
        #region Fields

        private static readonly Caches CacheList = new Caches();

        /// <summary>
        /// The existing labels, accessed for all map label layers, not just this instance
        /// </summary>
        private static readonly List<IPolygon> ExistingLabels = new List<IPolygon>(); // for collision prevention, tracks existing labels.

        #endregion

        #region  Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MapLabelLayer"/> class.
        /// </summary>
        public MapLabelLayer()
        {
            Configure();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MapLabelLayer"/> class based on the specified featureset.
        /// </summary>
        /// <param name="inFeatureSet">The feature set to build the label layer from.</param>
        public MapLabelLayer(IFeatureSet inFeatureSet)
            : base(inFeatureSet)
        {
            Configure();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MapLabelLayer"/> class based on the specified feature layer.
        /// </summary>
        /// <param name="inFeatureLayer">The feature layer to build the label layer from</param>
        public MapLabelLayer(IFeatureLayer inFeatureLayer)
            : base(inFeatureLayer)
        {
            Configure();
        }

        #endregion

        #region Events

        /// <summary>
        /// Fires an event that indicates to the parent map-frame that it should first
        /// redraw the specified clip
        /// </summary>
        public event EventHandler<ClipArgs> BufferChanged;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the back buffer that will be drawn to as part of the initialization process.
        /// </summary>
        [ShallowCopy]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Image BackBuffer { get; set; }

        /// <summary>
        /// Gets or sets the current buffer.
        /// </summary>
        [ShallowCopy]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Image Buffer { get; set; }

        /// <summary>
        /// Gets or sets the geographic region represented by the buffer.
        /// Calling Initialize will set this automatically.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Envelope BufferEnvelope { get; set; }

        /// <summary>
        /// Gets or sets the rectangle in pixels to use as the back buffer.
        /// Calling Initialize will set this automatically.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Rectangle BufferRectangle { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of labels that will be rendered before
        /// refreshing the screen.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ChunkSize { get; set; }

        /// <summary>
        /// Gets or sets the MapFeatureLayer that this label layer is attached to.
        /// </summary>
        [ShallowCopy]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new IMapFeatureLayer FeatureLayer
        {
            get
            {
                return base.FeatureLayer as IMapFeatureLayer;
            }

            set
            {
                base.FeatureLayer = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not this layer has been initialized.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool IsInitialized { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Clear all existing labels for all layers.
        /// </summary>
        public static void ClearAllExistingLabels()
        {
            ExistingLabels.Clear();
        }

        /// <summary>
        /// Draws a label on a line with various different methods.
        /// </summary>
        /// <param name="e">The map args.</param>
        /// <param name="g">Graphics object used for drawing.</param>
        /// <param name="f">Feature whose label gets drawn.</param>
        /// <param name="category">The label category the feature belongs to.</param>
        /// <param name="selected">Indicates whether the feature is selected.</param>
        /// <param name="existingLabels">List with the already existing labels.</param>
        /// <param name="symbolizer">LineSymbolizer</param>
        public static void DrawLineFeature(MapArgs e, Graphics g, IFeature f, ILabelCategory category, bool selected, List<IPolygon> existingLabels, ILineSymbolizer symbolizer)
        {
            var symb = selected ? category.SelectionSymbolizer : category.Symbolizer;

            // Gets the features text and calculate the label size
            string txt = category.CalculateExpression(f.DataRow, selected, f.Fid);
            if (string.IsNullOrWhiteSpace(txt)) return;

            SizeF labelSize = g.MeasureString(txt, CacheList.GetFont(symb));

            IGeometry geo = f.Geometry;

            if (geo.NumGeometries == 1)
            {
                var angle = GetAngleToRotate(symb, f, f.Geometry);
                RectangleF labelBounds = PlaceLineLabel(f.Geometry, e, labelSize, symb, angle, symbolizer);
                CollisionDraw(txt, g, symb, f, e, labelBounds, existingLabels, angle);
            }
            else
            {
                // Depending on the labeling strategy we do diff things
                if (symb.PartsLabelingMethod == PartLabelingMethod.LabelAllParts)
                {
                    for (int n = 0; n < geo.NumGeometries; n++)
                    {
                        var angle = GetAngleToRotate(symb, f, geo.GetGeometryN(n));
                        RectangleF labelBounds = PlaceLineLabel(geo.GetGeometryN(n), e, labelSize, symb, angle, symbolizer);
                        CollisionDraw(txt, g, symb, f, e, labelBounds, existingLabels, angle);
                    }
                }
                else
                {
                    double longestLine = 0;
                    int longestIndex = 0;
                    for (int n = 0; n < geo.NumGeometries; n++)
                    {
                        ILineString ls = geo.GetGeometryN(n) as ILineString;
                        double tempLength = 0;
                        if (ls != null) tempLength = ls.Length;
                        if (longestLine < tempLength)
                        {
                            longestLine = tempLength;
                            longestIndex = n;
                        }
                    }

                    var angle = GetAngleToRotate(symb, f, geo.GetGeometryN(longestIndex));
                    RectangleF labelBounds = PlaceLineLabel(geo.GetGeometryN(longestIndex), e, labelSize, symb, angle, symbolizer);
                    CollisionDraw(txt, g, symb, f, e, labelBounds, existingLabels, angle);
                }
            }
        }

        /// <summary>
        /// Draws a label on a point with various different methods.
        /// </summary>
        /// <param name="e">The map args.</param>
        /// <param name="g">Graphics object used for drawing.</param>
        /// <param name="f">Feature whose label gets drawn.</param>
        /// <param name="category">The label category the feature belongs to.</param>
        /// <param name="selected">Indicates whether the feature is selected.</param>
        /// <param name="existingLabels">List with the already existing labels.</param>
        /// <param name="symbolizer">point symbolizer</param>
        public static void DrawPointFeature(MapArgs e, Graphics g, IFeature f, ILabelCategory category, bool selected, List<IPolygon> existingLabels, IPointSymbolizer symbolizer)
        {
            var symb = selected ? category.SelectionSymbolizer : category.Symbolizer;

            // Gets the features text and calculate the label size
            string txt = category.CalculateExpression(f.DataRow, selected, f.Fid);
            if (txt == null) return;
            var angle = GetAngleToRotate(symb, f);
            SizeF labelSize = g.MeasureString(txt, CacheList.GetFont(symb));
            // Depending on the labeling strategy we do different things
            if (symb.PartsLabelingMethod == PartLabelingMethod.LabelAllParts)
            {
                for (int n = 0; n < f.Geometry.NumGeometries; n++)
                {
                    RectangleF labelBounds = PlacePointLabel(f.Geometry.GetGeometryN(n), e, labelSize, symb, angle, symbolizer);
                    CollisionDraw(txt, g, symb, f, e, labelBounds, existingLabels, angle);
                }
            }
            else
            {
                RectangleF labelBounds = PlacePointLabel(f.Geometry, e, labelSize, symb, angle, symbolizer);
                CollisionDraw(txt, g, symb, f, e, labelBounds, existingLabels, angle);
            }
        }

        /// <summary>
        /// Draws a label on a polygon with various different methods
        /// </summary>
        /// <param name="e">The map args.</param>
        /// <param name="g">Graphics object used for drawing.</param>
        /// <param name="f">Feature whose label gets drawn.</param>
        /// <param name="category">The label category the feature belongs to.</param>
        /// <param name="selected">Indicates whether the feature is selected.</param>
        /// <param name="existingLabels">List with the already existing labels.</param>
        public static void DrawPolygonFeature(MapArgs e, Graphics g, IFeature f, ILabelCategory category, bool selected, List<IPolygon> existingLabels)
        {
            var symb = selected ? category.SelectionSymbolizer : category.Symbolizer;

            // Gets the features text and calculate the label size
            string txt = category.CalculateExpression(f.DataRow, selected, f.Fid);
            if (txt == null) return;
            var angle = GetAngleToRotate(symb, f);
            SizeF labelSize = g.MeasureString(txt, CacheList.GetFont(symb));

            IGeometry geo = f.Geometry;

            if (geo.NumGeometries == 1)
            {
                RectangleF labelBounds = PlacePolygonLabel(f.Geometry, e, labelSize, symb, angle);
                CollisionDraw(txt, g, symb, f, e, labelBounds, existingLabels, angle);
            }
            else
            {
                if (symb.PartsLabelingMethod == PartLabelingMethod.LabelAllParts)
                {
                    for (int n = 0; n < geo.NumGeometries; n++)
                    {
                        RectangleF labelBounds = PlacePolygonLabel(geo.GetGeometryN(n), e, labelSize, symb, angle);
                        CollisionDraw(txt, g, symb, f, e, labelBounds, existingLabels, angle);
                    }
                }
                else
                {
                    double largestArea = 0;
                    IPolygon largest = null;
                    for (int n = 0; n < geo.NumGeometries; n++)
                    {
                        IPolygon pg = geo.GetGeometryN(n) as IPolygon;
                        if (pg == null) continue;
                        double tempArea = pg.Area;
                        if (largestArea < tempArea)
                        {
                            largestArea = tempArea;
                            largest = pg;
                        }
                    }

                    RectangleF labelBounds = PlacePolygonLabel(largest, e, labelSize, symb, angle);
                    CollisionDraw(txt, g, symb, f, e, labelBounds, existingLabels, angle);
                }
            }
        }

        /// <summary>
        /// Call StartDrawing before using this.
        /// </summary>
        /// <param name="rectangles">The rectangular region in pixels to clear.</param>
        /// <param name= "color">The color to use when clearing. Specifying transparent
        /// will replace content with transparent pixels.</param>
        public void Clear(List<Rectangle> rectangles, Color color)
        {
            if (BackBuffer == null) return;
            Graphics g = Graphics.FromImage(BackBuffer);
            foreach (Rectangle r in rectangles)
            {
                if (r.IsEmpty == false)
                {
                    g.Clip = new Region(r);
                    g.Clear(color);
                }
            }

            g.Dispose();
        }

        /// <summary>
        /// Draws the labels for the given features.
        /// </summary>
        /// <param name="args">The GeoArgs that control how these features should be drawn.</param>
        /// <param name="features">The features that should be drawn.</param>
        /// <param name="clipRectangles">If an entire chunk is drawn and an update is specified, this clarifies the changed rectangles.</param>
        /// <param name="useChunks">Boolean, if true, this will refresh the buffer in chunks.</param>
        public virtual void DrawFeatures(MapArgs args, List<IFeature> features, List<Rectangle> clipRectangles, bool useChunks)
        {
            if (useChunks == false)
            {
                DrawFeatures(args, features);
                return;
            }

            int count = features.Count;
            int numChunks = (int)Math.Ceiling(count / (double)ChunkSize);

            for (int chunk = 0; chunk < numChunks; chunk++)
            {
                int numFeatures = ChunkSize;
                if (chunk == numChunks - 1) numFeatures = features.Count - (chunk * ChunkSize);
                DrawFeatures(args, features.GetRange(chunk * ChunkSize, numFeatures));

                if (numChunks > 0 && chunk < numChunks - 1)
                {
                    OnBufferChanged(clipRectangles);
                    Application.DoEvents();
                }
            }
        }

        /// <summary>
        /// Draws the labels for the given features.
        /// </summary>
        /// <param name="args">The GeoArgs that control how these features should be drawn.</param>
        /// <param name="features">The features that should be drawn.</param>
        /// <param name="clipRectangles">If an entire chunk is drawn and an update is specified, this clarifies the changed rectangles.</param>
        /// <param name="useChunks">Boolean, if true, this will refresh the buffer in chunks.</param>
        public virtual void DrawFeatures(MapArgs args, List<int> features, List<Rectangle> clipRectangles, bool useChunks)
        {
            if (useChunks == false)
            {
                DrawFeatures(args, features);
                return;
            }

            int count = features.Count;
            int numChunks = (int)Math.Ceiling(count / (double)ChunkSize);

            for (int chunk = 0; chunk < numChunks; chunk++)
            {
                int numFeatures = ChunkSize;
                if (chunk == numChunks - 1) numFeatures = features.Count - (chunk * ChunkSize);
                DrawFeatures(args, features.GetRange(chunk * ChunkSize, numFeatures));

                if (numChunks > 0 && chunk < numChunks - 1)
                {
                    OnBufferChanged(clipRectangles);
                    Application.DoEvents();
                }
            }
        }

        /// <summary>
        /// This will draw any features that intersect this region. To specify the features
        /// directly, use OnDrawFeatures. This will not clear existing buffer content.
        /// For that call Initialize instead.
        /// </summary>
        /// <param name="args">A GeoArgs clarifying the transformation from geographic to image space</param>
        /// <param name="regions">The geographic regions to draw</param>
        /// <param name="selected">If this is true, nothing is painted, because selected labels get painted together with not selected labels.</param>
        public void DrawRegions(MapArgs args, List<Extent> regions, bool selected)
        {
            if (FeatureSet == null || selected) return;
#if DEBUG
            var sw = new Stopwatch();
            sw.Start();
#endif

            if (FeatureSet.IndexMode)
            {
                // First determine the number of features we are talking about based on region.
                List<int> drawIndices = new List<int>();
                foreach (Extent region in regions)
                {
                    if (region != null)
                    {
                        // We need to consider labels that go off the screen. Figure a region that is larger.
                        Extent sur = region.Copy();
                        sur.ExpandBy(region.Width, region.Height);

                        // Use union to prevent duplicates. No sense in drawing more than we have to.
                        drawIndices = drawIndices.Union(FeatureSet.SelectIndices(sur)).ToList();
                    }
                }

                List<Rectangle> clips = args.ProjToPixel(regions);
                DrawFeatures(args, drawIndices, clips, true);
            }
            else
            {
                // First determine the number of features we are talking about based on region.
                List<IFeature> drawList = new List<IFeature>();
                foreach (Extent region in regions)
                {
                    if (region != null)
                    {
                        // We need to consider labels that go off the screen. Figure a region that is larger.
                        Extent r = region.Copy();
                        r.ExpandBy(region.Width, region.Height);

                        // Use union to prevent duplicates. No sense in drawing more than we have to.
                        drawList = drawList.Union(FeatureSet.Select(r)).ToList();
                    }
                }

                List<Rectangle> clipRects = args.ProjToPixel(regions);
                DrawFeatures(args, drawList, clipRects, true);
            }

#if DEBUG
            sw.Stop();
            Debug.WriteLine("MapLabelLayer {0} DrawRegions: {1} ms", FeatureSet.Name, sw.ElapsedMilliseconds);
#endif
        }

        /// <inheritdoc/>
        public virtual void Print(MapArgs args, List<Extent> regions, bool selected)
        {
            DrawRegions(args, regions, selected);
        }

        /// <summary>
        /// Indicates that the drawing process has been finalized and swaps the back buffer
        /// to the front buffer.
        /// </summary>
        public void FinishDrawing()
        {
            OnFinishDrawing();
            if (Buffer != null && Buffer != BackBuffer) Buffer.Dispose();
            Buffer = BackBuffer;
            FeatureLayer.Invalidate();
        }

        /// <summary>
        /// Copies any current content to the back buffer so that drawing should occur on the
        /// back buffer (instead of the fore-buffer). Calling draw methods without
        /// calling this may cause exceptions.
        /// </summary>
        /// <param name="preserve">Boolean, true if the front buffer content should be copied to the back buffer
        /// where drawing will be taking place.</param>
        public void StartDrawing(bool preserve)
        {
            Bitmap backBuffer = new Bitmap(BufferRectangle.Width, BufferRectangle.Height);
            if (Buffer != null && preserve && Buffer.Width == backBuffer.Width && Buffer.Height == backBuffer.Height)
            {
                Graphics g = Graphics.FromImage(backBuffer);
                g.DrawImageUnscaled(Buffer, 0, 0);
            }

            if (BackBuffer != null && BackBuffer != Buffer) BackBuffer.Dispose();
            BackBuffer = backBuffer;
            OnStartDrawing();
        }

        /// <summary>
        /// Fires the OnBufferChanged event
        /// </summary>
        /// <param name="clipRectangles">The Rectangle in pixels</param>
        protected virtual void OnBufferChanged(List<Rectangle> clipRectangles)
        {
            BufferChanged?.Invoke(this, new ClipArgs(clipRectangles));
        }

        /// <summary>
        /// Indiciates that whatever drawing is going to occur has finished and the contents
        /// are about to be flipped forward to the front buffer.
        /// </summary>
        protected virtual void OnFinishDrawing()
        {
        }

        /// <summary>
        /// Occurs when a new drawing is started, but after the BackBuffer has been established.
        /// </summary>
        protected virtual void OnStartDrawing()
        {
        }

        private static IPolygon GetPolygon(RectangleF rectangleF, float angle)
        {
            IPolygon polygon = null;
            float left = rectangleF.Left;
            float top = rectangleF.Top;
            float right = rectangleF.Right;
            float bottom = rectangleF.Bottom;
            PointF[] points = { new PointF(left, top), new PointF(right, top), new PointF(right, bottom), new PointF(left, bottom) };
            using (Matrix matrix = new Matrix())
            {
                matrix.RotateAt(angle, rectangleF.Location, MatrixOrder.Append);
                matrix.TransformPoints(points);
            }
            var coordinates = points.Select(x => new Coordinate(x.X, x.Y)).ToList();
            coordinates.Add(coordinates[0]);
            ILinearRing shell = new LinearRing(coordinates.ToArray());
            polygon = new Polygon(shell);
            return polygon;
        }
        /// <summary>
        /// Checks whether the given rectangle collides with the drawnRectangles.
        /// </summary>
        /// <param name="polygon">Rectangle that we want to draw next</param>
        /// <param name="drawnRectangles">Rectangle that were already drawn</param>
        /// <returns>True, if the rectangle collides with a rectancle that was already drawn</returns>
        private static bool Collides(IPolygon polygon, List<IPolygon> drawnRectangles)
        {
            bool ret = false;
            foreach (IPolygon tmpPolygon in drawnRectangles)
            {
                if (polygon.Envelope.Intersects(tmpPolygon.Envelope))
                {
                    if (polygon.Intersects(tmpPolygon))
                    {
                        ret = true;
                        break;
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Draws the given text if it is on screen. If PreventCollision is set only labels that don't collide with existingLables are drawn.
        /// </summary>
        /// <param name="txt">Text that should be drawn.</param>
        /// <param name="g">Graphics object that does the drawing.</param>
        /// <param name="symb">Symbolizer to figure out the look of the label.</param>
        /// <param name="f">Feature, the label belongs to.</param>
        /// <param name="e">The map args.</param>
        /// <param name="labelBounds">The bounds of the label.</param>
        /// <param name="existingLabels">List with labels that were already drawn.</param>
        /// <param name="angle">Angle in degree the label gets rotated by.</param>
        private static void CollisionDraw(string txt, Graphics g, ILabelSymbolizer symb, IFeature f, MapArgs e, RectangleF labelBounds, List<IPolygon> existingLabels, float angle)
        {
            if (labelBounds.IsEmpty || !e.ImageRectangle.IntersectsWith(labelBounds)) return;
            if (symb.PreventCollisions)
            {
                IPolygon polygon = GetPolygon(labelBounds, angle);
                if (!Collides(polygon, existingLabels))
                {
                    DrawLabel(g, txt, labelBounds, symb, f, angle);
                    existingLabels.Add(polygon);
                }
            }
            else
            {
                DrawLabel(g, txt, labelBounds, symb, f, angle);
            }
        }

        /// <summary>
        /// Draws labels in a specified rectangle
        /// </summary>
        /// <param name="g">The graphics object to draw to</param>
        /// <param name="labelText">The label text to draw</param>
        /// <param name="labelBounds">The rectangle of the label</param>
        /// <param name="symb">the Label Symbolizer to use when drawing the label</param>
        /// <param name="feature">Feature to draw</param>
        /// <param name="angle">Angle in degree the label gets rotated by.</param>
        private static void DrawLabel(Graphics g, string labelText, RectangleF labelBounds, ILabelSymbolizer symb, IFeature feature, float angle)
        {
            // Sets up the brushes and such for the labeling
            Font textFont = CacheList.GetFont(symb);
            var format = new StringFormat
            {
                Alignment = symb.Alignment
            };

            // Text graphics path
            var gp = new GraphicsPath();
            gp.AddString(labelText, textFont.FontFamily, (int)textFont.Style, textFont.SizeInPoints * 96F / 72F, labelBounds, format);

            // Rotate text
            RotateAt(g, labelBounds.X, labelBounds.Y, angle);

            // Draws the text outline
            if (symb.BackColorEnabled && symb.BackColor != Color.Transparent)
            {
                var backBrush = CacheList.GetSolidBrush(symb.BackColor);
                if (symb.FontColor == Color.Transparent)
                {
                    using (var backgroundGp = new GraphicsPath())
                    {
                        backgroundGp.AddRectangle(labelBounds);
                        backgroundGp.FillMode = FillMode.Alternate;
                        backgroundGp.AddPath(gp, true);
                        g.FillPath(backBrush, backgroundGp);
                    }
                }
                else
                {
                    g.FillRectangle(backBrush, labelBounds);
                }
            }

            // Draws the border if its enabled
            if (symb.BorderVisible && symb.BorderColor != Color.Transparent)
            {
                var borderPen = CacheList.GetPen(symb.BorderColor);
                g.DrawRectangle(borderPen, labelBounds.X, labelBounds.Y, labelBounds.Width, labelBounds.Height);
            }

            // Draws the drop shadow
            if (symb.DropShadowEnabled && symb.DropShadowColor != Color.Transparent)
            {
                var shadowBrush = CacheList.GetSolidBrush(symb.DropShadowColor);
                var gpTrans = new Matrix();
                gpTrans.Translate(symb.DropShadowPixelOffset.X, symb.DropShadowPixelOffset.Y);
                gp.Transform(gpTrans);
                g.FillPath(shadowBrush, gp);
                gpTrans = new Matrix();
                gpTrans.Translate(-symb.DropShadowPixelOffset.X, -symb.DropShadowPixelOffset.Y);
                gp.Transform(gpTrans);
                gpTrans.Dispose();
            }

            // Draws the text halo
            if (symb.HaloEnabled && symb.HaloColor != Color.Transparent)
            {
                using (var haloPen = new Pen(symb.HaloColor)
                {
                    Width = 2,
                    Alignment = PenAlignment.Outset
                })
                    g.DrawPath(haloPen, gp);
            }

            // Draws the text if its not transparent
            if (symb.FontColor != Color.Transparent)
            {
                var foreBrush = CacheList.GetSolidBrush(symb.FontColor);
                g.FillPath(foreBrush, gp);
            }

            gp.Dispose();
        }

        /// <summary>
        /// Rotates the label for the given feature by the angle of the LabelSymbolizer.
        /// </summary>
        /// <param name="symb">LabelSymbolizer that indicates the angle to use.</param>
        /// <param name="feature">Feature whose label gets rotated.</param>
        /// <param name="lineString">Line string to get the angle from if line orientation should be used.</param>
        /// <returns>Resulting angle in degree.</returns>
        private static float GetAngleToRotate(ILabelSymbolizer symb, IFeature feature, IGeometry lineString = null)
        {
            if (symb.UseAngle)
            {
                return ToSingle(symb.Angle);
            }

            if (symb.UseLabelAngleField)
            {
                var angleField = symb.LabelAngleField;
                if (string.IsNullOrEmpty(angleField)) return 0;
                return ToSingle(feature.DataRow[angleField]);
            }

            if (symb.UseLineOrientation)
            {
                LineString ls = lineString as LineString;
                if (ls != null)
                {
                    ls = GetSegment(ls, symb);
                    if (ls == null) return 0;
                    if (symb.LineOrientation == LineOrientation.Parallel)
                        return ToSingle(-Angle(ls));
                    return ToSingle(-ls.Angle - 90);
                }
            }

            return 0;
        }

        private static double Angle(LineString lineString)
        {
            var endPoint = lineString.EndPoint;
            var startPoint = lineString.StartPoint;
            var dx = endPoint.X - startPoint.X;
            var dy = endPoint.Y - startPoint.Y;
            var dc = Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));
            double radian = 0;
            if (dc != 0)
            {
                radian = Math.Acos(dx / dc);
                if (dy < 0)
                {
                    radian = Math.PI * 2 - radian;
                }
                radian %= 2 * Math.PI;
            }
            double angle = radian * 180 / Math.PI;

            return angle;
        }
        /// <summary>
        /// Gets the segment of the LineString that is used to position and rotate the label.
        /// </summary>
        /// <param name="lineString">LineString to get the segment from.</param>
        /// <param name="symb">Symbolizer to get the LineLabelPlacement from.</param>
        /// <returns>Null on unnown LineLabelPlacementMethod else the calculated segment. </returns>
        private static LineString GetSegment(LineString lineString, ILabelSymbolizer symb)
        {
            if (lineString.Coordinates.Length <= 2) return lineString;

            var coords = lineString.Coordinates;
            switch (symb.LineLabelPlacementMethod)
            {
                case LineLabelPlacementMethod.FirstSegment:
                    return new LineString(new[] { coords[0], coords[1] });
                case LineLabelPlacementMethod.LastSegment:
                    return new LineString(new[] { coords[coords.Length - 2], coords[coords.Length - 1] });
                case LineLabelPlacementMethod.MiddleSegment:
                    int start = (int)Math.Ceiling(coords.Length / 2D) - 1;
                    return new LineString(new[] { coords[start], coords[start + 1] });
                case LineLabelPlacementMethod.LongestSegment:
                    double length = 0;
                    LineString temp = null;
                    for (int i = 0; i < coords.Length - 1; i++)
                    {
                        LineString l = new LineString(new[] { coords[i], coords[i + 1] });
                        if (l.Length > length)
                        {
                            length = l.Length;
                            temp = l;
                        }
                    }

                    return temp;
            }

            return null;
        }

        /// <summary>
        /// Places the label according to the selected LabelPlacementMethode.
        /// </summary>
        /// <param name="lineString">LineString, whose label gets drawn.</param>
        /// <param name="e">The map args.</param>
        /// <param name="labelSize">the size of the label.</param>
        /// <param name="symb">Symbolizer to figure out the look of the label.</param>
        /// <param name="angle">Angle in degree the label gets rotated by.</param>
        /// <param name="symbolizer">feature symbolizer</param>
        /// <returns>The RectangleF that is needed to draw the label.</returns>
        private static RectangleF PlaceLineLabel(IGeometry lineString, MapArgs e, SizeF labelSize, ILabelSymbolizer symb, float angle, ILineSymbolizer symbolizer)
        {
            LineString ls = lineString as LineString;
            if (ls == null) return Rectangle.Empty;

            ls = GetSegment(ls, symb);
            var c = ls.Centroid.Coordinate;
            if (!e.GeographicExtents.Intersects(c)) return RectangleF.Empty;
            PointF adjustment = GetLineLabelPosition(symb, labelSize, symbolizer);
            return PlaceLabel(e, c, adjustment, angle, labelSize);
        }
        private static RectangleF PlaceLabel(MapArgs e, Coordinate c, PointF adjustment, float angle, SizeF lz)
        {
            RotatePoint(ref adjustment, angle); // rotates the adjustment according to the given angle
            float x = Convert.ToSingle((c.X - e.MinX) * e.Dx) + e.ImageRectangle.X + adjustment.X;
            float y = Convert.ToSingle((e.MaxY - c.Y) * e.Dy) + e.ImageRectangle.Y + adjustment.Y;
            return new RectangleF(x, y, lz.Width, lz.Height);
        }
        private static RectangleF PlacePointLabel(IGeometry f, MapArgs e, SizeF labelSize, ILabelSymbolizer symb, float angle, IPointSymbolizer symbolizer)
        {
            Coordinate c = f.GetGeometryN(1).Coordinates[0];
            if (!e.GeographicExtents.Intersects(c)) return RectangleF.Empty;
            PointF adjustment = GetPointLabelPosition(symb, labelSize, symbolizer);
            return PlaceLabel(e, c, adjustment, angle, labelSize);
        }

        /// <summary>
        /// Calculates the position of the polygon label.
        /// </summary>
        /// <param name="geom">The polygon the label belongs to.</param>
        /// <param name="e">The map args.</param>
        /// <param name="labelSize">the labelSize.</param>
        /// <param name="symb">The symbolizer that contains the drawing styles.</param>
        /// <param name="angle">The angle used for drawing.</param>
        /// <returns>The resulting drawing rectangle.</returns>
        private static RectangleF PlacePolygonLabel(IGeometry geom, MapArgs e, SizeF labelSize, ILabelSymbolizer symb, float angle)
        {
            IPolygon pg = geom as IPolygon;
            if (pg == null) return RectangleF.Empty;
            Coordinate c;
            switch (symb.LabelPlacementMethod)
            {
                case LabelPlacementMethod.Centroid:
                    c = pg.Centroid.Coordinates[0];
                    break;
                case LabelPlacementMethod.InteriorPoint:
                    c = pg.InteriorPoint.Coordinate;
                    break;
                default:
                    c = geom.EnvelopeInternal.Centre;
                    break;
            }

            if (!e.GeographicExtents.Intersects(c)) return RectangleF.Empty;
            PointF adjustment = GetLabelPosition(symb, labelSize);
            return PlaceLabel(e, c, adjustment, angle, labelSize);
        }

        /// <summary>
        /// Calculates the adjustment of the the label's position based on the symbolizers orientation.
        /// </summary>
        /// <param name="symb">ILabelSymbolizer whose orientation should be considered.</param>
        /// <param name="size">Size of the label.</param>
        /// <returns>New label-position based on label-size and symbolizer-orientation.</returns>
        private static PointF GetLabelPosition(ILabelSymbolizer symb, SizeF size)
        {
            ContentAlignment orientation = symb.Orientation;
            float x = symb.OffsetX;
            float y = -symb.OffsetY;
            switch (orientation)
            {
                case ContentAlignment.TopLeft:
                    return new PointF(-size.Width + x, -size.Height + y);
                case ContentAlignment.TopCenter:
                    return new PointF((-size.Width / 2) + x, -size.Height + y);
                case ContentAlignment.TopRight:
                    return new PointF(0 + x, -size.Height + y);
                case ContentAlignment.MiddleLeft:
                    return new PointF(-size.Width + x, (-size.Height / 2) + y);
                case ContentAlignment.MiddleCenter:
                    return new PointF((-size.Width / 2) + x, (-size.Height / 2) + y);
                case ContentAlignment.MiddleRight:
                    return new PointF(0 + x, (-size.Height / 2) + y);
                case ContentAlignment.BottomLeft:
                    return new PointF(-size.Width + x, 0 + y);
                case ContentAlignment.BottomCenter:
                    return new PointF((-size.Width / 2) + x, 0 + y);
                case ContentAlignment.BottomRight:
                    return new PointF(0 + x, 0 + y);
            }
            return new PointF(0, 0);
        }
        private static PointF GetPointLabelPosition(ILabelSymbolizer symb, SizeF size, IPointSymbolizer symbolizer)
        {
            ContentAlignment orientation = symb.Orientation;
            var symbolSize2d = symbolizer.GetSize();
            var symbolSize = new SizeF(Convert.ToSingle(symbolSize2d.Width), Convert.ToSingle(symbolSize2d.Height));
            var halfWidth = symbolSize.Width / 2;
            var halfHeight = symbolSize.Height / 2;
            float x = symb.OffsetX;
            float y = -symb.OffsetY;
            switch (orientation)
            {
                case ContentAlignment.TopLeft:
                    return new PointF(-size.Width + x, -size.Height + y - halfHeight);
                case ContentAlignment.TopCenter:
                    return new PointF((-size.Width / 2) + x, -size.Height + y - halfHeight);
                case ContentAlignment.TopRight:
                    return new PointF(0 + x, -size.Height + y - halfHeight);
                case ContentAlignment.MiddleLeft:
                    return new PointF(-size.Width + x - halfWidth, (-size.Height / 2) + y);
                case ContentAlignment.MiddleCenter:
                    return new PointF((-size.Width / 2) + x, (-size.Height / 2) + y);
                case ContentAlignment.MiddleRight:
                    return new PointF(0 + x + halfWidth, (-size.Height / 2) + y);
                case ContentAlignment.BottomLeft:
                    return new PointF(-size.Width + x, 0 + y + halfHeight);
                case ContentAlignment.BottomCenter:
                    return new PointF((-size.Width / 2) + x, 0 + y + halfHeight);
                case ContentAlignment.BottomRight:
                    return new PointF(0 + x, 0 + y + halfHeight);
            }
            return new PointF(0, 0);
        }
        private static PointF GetLineLabelPosition(ILabelSymbolizer symb, SizeF size, ILineSymbolizer symbolizer)
        {
            ContentAlignment orientation = symb.Orientation;
            var lineWidth = Convert.ToSingle(symbolizer.GetWidth());
            float x = symb.OffsetX;
            float y = -symb.OffsetY;
            switch (orientation)
            {
                case ContentAlignment.TopLeft:
                    return new PointF(-size.Width + x, -size.Height + y - lineWidth);
                case ContentAlignment.TopCenter:
                    return new PointF((-size.Width / 2) + x, -size.Height + y - lineWidth);
                case ContentAlignment.TopRight:
                    return new PointF(0 + x, -size.Height + y - lineWidth);
                case ContentAlignment.MiddleLeft:
                    return new PointF(-size.Width + x - lineWidth, (-size.Height / 2) + y);
                case ContentAlignment.MiddleCenter:
                    return new PointF((-size.Width / 2) + x, (-size.Height / 2) + y);
                case ContentAlignment.MiddleRight:
                    return new PointF(0 + x + lineWidth, (-size.Height / 2) + y);
                case ContentAlignment.BottomLeft:
                    return new PointF(-size.Width + x, 0 + y + lineWidth);
                case ContentAlignment.BottomCenter:
                    return new PointF((-size.Width / 2) + x, 0 + y + lineWidth);
                case ContentAlignment.BottomRight:
                    return new PointF(0 + x, 0 + y + lineWidth);
            }
            return new PointF(0, 0);
        }

        private static void RotateAt(Graphics gr, float cx, float cy, float angle)
        {
            gr.ResetTransform();
            gr.TranslateTransform(-cx, -cy, MatrixOrder.Append);
            gr.RotateTransform(angle, MatrixOrder.Append);
            gr.TranslateTransform(cx, cy, MatrixOrder.Append);
        }

        /// <summary>
        /// Rotates the given point by angle around (0,0).
        /// </summary>
        /// <param name="point">Point that gets rotated.</param>
        /// <param name="angle">Angle in degree.</param>
        private static void RotatePoint(ref PointF point, double angle)
        {
            double rad = angle * Math.PI / 180;
            double x = (Math.Cos(rad) * point.X) - (Math.Sin(rad) * point.Y);
            double y = (Math.Sin(rad) * point.X) + (Math.Cos(rad) * point.Y);
            point.X = (float)x;
            point.Y = (float)y;
        }

        /// <summary>
        /// Converts the given value to single.
        /// </summary>
        /// <param name="value">Value that gets converted to single.</param>
        /// <returns>0 on error else the resulting value.</returns>
        private static float ToSingle(object value)
        {
            try
            {
                return Convert.ToSingle(value);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private void Configure()
        {
            ChunkSize = 10000;
        }

        /// <summary>
        /// Draws the labels for the given features.
        /// </summary>
        /// <param name="e">MapArgs to get Graphics object from.</param>
        /// <param name="features">Indizes of the features whose labels get drawn.</param>
        private void DrawFeatures(MapArgs e, IEnumerable<int> features)
        {
            // Check that exists at least one category with Expression
            if (Symbology.Categories.All(_ => string.IsNullOrEmpty(_.Expression))) return;

            Graphics g = e.Device ?? Graphics.FromImage(BackBuffer);
            Matrix origTransform = g.Transform;

            // Only draw features that are currently visible.
            //if (FastDrawnStates == null)
            //{
            //    CreateIndexedLabels();
            //}

            //FastLabelDrawnState[] drawStates = FastDrawnStates;
            //if (drawStates == null) return;

            // Sets the graphics objects smoothing modes
            g.TextRenderingHint = TextRenderingHint.AntiAlias;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Action<ILabelCategory, IFeature> drawFeature;
            bool selected = false;
            switch (FeatureSet.FeatureType)
            {
                case FeatureType.Polygon:
                    drawFeature = (category, feature) => DrawPolygonFeature(e, g, feature, category, selected, ExistingLabels);
                    break;
                case FeatureType.Line:
                    drawFeature = (category, feature) =>
                    {
                        var symbolizer = FeatureLayer.GetCategory(feature).Symbolizer as ILineSymbolizer;
                        DrawLineFeature(e, g, feature, category, selected, ExistingLabels, symbolizer);
                    };
                    break;
                case FeatureType.Point:
                case FeatureType.MultiPoint:
                    drawFeature = (category, feature) =>
                    {
                        var symbolizer = FeatureLayer.GetCategory(feature).Symbolizer as IPointSymbolizer;
                        DrawPointFeature(e, g, feature, category, selected, ExistingLabels, symbolizer);
                    };
                    break;
                default:
                    return; // Can't draw something else
            }

            foreach (var category in Symbology.Categories)
            {
                category.UpdateExpressionColumns(FeatureSet.DataTable.Columns);
                var catFeatures = new List<int>();
                foreach (int fid in features)
                {
                    //if (drawStates[fid] == null || drawStates[fid].Category == null) continue;
                    //if (drawStates[fid].Category == category)
                    //{
                    //    catFeatures.Add(fid);
                    //}
                    catFeatures.Add(fid);
                }

                // Now that we are restricted to a certain category, we can look at priority
                if (category.Symbolizer.PriorityField != "FID")
                {
                    Feature.ComparisonField = category.Symbolizer.PriorityField;
                    catFeatures.Sort();

                    // When preventing collisions, we want to do high priority first.
                    // Otherwise, do high priority last.
                    if (category.Symbolizer.PreventCollisions)
                    {
                        if (!category.Symbolizer.PrioritizeLowValues)
                        {
                            catFeatures.Reverse();
                        }
                    }
                    else
                    {
                        if (category.Symbolizer.PrioritizeLowValues)
                        {
                            catFeatures.Reverse();
                        }
                    }
                }

                foreach (var fid in catFeatures)
                {
                    if (!FeatureLayer.DrawnStates[fid].Visible) continue;
                    var feature = FeatureSet.GetFeature(fid);
                    drawFeature(category, feature);
                }
            }

            if (e.Device == null) g.Dispose();
            else g.Transform = origTransform;
        }

        /// <summary>
        /// Draws the labels for the given features.
        /// </summary>
        /// <param name="e">MapArgs to get Graphics object from.</param>
        /// <param name="features">Features, whose labels get drawn.</param>
        private void DrawFeatures(MapArgs e, IEnumerable<IFeature> features)
        {
            // Check that exists at least one category with Expression
            if (Symbology.Categories.All(_ => string.IsNullOrEmpty(_.Expression))) return;

            Graphics g = e.Device ?? Graphics.FromImage(BackBuffer);
            Matrix origTransform = g.Transform;

            // Only draw features that are currently visible.
            var featureList = features as IList<IFeature> ?? features.ToList();
            if (DrawnStates == null || !DrawnStates.ContainsKey(featureList.First()))
            {
                CreateLabels();
            }

            //Dictionary<IFeature, LabelDrawState> drawStates = DrawnStates;
            //if (drawStates == null) return;

            // Sets the graphics objects smoothing modes
            g.TextRenderingHint = TextRenderingHint.AntiAlias;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            bool selected = false;
            Action<IFeature, ILabelCategory> drawFeature;
            switch (featureList.First().FeatureType)
            {
                case FeatureType.Polygon:
                    // drawFeature = (f, category) => DrawPolygonFeature(e, g, f, category, drawStates[f].Selected, ExistingLabels);
                    drawFeature = (f, category) => DrawPolygonFeature(e, g, f, category, selected, ExistingLabels);
                    break;
                case FeatureType.Line:
                    drawFeature = (f, category) =>
                    {
                        var symbolizer = FeatureLayer.GetCategory(f).Symbolizer as ILineSymbolizer;
                        DrawLineFeature(e, g, f, category, selected, ExistingLabels, symbolizer);
                    };
                    break;
                case FeatureType.Point:
                case FeatureType.MultiPoint:
                    drawFeature = (f, category) =>
                    {
                        var symbolizer = FeatureLayer.GetCategory(f).Symbolizer as IPointSymbolizer;
                        DrawPointFeature(e, g, f, category, selected, ExistingLabels, symbolizer);
                    };
                    break;
                default:
                    return; // Can't draw something else
            }

            foreach (ILabelCategory category in Symbology.Categories)
            {
                category.UpdateExpressionColumns(FeatureSet.DataTable.Columns);
                var cat = category; // prevent access to unmodified closure problems
                List<IFeature> catFeatures = new List<IFeature>();
                foreach (IFeature f in featureList)
                {
                    //if (drawStates.ContainsKey(f) && drawStates[f].Category == cat)
                    //{
                    //    catFeatures.Add(f);
                    //}
                    catFeatures.Add(f);
                }

                // Now that we are restricted to a certain category, we can look at
                // priority
                if (category.Symbolizer.PriorityField != "FID")
                {
                    Feature.ComparisonField = cat.Symbolizer.PriorityField;
                    catFeatures.Sort();

                    // When preventing collisions, we want to do high priority first.
                    // otherwise, do high priority last.
                    if (cat.Symbolizer.PreventCollisions)
                    {
                        if (!cat.Symbolizer.PrioritizeLowValues)
                        {
                            catFeatures.Reverse();
                        }
                    }
                    else
                    {
                        if (cat.Symbolizer.PrioritizeLowValues)
                        {
                            catFeatures.Reverse();
                        }
                    }
                }

                for (int i = 0; i < catFeatures.Count; i++)
                {
                    if (!FeatureLayer.DrawnStates[i].Visible) continue;
                    drawFeature(catFeatures[i], category);
                }
            }

            if (e.Device == null) g.Dispose();
            else g.Transform = origTransform;
        }

        #endregion

        #region Classes

        private class Caches
        {
            #region Fields

            private readonly Dictionary<Color, Pen> _pens = new Dictionary<Color, Pen>();
            private readonly Dictionary<Color, Brush> _solidBrushes = new Dictionary<Color, Brush>();
            private readonly Dictionary<string, Font> _symbFonts = new Dictionary<string, Font>();

            #endregion

            #region Methods

            public Font GetFont(ILabelSymbolizer symb)
            {
                var fontDesc = $"{symb.FontFamily};{symb.FontSize};{symb.FontStyle}";
                return _symbFonts.GetOrAdd(fontDesc, _ => symb.GetFont());
            }

            public Pen GetPen(Color color)
            {
                return _pens.GetOrAdd(color, _ => new Pen(color));
            }

            public Brush GetSolidBrush(Color color)
            {
                return _solidBrushes.GetOrAdd(color, _ => new SolidBrush(color));
            }

            #endregion
        }

        #endregion
    }
}