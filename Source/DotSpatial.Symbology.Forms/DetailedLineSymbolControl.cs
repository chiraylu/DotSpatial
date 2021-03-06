// Copyright (c) DotSpatial Team. All rights reserved.
// Licensed under the MIT license. See License.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using DotSpatial.Data;
using DotSpatial.Serialization;

namespace DotSpatial.Symbology.Forms
{
    /// <summary>
    /// DetailedLineSymbolDialog
    /// </summary>
    public partial class DetailedLineSymbolControl : UserControl
    {
        #region Fields

        private bool _ignoreChanges;
        private ILineSymbolizer _original;
        private ILineSymbolizer _symbolizer;

        #endregion

        #region  Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DetailedLineSymbolControl"/> class.
        /// </summary>
        public DetailedLineSymbolControl()
        {
            _original = new LineSymbolizer();
            _symbolizer = new LineSymbolizer();
            Configure();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DetailedLineSymbolControl"/> class that displays a copy of the original,
        /// and when apply changes is pressed, will copy properties to the original.
        /// </summary>
        /// <param name="original">The current symbolizer being viewed on the map</param>
        public DetailedLineSymbolControl(ILineSymbolizer original)
        {
            _original = original;
            _symbolizer = original.Copy();
            Configure();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DetailedLineSymbolControl"/> class.
        /// This constructor shows a different symbolizer in the view from what is currently loaded on the map.
        /// If apply changes is clicked, the properties of the current symbolizer will be copied to the original.
        /// </summary>
        /// <param name="original">The symbolizer on the map</param>
        /// <param name="displayed">The symbolizer that defines the form setup</param>
        public DetailedLineSymbolControl(ILineSymbolizer original, ILineSymbolizer displayed)
        {
            _symbolizer = displayed;
            _original = original;
            Configure();
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the user clicks the AddToCustomSymbols button
        /// </summary>
        public event EventHandler<LineSymbolizerEventArgs> AddToCustomSymbols;

        /// <summary>
        /// Fires an event indicating that changes should be applied.
        /// </summary>
        public event EventHandler ChangesApplied;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the current (copied) symbolizer or initializes this control to work with the
        /// specified symbolizer as the original.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ILineSymbolizer Symbolizer
        {
            get
            {
                return _symbolizer;
            }

            set
            {
                if (value != null) Initialize(value);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Forces the original to apply the changes to the new control.
        /// </summary>
        public void ApplyChanges()
        {
            OnApplyChanges();
        }

        /// <summary>
        /// Initializes the control by updating the symbolizer.
        /// </summary>
        /// <param name="original">The original symbolizer.</param>
        public void Initialize(ILineSymbolizer original)
        {
            _original = original;
            _symbolizer = original.Copy();
            ccStrokes.Strokes = _symbolizer.Strokes;
            chkSmoothing.Checked = _symbolizer.Smoothing;
            ccStrokes.RefreshList();
            if (_symbolizer.Strokes.Count > 0)
            {
                ccStrokes.SelectedStroke = _symbolizer.Strokes[0];
            }

            UpdatePreview();
            UpdateStrokeControls();
        }

        /// <summary>
        /// Fires the AddtoCustomSymbols event.
        /// </summary>
        protected virtual void OnAddToCustomSymbols()
        {
            AddToCustomSymbols?.Invoke(this, new LineSymbolizerEventArgs(_symbolizer));
        }

        /// <summary>
        /// Fires the ChangesApplied event
        /// </summary>
        protected void OnApplyChanges()
        {
            _original.CopyProperties(_symbolizer);
            ChangesApplied?.Invoke(this, EventArgs.Empty);
        }

        private void BtnEditClick(object sender, EventArgs e)
        {
            ILineDecoration dec = ccDecorations.SelectedDecoration;
            if (dec == null) return;
            DetailedPointSymbolDialog dpd = new DetailedPointSymbolDialog(dec.Symbol);
            dpd.ChangesApplied += DpdChangesApplied;
            dpd.ShowDialog();
        }

        private void CbColorCartographicColorChanged(object sender, EventArgs e)
        {
            SetColor(cbColorCartographic.Color);
        }

        private void CbColorSimpleColorChanged(object sender, EventArgs e)
        {
            SetColor(cbColorSimple.Color);
        }

        private void CcDecorationsAddClicked(object sender, EventArgs e)
        {
            ICartographicStroke currentStroke = ccStrokes.SelectedStroke as ICartographicStroke;
            if (currentStroke != null)
            {
                LineDecoration decoration = new LineDecoration();
                currentStroke.Decorations.Add(decoration);
                ccDecorations.RefreshList();
                ccDecorations.SelectedDecoration = decoration;
            }
        }

        private void CcDecorationsListChanged(object sender, EventArgs e)
        {
            UpdatePreview();
        }

        private void CcDecorationsSelectedItemChanged(object sender, EventArgs e)
        {
            if (ccDecorations.SelectedDecoration != null)
            {
                UpdateDecorationControls();
            }

            UpdatePreview();
        }

        private void CcStrokesAddClicked(object sender, EventArgs e)
        {
            StrokeStyle strokeStyle = Global.ParseEnum<StrokeStyle>(cmbStrokeType.SelectedIndex);
            switch (strokeStyle)
            {
                case StrokeStyle.Simple:
                    _symbolizer.Strokes.Add(new SimpleStroke());
                    break;
                case StrokeStyle.Cartographic:
                    _symbolizer.Strokes.Add(new CartographicStroke());
                    break;
                case StrokeStyle.Marker:
                    _symbolizer.Strokes.Add(new MarkerStroke());
                    break;
            }
            ccStrokes.RefreshList();
            UpdatePreview();
        }

        private void CcStrokesListChanged(object sender, EventArgs e)
        {
            UpdatePreview();
        }

        private void CcStrokesSelectedItemChanged(object sender, EventArgs e)
        {
            if (ccStrokes.SelectedStroke != null)
            {
                UpdateStrokeControls();
            }

            UpdatePreview();
        }

        /// <summary>
        /// Update the selected linedecoration: set whether all decorations should be flipped.
        /// </summary>
        /// <param name="sender">Sender that raised the event.</param>
        /// <param name="e">The event args.</param>
        private void ChkFlipAllCheckedChanged(object sender, EventArgs e)
        {
            if (_ignoreChanges) return;
            ILineDecoration dec = ccDecorations.SelectedDecoration;
            if (dec != null)
            {
                dec.FlipAll = chkFlipAll.Checked;
            }

            UpdatePreview();
        }

        /// <summary>
        /// Update the selected linedecoration: set whether the first decoration should be flipped.
        /// </summary>
        /// <param name="sender">Sender that raised the event.</param>
        /// <param name="e">The event args.</param>
        private void ChkFlipFirstCheckedChanged(object sender, EventArgs e)
        {
            if (_ignoreChanges) return;
            ILineDecoration dec = ccDecorations.SelectedDecoration;
            if (dec != null)
            {
                dec.FlipFirst = chkFlipFirst.Checked;
            }

            UpdatePreview();
        }

        private void ChkSmoothingCheckedChanged(object sender, EventArgs e)
        {
            _symbolizer.Smoothing = chkSmoothing.Checked;
            UpdatePreview();
        }

        private void CmbEndCapSelectedIndexChanged(object sender, EventArgs e)
        {
            if (_ignoreChanges) return;
            if (ccStrokes.SelectedStroke == null) return;
            IStroke stroke = ccStrokes.SelectedStroke;
            ICartographicStroke cs = stroke as ICartographicStroke;
            if (cs != null && cmbEndCap.SelectedIndex != -1)
            {
                cs.EndCap = (LineCap)cmbEndCap.SelectedItem;
            }

            UpdatePreview();
        }

        private void CmbScaleModeSelectedIndexChanged(object sender, EventArgs e)
        {
            if (_ignoreChanges) return;
            ScaleMode scaleMode = Global.ParseEnum<ScaleMode>(cmbScaleMode.SelectedIndex);
            _symbolizer.ScaleMode = scaleMode;
        }

        private void CmbStartCapSelectedIndexChanged(object sender, EventArgs e)
        {
            if (_ignoreChanges) return;
            if (ccStrokes.SelectedStroke == null) return;
            IStroke stroke = ccStrokes.SelectedStroke;
            ICartographicStroke cs = stroke as ICartographicStroke;
            if (cs != null && cmbStartCap.SelectedIndex != -1)
            {
                cs.StartCap = (LineCap)cmbStartCap.SelectedItem;
            }

            UpdatePreview();
        }

        private void CmbStrokeTypeSelectedIndexChanged(object sender, EventArgs e)
        {
            if (_ignoreChanges) return;
            var oldStroke = ccStrokes.SelectedStroke;
            var oldStrokeStyle = oldStroke.StrokeStyle;
            int index = ccStrokes.Strokes.IndexOf(ccStrokes.SelectedStroke);
            if (index == -1) return;
            StrokeStyle strokeStyle = Global.ParseEnum<StrokeStyle>(cmbStrokeType.SelectedIndex);
            IStroke newStroke= null;
            switch (strokeStyle)
            {
                case StrokeStyle.Cartographic:
                    newStroke = new CartographicStroke();
                    break;
                case StrokeStyle.Simple:
                    newStroke = new SimpleStroke();
                    break;
                case StrokeStyle.Marker:
                    newStroke = new MarkerStroke();
                    break;
                default:
                    cmbStrokeType.SelectedIndex = Global.GetEnumIndex(oldStrokeStyle);
                    MessageBox.Show("暂未实现");
                    return;
            }
            StrokeStyle newStrokeStyle = newStroke.StrokeStyle;
            newStroke.CopyProperties(oldStroke);
            ccStrokes.Strokes[index] = newStroke;
            ccStrokes.RefreshList();
            ccStrokes.SelectedStroke = newStroke;
        }
        private void CopyProperties(ISimpleStroke stroke, ISimpleStroke other)
        {
            if (stroke == null || other == null)
            {
                if (stroke.StrokeStyle == other.StrokeStyle)
                {
                    stroke.CopyProperties(other);
                }
                else
                {
                    stroke.Color = other.Color;
                    stroke.DashStyle = other.DashStyle;
                    stroke.Opacity = other.Opacity;
                    stroke.Width = other.Width;
                }
            }
        }
        private void CopyProperties(ICartographicStroke stroke, ICartographicStroke other)
        {
            if (stroke == null || other == null)
            {
                if (stroke.StrokeStyle == other.StrokeStyle)
                {
                    stroke.CopyProperties(other);
                }
                else
                {
                    CopyProperties(stroke as ISimpleStroke, other as ISimpleStroke);
                    stroke.Color = other.Color;
                    stroke.DashStyle = other.DashStyle;
                    stroke.Opacity = other.Opacity;
                    stroke.Width = other.Width;

                    stroke.CompoundArray = other.CompoundArray.Copy();
                    stroke.CompoundButtons = other.CompoundButtons.Copy();
                    stroke.DashButtons = other.DashButtons.Copy();
                    stroke.DashCap = other.DashCap;
                    stroke.DashPattern = other.DashPattern.Copy();
                    stroke.Decorations = new List<ILineDecoration>(other.Decorations);
                    stroke.EndCap = other.EndCap;
                    stroke.JoinType = other.JoinType;
                    stroke.Offset = other.Offset;
                    stroke.StartCap = other.StartCap;
                }
            }
        }
        private void CopyProperties(IStroke stroke, IStroke other)
        {
            if (stroke == null || other == null)
            {
                if (stroke.StrokeStyle == other.StrokeStyle)
                {
                    stroke.CopyProperties(other);
                }
                else
                {
                    switch (stroke.StrokeStyle)
                    {
                        case StrokeStyle.Simple:
                            ISimpleStroke simpleStroke = stroke as ISimpleStroke;
                            if (other is ISimpleStroke otherSimpleStroke)
                            {
                                CopyProperties(simpleStroke, otherSimpleStroke);
                            }
                            break;
                        case StrokeStyle.Cartographic:
                            break;
                        case StrokeStyle.Marker:
                            break;
                        default: // notimplement
                            break;

                    }
                }
            }
        }
        private void ComboBox1SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_ignoreChanges) return;
            if (ccStrokes.SelectedStroke == null) return;
            ISimpleStroke ss = ccStrokes.SelectedStroke as ISimpleStroke;
            if (ss != null) ss.DashStyle = (DashStyle)cmbStrokeStyle.SelectedIndex;
            UpdatePreview();
        }

        private void Configure()
        {
            InitializeComponent();

            ccStrokes.Strokes = _symbolizer.Strokes;
            ccStrokes.AddClicked += CcStrokesAddClicked;
            ccStrokes.SelectedItemChanged += CcStrokesSelectedItemChanged;
            ccStrokes.ListChanged += CcStrokesListChanged;

            ccDecorations.AddClicked += CcDecorationsAddClicked;
            ccDecorations.SelectedItemChanged += CcDecorationsSelectedItemChanged;
            ccDecorations.ListChanged += CcDecorationsListChanged;

            lblPreview.Paint += LblPreviewPaint;
            lblDecorationPreview.Paint += LblDecorationPreviewPaint;
            dashControl1.PatternChanged += DashControl1PatternChanged;
            dblWidthCartographic.TextChanged += DblWidthCartographicTextChanged;
            cbColorCartographic.ColorChanged += CbColorCartographicColorChanged;
            cbColorSimple.ColorChanged += CbColorSimpleColorChanged;

            cmbScaleMode.SelectedIndex = Global.GetEnumIndex(_symbolizer.ScaleMode);
            chkSmoothing.Checked = _symbolizer.Smoothing;
            dblOffset.TextChanged += DblOffsetTextChanged;
            dblStrokeOffset.TextChanged += DblStrokeOffsetTextChanged;
            sldOpacitySimple.ValueChanged += SldOpacitySimpleValueChanged;
            sldOpacityCartographic.ValueChanged += SldOpacityCartographicValueChanged;
            if (_symbolizer?.Strokes != null && _symbolizer.Strokes.Count > 0)
            {
                ccStrokes.SelectedStroke = _symbolizer.Strokes[0];
            }
        }

        private void DashControl1PatternChanged(object sender, EventArgs e)
        {
            if (_ignoreChanges) return;
            IStroke stroke = ccStrokes.SelectedStroke;
            if (stroke == null) return;
            ICartographicStroke cs = stroke as ICartographicStroke;
            if (cs != null)
            {
                cs.DashStyle = DashStyle.Custom;
                cs.DashPattern = dashControl1.GetDashPattern();
                cs.DashButtons = dashControl1.DashButtons;
                cs.CompoundButtons = dashControl1.CompoundButtons;
                cs.CompoundArray = dashControl1.GetCompoundArray();
                //cs.Width = cs.CompoundButtons.Length;
            }

            UpdatePreview();
        }

        private void DblOffsetTextChanged(object sender, EventArgs e)
        {
            if (ccDecorations.SelectedDecoration == null) return;
            ccDecorations.SelectedDecoration.Offset = dblOffset.Value;
            UpdatePreview();
        }

        private void DblStrokeOffsetTextChanged(object sender, EventArgs e)
        {
            ICartographicStroke cs = ccStrokes?.SelectedStroke as ICartographicStroke;
            if (cs == null) return;
            cs.Offset = (float)dblStrokeOffset.Value;
            UpdatePreview();
        }

        private void DblWidthTextChanged(object sender, EventArgs e)
        {
            UpdateWidth(dblWidth.Value);
        }

        private void DblWidthCartographicTextChanged(object sender, EventArgs e)
        {
            UpdateWidth(dblWidthCartographic.Value);
        }

        private void DpdChangesApplied(object sender, EventArgs e)
        {
            UpdatePreview();
            ccDecorations.Refresh();
            lblDecorationPreview.Refresh();
        }

        private void LblDecorationPreviewPaint(object sender, PaintEventArgs e)
        {
            UpdateDecorationPreview(e.Graphics);
        }

        private void LblPreviewPaint(object sender, PaintEventArgs e)
        {
            UpdatePreview(e.Graphics);
        }

        private void LoadLineCaps()
        {
            Array names = Enum.GetValues(typeof(LineCap));
            foreach (object name in names)
            {
                cmbEndCap.Items.Add(name);
                cmbStartCap.Items.Add(name);
            }
        }

        private void NudDecorationCountValueChanged(object sender, EventArgs e)
        {
            nudPercentualPosition.Visible = nudDecorationCount.Value == 1;
            lblPercentualPosition.Visible = nudDecorationCount.Value == 1;
            if (_ignoreChanges) return;
            ILineDecoration dec = ccDecorations.SelectedDecoration;
            if (dec != null)
            {
                dec.NumSymbols = (int)nudDecorationCount.Value;
            }

            UpdatePreview();
        }

        /// <summary>
        /// Update the selected linedecoration: set the percentual position between line start and end of the single decoration.
        /// </summary>
        /// <param name="sender">Sender that raised the event.</param>
        /// <param name="e">The event args.</param>
        private void NudPercentualPositionValueChanged(object sender, EventArgs e)
        {
            if (_ignoreChanges) return;
            ILineDecoration dec = ccDecorations.SelectedDecoration;
            if (dec != null)
            {
                dec.PercentualPosition = (int)nudPercentualPosition.Value;
            }

            UpdatePreview();
        }

        private void RadLineJoinValueChanged(object sender, EventArgs e)
        {
            if (_ignoreChanges) return;
            if (ccStrokes.SelectedStroke == null) return;
            IStroke stroke = ccStrokes.SelectedStroke;
            ICartographicStroke cs = stroke as ICartographicStroke;
            if (cs != null)
            {
                cs.JoinType = radLineJoin.Value;
            }

            UpdatePreview();
        }

        private void RadRotationWithLineCheckedChanged(object sender, EventArgs e)
        {
            if (_ignoreChanges) return;
            ILineDecoration dec = ccDecorations.SelectedDecoration;
            if (dec != null)
            {
                dec.RotateWithLine = radRotationWithLine.Checked;
            }

            UpdatePreview();
        }

        private void SetColor(Color color)
        {
            if (_ignoreChanges) return;
            IStroke stroke = ccStrokes.SelectedStroke;
            if (stroke == null) return;
            ISimpleStroke ss = stroke as ISimpleStroke;
            if (ss != null)
            {
                ss.Color = color;
            }

            ICartographicStroke cs = stroke as ICartographicStroke;
            if (cs != null)
            {
                dashControl1.LineColor = color;
            }

            // only call if changed, or we will create an infinite loop here
            if (cbColorSimple.Color != color) cbColorSimple.Color = color;
            if (cbColorCartographic.Color != color) cbColorCartographic.Color = color;
            sldOpacityCartographic.MaximumColor = Color.FromArgb(255, color);
            sldOpacitySimple.MaximumColor = Color.FromArgb(255, color);
            sldOpacityCartographic.Invalidate();
            sldOpacitySimple.Invalidate();
            UpdatePreview();
        }

        private void SldOpacityCartographicValueChanged(object sender, EventArgs e)
        {
            UpdateOpacity((float)sldOpacityCartographic.Value);
        }

        private void SldOpacitySimpleValueChanged(object sender, EventArgs e)
        {
            UpdateOpacity((float)sldOpacitySimple.Value);
        }

        private void UpdateDecorationControls()
        {
            _ignoreChanges = true;
            ILineDecoration decoration = ccDecorations.SelectedDecoration;
            if (decoration != null)
            {
                chkFlipAll.Checked = decoration.FlipAll;
                chkFlipFirst.Checked = decoration.FlipFirst;
                if (decoration.RotateWithLine)
                {
                    radRotationWithLine.Checked = true;
                }
                else
                {
                    radRotationFixed.Checked = true;
                }

                nudDecorationCount.Value = decoration.NumSymbols;
                nudPercentualPosition.Value = decoration.PercentualPosition;
                nudPercentualPosition.Visible = decoration.NumSymbols == 1;
                lblPercentualPosition.Visible = decoration.NumSymbols == 1;
                dblOffset.Value = decoration.Offset;
            }

            _ignoreChanges = false;
        }

        private void UpdateDecorationPreview(Graphics g)
        {
            ILineDecoration dec = ccDecorations.SelectedDecoration;
            dec?.Symbol.Draw(g, lblDecorationPreview.ClientRectangle);
        }

        /// <summary>
        /// Updates the opacity of the simple/cartographic stroke
        /// </summary>
        /// <param name="value">THe floating point value to use for the opacity, where 0 is transparent and 1 is opaque</param>
        private void UpdateOpacity(float value)
        {
            if (_ignoreChanges) return;
            IStroke stroke = ccStrokes.SelectedStroke;
            ISimpleStroke ss = stroke as ISimpleStroke;
            if (ss != null)
            {
                if (ss.Opacity != value)
                {
                    ss.Opacity = value;
                    cbColorSimple.Color = ss.Color;
                    cbColorCartographic.Color = ss.Color;
                }
            }
        }

        private void UpdatePreview()
        {
            using (Graphics g = lblPreview.CreateGraphics())
            {
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                UpdatePreview(g);
            }

            ICartographicStroke cs = ccStrokes.SelectedStroke as ICartographicStroke;
            if (cs != null)
            {
                using (Graphics g = lblDecorationPreview.CreateGraphics())
                {
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    UpdateDecorationPreview(g);
                }
            }
        }

        private void UpdatePreview(Graphics g)
        {
            g.FillRectangle(Brushes.White, new Rectangle(0, 0, lblPreview.Width, lblPreview.Height));
            GraphicsPath gp = new GraphicsPath();
            gp.AddLine(10, lblPreview.Height / 2, lblPreview.Width - 20, lblPreview.Height / 2);
            foreach (IStroke stroke in _symbolizer.Strokes)
            {
                stroke.DrawPath(g, gp, 1);
            }

            gp.Dispose();

            ccStrokes.Refresh();
        }

        /// <summary>
        /// When the stroke is changed, this updates the controls to match it.
        /// </summary>
        private void UpdateStrokeControls()
        {
            _ignoreChanges = true;
            IStroke stroke = ccStrokes.SelectedStroke;
            cmbStrokeType.SelectedIndex = Global.GetEnumIndex(stroke.StrokeStyle);
            switch (stroke.StrokeStyle)
            {
                case StrokeStyle.Cartographic:
                    ICartographicStroke cs = ccStrokes.SelectedStroke as ICartographicStroke;
                    if (tabStrokeProperties.TabPages.Contains(tabSimple))
                    {
                        tabStrokeProperties.TabPages.Remove(tabSimple);
                    }
                    if (tabStrokeProperties.TabPages.Contains(tabMarker))
                    {
                        tabStrokeProperties.TabPages.Remove(tabMarker);
                    }

                    if (tabStrokeProperties.TabPages.Contains(tabCartographic) == false)
                    {
                        tabStrokeProperties.TabPages.Add(tabCartographic);
                    }

                    if (tabStrokeProperties.TabPages.Contains(tabDash) == false)
                    {
                        tabStrokeProperties.TabPages.Add(tabDash);
                    }

                    if (tabStrokeProperties.TabPages.Contains(tabDecoration) == false)
                    {
                        tabStrokeProperties.TabPages.Add(tabDecoration);
                    }

                    // Cartographic Tab Page
                    if (cmbStartCap.Items.Count == 0)
                    {
                        LoadLineCaps();
                    }

                    cmbStartCap.SelectedItem = cs.StartCap;
                    cmbEndCap.SelectedItem = cs.EndCap;
                    radLineJoin.Value = cs.JoinType;
                    dblStrokeOffset.Value = cs.Offset;

                    // Template Tab Page
                    dashControl1.SetPattern(cs);
                    dashControl1.Invalidate();

                    // Decoration Tab Page
                    ccDecorations.Decorations = cs.Decorations;
                    if (cs.Decorations != null && cs.Decorations.Count > 0)
                    {
                        ccDecorations.SelectedDecoration = cs.Decorations[0];
                    }
                    break;
                case StrokeStyle.Simple:
                    if (tabStrokeProperties.TabPages.Contains(tabMarker))
                    {
                        tabStrokeProperties.TabPages.Remove(tabMarker);
                    }
                    if (tabStrokeProperties.TabPages.Contains(tabDash))
                    {
                        tabStrokeProperties.TabPages.Remove(tabDash);
                    }

                    if (tabStrokeProperties.TabPages.Contains(tabCartographic))
                    {
                        tabStrokeProperties.TabPages.Remove(tabCartographic);
                    }

                    if (tabStrokeProperties.TabPages.Contains(tabDecoration))
                    {
                        tabStrokeProperties.TabPages.Remove(tabDecoration);
                    }

                    if (tabStrokeProperties.TabPages.Contains(tabSimple) == false)
                    {
                        tabStrokeProperties.TabPages.Add(tabSimple);
                    }
                    break;
                case StrokeStyle.Marker:
                    IMarkerStroke ms = ccStrokes.SelectedStroke as IMarkerStroke;
                    if (tabStrokeProperties.TabPages.Contains(tabSimple))
                    {
                        tabStrokeProperties.TabPages.Remove(tabSimple);
                    }
                    if (!tabStrokeProperties.TabPages.Contains(tabMarker))
                    {
                        tabStrokeProperties.TabPages.Insert(0,tabMarker);
                    }

                    if (!tabStrokeProperties.TabPages.Contains(tabCartographic))
                    {
                        tabStrokeProperties.TabPages.Add(tabCartographic);
                    }

                    if (!tabStrokeProperties.TabPages.Contains(tabDash))
                    {
                        tabStrokeProperties.TabPages.Add(tabDash);
                    }

                    if (!tabStrokeProperties.TabPages.Contains(tabDecoration))
                    {
                        tabStrokeProperties.TabPages.Add(tabDecoration);
                    }

                    // Cartographic Tab Page
                    if (cmbStartCap.Items.Count == 0)
                    {
                        LoadLineCaps();
                    }

                    cmbStartCap.SelectedItem = ms.StartCap;
                    cmbEndCap.SelectedItem = ms.EndCap;
                    radLineJoin.Value = ms.JoinType;
                    dblStrokeOffset.Value = ms.Offset;

                    // Template Tab Page
                    dashControl1.SetPattern(ms);
                    dashControl1.Invalidate();

                    // Decoration Tab Page
                    ccDecorations.Decorations = ms.Decorations;
                    if (ms.Decorations != null && ms.Decorations.Count > 0)
                    {
                        ccDecorations.SelectedDecoration = ms.Decorations[0];
                    }
                    break;
            }

            ISimpleStroke ss = ccStrokes.SelectedStroke as ISimpleStroke;
            if (ss != null)
            {
                // Simple Tab Page
                cbColorSimple.Color = ss.Color;
                cbColorCartographic.Color = ss.Color;
                dblWidthCartographic.Value = ss.Width;
                dblWidth.Value = ss.Width;
                cmbStrokeStyle.SelectedIndex = (int)ss.DashStyle;
                sldOpacityCartographic.MaximumColor = Color.FromArgb(255, ss.Color.R, ss.Color.G, ss.Color.B);
                sldOpacitySimple.MaximumColor = Color.FromArgb(255, ss.Color.R, ss.Color.G, ss.Color.B);
                sldOpacitySimple.Value = ss.Opacity;
                sldOpacityCartographic.Value = ss.Opacity;
                sldOpacityCartographic.Invalidate();
                sldOpacitySimple.Invalidate();
            }

            _ignoreChanges = false;
        }

        private void UpdateWidth(double value)
        {
            if (_ignoreChanges) return;
            IStroke stroke = ccStrokes.SelectedStroke;
            if (stroke == null) return;
            ISimpleStroke ss = stroke as ISimpleStroke;
            if (ss != null)
            {
                ss.Width = dblWidth.Value;
            }

            // only call if changed, or else we will create an infinite loop here
            if (dblWidth.Value != value) dblWidth.Value = value;
            if (dblWidthCartographic.Value != value) dblWidthCartographic.Value = value;
            UpdatePreview();
        }

        #endregion

        private void BtnMarkerClick(object sender, EventArgs e)
        {
            IStroke stroke = ccStrokes.SelectedStroke;
            switch (stroke.StrokeStyle)
            {
                case StrokeStyle.Marker:
                    IMarkerStroke markerStroke = stroke as IMarkerStroke;
                    DetailedPointSymbolDialog dpd = new DetailedPointSymbolDialog(markerStroke.Marker);
                    dpd.ChangesApplied += (x, y) => { UpdatePreview(); };
                    dpd.ShowDialog();
                    break;
                default:
                    return;
            }
        }

        private void BtnSaveSymbolClick(object sender, EventArgs e)
        {
            SerializeHelper.SaveFeatureSymbolizer(_symbolizer);
        }

        private void BtnImportSymbolClick(object sender, EventArgs e)
        {
            ILineSymbolizer ps = SerializeHelper.OpenFeatureSymbolizer() as ILineSymbolizer;
            if (ps != null)
            {
                _symbolizer = ps;
                ccStrokes.Strokes = _symbolizer.Strokes;
                ccStrokes.RefreshList();
                if (_symbolizer.Strokes.Count > 0)
                {
                    ccStrokes.SelectedStroke = _symbolizer.Strokes[0];
                }

                UpdatePreview();
                UpdateStrokeControls();
            }
            else
            {
                MessageBox.Show("Failed to open file");
            }
        }
    }
}