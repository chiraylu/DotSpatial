// Copyright (c) DotSpatial Team. All rights reserved.
// Licensed under the MIT license. See License.txt file in the project root for full license information.

using DotSpatial.Data;
using GeoAPI.Geometries;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace DotSpatial.Controls
{
    /// <summary>
    /// A MapFunction that zooms the map by scrolling the scroll wheel and pans the map by pressing the mouse wheel and moving the mouse.
    /// </summary>
    public class MapFunctionZoom : MapFunction
    {
        #region Fields

        private Rectangle _client;
        private int _direction;
        private Point _dragStart;
        private bool _isDragging;
        private IMapFrame _mapFrame;
        private bool _preventDrag;
        private double _sensitivity;
        private Rectangle _source;
        private int _timerInterval;
        private Timer _zoomTimer;

        #endregion

        #region  Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MapFunctionZoom"/> class.
        /// </summary>
        /// <param name="inMap">The map the tool should work on.</param>
        public MapFunctionZoom(IMap inMap)
            : base(inMap)
        {
            Configure();
            BusySet = false;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether the map function is currently interacting with the map.
        /// </summary>
        public bool BusySet { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether forward zooms in. This controls the sense (direction) of zoom (in or out) as you roll the mouse wheel.
        /// </summary>
        public bool ForwardZoomsIn
        {
            get
            {
                return _direction > 0;
            }

            set
            {
                _direction = value ? 1 : -1;
            }
        }

        /// <summary>
        /// Gets or sets the wheel zoom sensitivity. Increasing makes it more sensitive. Maximum is 0.5, Minimum is 0.01
        /// </summary>
        public double Sensitivity
        {
            get
            {
                return 1.0 / _sensitivity;
            }

            set
            {
                if (value > 0.5)
                    value = 0.5;
                else if (value < 0.01)
                    value = 0.01;
                _sensitivity = 1.0 / value;
            }
        }

        /// <summary>
        /// Gets or sets the full refresh timeout value in milliseconds
        /// </summary>
        public int TimerInterval
        {
            get
            {
                return _timerInterval;
            }

            set
            {
                _timerInterval = value;
                _zoomTimer.Interval = _timerInterval;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Handles the actions that the tool controls during the OnMouseDown event
        /// </summary>
        /// <param name="e">The event args.</param>
        protected override void OnMouseDown(GeoMouseArgs e)
        {
            if (e.Button == MouseButtons.Middle && !_preventDrag)
            {
                _dragStart = e.Location;
                _source = e.Map.MapFrame.View;
            }

            base.OnMouseDown(e);
        }

        /// <summary>
        /// Handles the mouse move event, changing the viewing extents to match the movements
        /// of the mouse if the left mouse button is down.
        /// </summary>
        /// <param name="e">The event args.</param>
        protected override void OnMouseMove(GeoMouseArgs e)
        {
            if (_dragStart != Point.Empty && !_preventDrag)
            {
                if (!BusySet)
                {
                    Map.IsBusy = true;
                    BusySet = true;
                }

                _isDragging = true;
                Point diff = new Point
                {
                    X = _dragStart.X - e.X,
                    Y = _dragStart.Y - e.Y
                };
                e.Map.MapFrame.View = new Rectangle(_source.X + diff.X, _source.Y + diff.Y, _source.Width, _source.Height);
                Map.Invalidate();
            }

            base.OnMouseMove(e);
        }

        /// <summary>
        /// Mouse Up
        /// </summary>
        /// <param name="e">The event args.</param>
        protected override void OnMouseUp(GeoMouseArgs e)
        {
            if (e.Button == MouseButtons.Middle && _isDragging)
            {
                _isDragging = false;
                _preventDrag = true;
                e.Map.MapFrame.ResetExtents();
                _preventDrag = false;
                Map.IsBusy = false;
                BusySet = false;
            }

            _dragStart = Point.Empty;

            base.OnMouseUp(e);
        }

        /// <summary>
        /// Mouse Wheel
        /// </summary>
        /// <param name="e">The event args.</param>
        protected override void OnMouseWheel(GeoMouseArgs e)
        {
            // Fix this
            _zoomTimer.Stop(); // if the timer was already started, stop it.
            Rectangle r = e.Map.MapFrame.View;
            Extent extent = e.Map.MapFrame.ViewExtents;

            // For multiple zoom steps before redrawing, we actually
            // want the x coordinate relative to the screen, not
            // the x coordinate relative to the previously modified view.
            if (_client == Rectangle.Empty) _client = r;
            int cw = _client.Width;
            int ch = _client.Height;

            double w = r.Width;
            double h = r.Height;
            double srcCenterX = r.X + r.Width / 2.0;
            double srcCenterY = r.Y + r.Height / 2.0;
            if (_direction * e.Delta > 0)
            {
                double ratio = Sensitivity / 2;
                double dHalfWidth = -w * ratio;
                double dHalfHeight = -h * ratio;
                r.Inflate(Convert.ToInt32(dHalfWidth), Convert.ToInt32(dHalfHeight));
                double ratioWidth = r.Width / w;
                double ratioHeight = r.Height / h;
                double destX = e.X * ratioWidth + (1 - ratioWidth) * srcCenterX;
                double destY = e.Y * ratioHeight + (1 - ratioHeight) * srcCenterY;
                int xOff = Convert.ToInt32(e.X - destX);
                int yOff = Convert.ToInt32(e.Y - destY);
                r.X += xOff;
                r.Y += yOff;

                //Extent destExtent = (Extent)extent.Clone();
                //var currentCoord = e.Map.MapFrame.BufferToProj(e.Location);
                //destExtent.SetCenter(currentCoord);
                //destExtent.ExpandBy(-extent.Width * ratio, -extent.Height * ratio);
                //var destCoordX = (extent.Center.X - currentCoord.X) * ((1 - Sensitivity) / 2) + currentCoord.X;
                //var destCoordY = (extent.Center.Y - currentCoord.Y) * ((1 - Sensitivity) / 2) + currentCoord.Y;
                //destExtent.SetCenter(new Coordinate(destCoordX, destCoordY));
                //Rectangle rect = e.Map.MapFrame.ProjToBuffer(destExtent);
                //r = rect;
            }
            else
            {
                double ratio = Sensitivity / (2 * (1 - Sensitivity));
                double dHalfWidth = w * ratio;
                double dHalfHeight = h * ratio;
                r.Inflate(Convert.ToInt32(dHalfWidth), Convert.ToInt32(dHalfHeight));
                double ratioWidth = r.Width / w;
                double ratioHeight = r.Height / h;
                double destX = e.X * ratioWidth + (1 - ratioWidth) * srcCenterX;
                double destY = e.Y * ratioHeight + (1 - ratioHeight) * srcCenterY;
                int xOff = Convert.ToInt32(e.X - destX);
                int yOff = Convert.ToInt32(e.Y - destY);
                r.X += xOff;
                r.Y += yOff;
            }

            e.Map.MapFrame.View = r;
            e.Map.Invalidate();
            _zoomTimer.Start();
            _mapFrame = e.Map.MapFrame;
            if (!BusySet)
            {
                Map.IsBusy = true;
                BusySet = true;
            }

            base.OnMouseWheel(e);
        }

        private void Configure()
        {
            YieldStyle = YieldStyles.Scroll;
            _timerInterval = 100;
            _zoomTimer = new Timer
            {
                Interval = _timerInterval
            };
            _zoomTimer.Tick += ZoomTimerTick;
            _client = Rectangle.Empty;
            Sensitivity = 0.2;
            ForwardZoomsIn = true;
            Name = "ScrollZoom";
        }

        private void ZoomTimerTick(object sender, EventArgs e)
        {
            _zoomTimer.Stop();
            if (_mapFrame == null) return;
            _client = Rectangle.Empty;
            _mapFrame.ResetExtents();
            Map.IsBusy = false;
            BusySet = false;
        }

        #endregion
    }
}