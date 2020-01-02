using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Xml.Serialization;
using DotSpatial.Serialization;

namespace DotSpatial.Symbology
{
    [Serializable]
    [XmlRoot("MarkerStroke")]
    public class MarkerStroke : CartographicStroke, IMarkerStroke
    {
        [Serialize("Marker")]
        public IPointSymbolizer Marker { get; set; }
        public MarkerStroke() : base(StrokeStyle.Marker)
        {
            Marker = new PointSymbolizer(SymbologyGlobal.RandomColor(), PointShape.Triangle, 10);
        }
        public override void DrawLegendPath(Graphics g, GraphicsPath path, double scaleWidth)
        {
            DrawPath(g, path, scaleWidth); // draw the actual line
        }
        public override void DrawPath(Graphics g, GraphicsPath path, double scaleWidth)
        {
            DrawMarker(g, path, scaleWidth);
            if (Decorations != null)
            {
                foreach (ILineDecoration decoration in Decorations)
                {
                    decoration.Draw(g, path, scaleWidth);
                }
            }
        }

        private void DrawMarker(Graphics g, GraphicsPath path, double scaleWidth)
        {
            if (Marker == null)
            {
                return;
            }
            if (DashButtons == null || DashButtons.Length <= 1)
            {
                return;
            }
            GraphicsPathIterator myIterator = new GraphicsPathIterator(path);
            myIterator.Rewind();
            int start, end;
            bool isClosed;
            Size2D symbolSize = Marker.GetSize();
            Bitmap symbol = new Bitmap((int)symbolSize.Width, (int)symbolSize.Height);
            using (Graphics sg = Graphics.FromImage(symbol))
            {
                Marker.Draw(sg, new Rectangle(0, 0, symbol.Width, symbol.Height));
            }

            Matrix oldMat = g.Transform;
            PointF[] points;
            if (path.PointCount == 0) return;

            try
            {
                points = path.PathPoints;
            }
            catch
            {
                return;
            }
            while (myIterator.NextSubpath(out start, out end, out isClosed) > 0)
            {
                double totalLength = GetLength(points, start, end);
                PointF startPoint = points[start];
                PointF endPoint = points[end];
                double totalUsedLength = 0;
                if (DashButtons.Length == 2)
                {
                    bool dash = DashButtons[0];
                    if (!dash)
                    {
                        totalUsedLength = (float)(totalLength / 2);

                        double usedLength = 0;
                        for (int i = start; i < end; i++)
                        {
                            startPoint = points[i];
                            endPoint = points[i + 1];
                            double segmentLength = Math.Sqrt(Math.Pow(endPoint.X - startPoint.X, 2) + Math.Pow(endPoint.Y - startPoint.Y, 2));
                            if (usedLength + segmentLength > totalUsedLength)
                            {
                                double length = totalUsedLength - usedLength;
                                PointF location = GetPoint(startPoint, endPoint, length);
                                DrawImage(g, startPoint, endPoint, location, symbol);
                                break;
                            }
                            usedLength += segmentLength;
                        }
                    }
                }
                else
                {
                    int k = 0;
                    for (int i = start; i < end; i++)
                    {
                        startPoint = points[i];
                        endPoint = points[i + 1];
                        double segmentLength = Math.Sqrt(Math.Pow(endPoint.X - startPoint.X, 2) + Math.Pow(endPoint.Y - startPoint.Y, 2));
                        double usedLength = 0;
                        while (totalUsedLength < totalLength && usedLength < segmentLength)
                        {
                            if (k == DashButtons.Length)
                            {
                                k = 0;
                            }
                            bool dash = DashButtons[k];
                            if (!dash)
                            {
                                PointF location = GetPoint(startPoint, endPoint, usedLength);
                                DrawImage(g, startPoint, endPoint, location, symbol);
                            }
                            totalUsedLength++;
                            usedLength++;
                            k++;
                        }
                    }
                }
            }
        }
        private PointF GetPoint(PointF start, PointF end, double length)
        {
            PointF point = new PointF();
            double dx = end.X - start.X;
            double dy = end.Y - start.Y;
            double distance = Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));
            point.X = (float)(dx * length / distance + start.X);
            point.Y = (float)(dy * length / distance + start.Y);
            return point;
        }
        /// <summary>
        /// Gets the length of the line between startpoint and endpoint.
        /// </summary>
        /// <param name="startPoint">Startpoint of the line.</param>
        /// <param name="endPoint">Endpoint of the line.</param>
        /// <returns>Length of the line.</returns>
        private static double GetLength(PointF startPoint, PointF endPoint)
        {
            double dx = endPoint.X - startPoint.X;
            double dy = endPoint.Y - startPoint.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// Gets the length of all the lines between startpoint and endpoint.
        /// </summary>
        /// <param name="points">Points of the lines we want to measure.</param>
        /// <param name="start">Startpoint of measuring.</param>
        /// <param name="end">Endpoint of measuring.</param>
        /// <returns>Combined length of all lines between startpoint and endpoint.</returns>
        private static double GetLength(PointF[] points, int start, int end)
        {
            double result = 0;
            for (int i = start; i < end; i++)
            {
                result += GetLength(points[i], points[i + 1]);
            }

            return result;
        }
        private void DrawImage(Graphics g, PointF startPoint, PointF stopPoint, PointF locationPoint, Bitmap symbol)
        {
            Matrix oldMatrix = g.Transform;
            // Move the point to the position including the offset
            PointF offset = stopPoint == locationPoint ? GetOffset(startPoint, locationPoint) : GetOffset(locationPoint, stopPoint);
            var point = new PointF(locationPoint.X + offset.X, locationPoint.Y + offset.Y);

            // rotate it by the given angle
            float angle = 0F;
            angle = GetAngle(startPoint, stopPoint);
            Matrix rotated = g.Transform;
            rotated.RotateAt(angle, point);
            g.Transform = rotated;

            // correct the position so that the symbol is drawn centered
            point.X -= (float)symbol.Width / 2;
            point.Y -= (float)symbol.Height / 2;
            g.DrawImage(symbol, point);

            g.Transform = oldMatrix;
        }

        /// <summary>
        /// Gets the angle of the line between StartPoint and EndPoint taking into account the direction of the line.
        /// </summary>
        /// <param name="startPoint">StartPoint of the line.</param>
        /// <param name="endPoint">EndPoint of the line.</param>
        /// <returns>Angle of the given line.</returns>
        private static float GetAngle(PointF startPoint, PointF endPoint)
        {
            double deltaX = endPoint.X - startPoint.X;
            double deltaY = endPoint.Y - startPoint.Y;
            double angle = Math.Atan(deltaY / deltaX);

            if (deltaX < 0)
            {
                if (deltaY <= 0) angle += Math.PI;
                if (deltaY > 0) angle -= Math.PI;
            }

            return (float)(angle * 180.0 / Math.PI);
        }

        private PointF GetOffset(PointF point, PointF nextPoint)
        {
            var dX = nextPoint.X - point.X;
            var dY = nextPoint.Y - point.Y;
            var alpha = Math.Atan(-dX / dY);
            double x, y;

            if (dX == 0 && point.Y > nextPoint.Y)
            {
                // line is parallel to y-axis and goes bottom up
                x = Math.Cos(alpha) * -Offset;
                y = Math.Sin(alpha) * Offset;
            }
            else if (dY != 0 && point.Y > nextPoint.Y)
            {
                // line goes bottom up
                x = Math.Cos(alpha) * -Offset;
                y = Math.Sin(alpha) * -Offset;
            }
            else
            {
                // line is parallel to x-axis or goes top down
                x = Math.Cos(alpha) * Offset;
                y = Math.Sin(alpha) * Offset;
            }

            return new PointF((float)x, (float)y);
        }
    }
}
