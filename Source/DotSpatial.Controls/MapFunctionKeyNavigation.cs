// Copyright (c) DotSpatial Team. All rights reserved.
// Licensed under the MIT license. See License.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DotSpatial.Controls
{
    /// <summary>
    /// A MapFunction that can zoom the map out based on mouse clicks.
    /// </summary>
    public class MapFunctionKeyNavigation : MapFunction
    {
        #region Fields
        private bool _isPanningTemporarily;
        private int _keyPanCount;
        private FunctionMode _previousFunctionMode = FunctionMode.None;
        private List<IMapFunction> _previousFunctions = new List<IMapFunction>();
        #endregion

        #region  Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MapFunctionKeyNavigation"/> class.
        /// </summary>
        /// <param name="inMap">The map the tool should work on.</param>
        public MapFunctionKeyNavigation(IMap inMap)
            : base(inMap)
        {
            YieldStyle = YieldStyles.AlwaysOn;
            BusySet = false;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether the map function is currently interacting with the map.
        /// </summary>
        public bool BusySet { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Handles the Key Down situation.
        /// </summary>
        /// <param name="e">The event args.</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            // Allow panning if the space is pressed.
            if (e.KeyCode == Keys.Space && !_isPanningTemporarily)
            {
                _previousFunctionMode = Map.FunctionMode;
                _previousFunctions.Clear();
                IMapFunction pan = Map.MapFunctions.FirstOrDefault(x => x is MapFunctionPan);
                foreach (var f in Map.MapFunctions)
                {
                    if (!f.Enabled || (f.YieldStyle & YieldStyles.AlwaysOn) == YieldStyles.AlwaysOn) continue; // ignore "Always On" functions
                    int test = (int)(f.YieldStyle & pan.YieldStyle);
                    if (test > 0)
                    {
                        _previousFunctions.Add(f);
                    }
                }

                Map.FunctionMode = FunctionMode.Pan;
                _isPanningTemporarily = true;
            }

            // Arrow-Key Panning
            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down || e.KeyCode == Keys.Left || e.KeyCode == Keys.Right)
            {
                if (!BusySet)
                {
                    Map.IsBusy = true;
                    BusySet = true;
                }

                var source = Map.MapFrame.View;

                switch (e.KeyCode)
                {
                    case Keys.Up:
                        Map.MapFrame.View = new Rectangle(source.X, source.Y - 20, source.Width, source.Height);
                        break;
                    case Keys.Down:
                        Map.MapFrame.View = new Rectangle(source.X, source.Y + 20, source.Width, source.Height);
                        break;
                    case Keys.Left:
                        Map.MapFrame.View = new Rectangle(source.X - 20, source.Y, source.Width, source.Height);
                        break;
                    case Keys.Right:
                        Map.MapFrame.View = new Rectangle(source.X + 20, source.Y, source.Width, source.Height);
                        break;
                }

                _keyPanCount++;

                if (_keyPanCount == 16)
                {
                    Map.MapFrame.ResetExtents();
                    _keyPanCount = 0;
                }
                else
                {
                    Map.Invalidate();
                }
            }

            // Zoom Out
            if (e.KeyCode == (Keys.LButton | Keys.MButton | Keys.Back | Keys.ShiftKey | Keys.Space | Keys.F17) || e.KeyCode == Keys.Subtract)
            {
                Map.IsBusy = true;
                Rectangle r = Map.MapFrame.View;

                r.Inflate(r.Width / 2, r.Height / 2);
                Map.MapFrame.View = r;
                Map.MapFrame.ResetExtents();
                Map.IsBusy = false;
            }

            // Zoom In
            if (e.KeyCode == (Keys.LButton | Keys.RButton | Keys.Back | Keys.ShiftKey | Keys.Space | Keys.F17) || e.KeyCode == Keys.Add)
            {
                Map.IsBusy = true;
                Rectangle r = Map.MapFrame.View;

                r.Inflate(-r.Width / 4, -r.Height / 4);

                Map.MapFrame.View = r;
                Map.MapFrame.ResetExtents();
                Map.IsBusy = false;
            }
        }
        /// <summary>
        /// Handles the Key Up situation.
        /// </summary>
        /// <param name="e">The event args.</param>
        protected override void OnKeyUp(KeyEventArgs e)
        {
            // Allow panning if the space is pressed.
            if (e.KeyCode == Keys.Space && _isPanningTemporarily)
            {
                Map.FunctionMode = _previousFunctionMode;
                if (Map.FunctionMode == FunctionMode.None && _previousFunctions.Count > 0)
                {
                    foreach (var item in _previousFunctions)
                    {
                        item.Activate();
                    }
                }
                _previousFunctions.Clear();
                _isPanningTemporarily = false;
            }

            // Arrow-Key Panning
            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down || e.KeyCode == Keys.Left || e.KeyCode == Keys.Right)
            {
                Map.MapFrame.ResetExtents();
                Map.IsBusy = false;
                BusySet = false;
                _keyPanCount = 0;
            }

            // Large Step Panning
            if (e.KeyCode == Keys.PageUp || e.KeyCode == Keys.PageDown || e.KeyCode == Keys.Home || e.KeyCode == Keys.End)
            {
                Map.IsBusy = true;
                var source = Map.MapFrame.View;

                switch (e.KeyCode)
                {
                    case Keys.PageUp:
                        Map.MapFrame.View = new Rectangle(source.X, source.Y - (int)(source.Height * 0.75), source.Width, source.Height);
                        break;
                    case Keys.PageDown:
                        Map.MapFrame.View = new Rectangle(source.X, source.Y + (int)(source.Height * 0.75), source.Width, source.Height);
                        break;
                    case Keys.Home:
                        Map.MapFrame.View = new Rectangle(source.X - (int)(source.Width * 0.75), source.Y, source.Width, source.Height);
                        break;
                    case Keys.End:
                        Map.MapFrame.View = new Rectangle(source.X + (int)(source.Width * 0.75), source.Y, source.Width, source.Height);
                        break;
                }

                Map.MapFrame.ResetExtents();
                Map.IsBusy = false;
            }
        }

        #endregion
    }
}