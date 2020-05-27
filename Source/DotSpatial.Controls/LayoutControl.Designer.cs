using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace DotSpatial.Controls
{
    partial class LayoutControl
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && SelectionPen != null)
            {
                SelectionPen.Dispose();
                SelectionPen = null;
            }
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code


        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LayoutControl));
            this._hScrollBarPanel = new System.Windows.Forms.Panel();
            this._hScrollBar = new System.Windows.Forms.HScrollBar();
            this._vScrollBar = new System.Windows.Forms.VScrollBar();
            this._contextMenuRight = new System.Windows.Forms.ContextMenu();
            this._cMnuMoveUp = new System.Windows.Forms.MenuItem();
            this._cMnuMoveDown = new System.Windows.Forms.MenuItem();
            this._menuItem2 = new System.Windows.Forms.MenuItem();
            this._cMnuSelAli = new System.Windows.Forms.MenuItem();
            this._cMnuSelLeft = new System.Windows.Forms.MenuItem();
            this._cMnuSelRight = new System.Windows.Forms.MenuItem();
            this._cMnuSelTop = new System.Windows.Forms.MenuItem();
            this._cMnuSelBottom = new System.Windows.Forms.MenuItem();
            this._cMnuSelHor = new System.Windows.Forms.MenuItem();
            this._cMnuSelVert = new System.Windows.Forms.MenuItem();
            this._cMnuMarAli = new System.Windows.Forms.MenuItem();
            this._cMnuMargLeft = new System.Windows.Forms.MenuItem();
            this._cMnuMargRight = new System.Windows.Forms.MenuItem();
            this._cMnuMargTop = new System.Windows.Forms.MenuItem();
            this._cMnuMargBottom = new System.Windows.Forms.MenuItem();
            this._cMnuMargHor = new System.Windows.Forms.MenuItem();
            this._cMnuMargVert = new System.Windows.Forms.MenuItem();
            this._menuItem19 = new System.Windows.Forms.MenuItem();
            this._cMnuSelFit = new System.Windows.Forms.MenuItem();
            this._cMnuSelWidth = new System.Windows.Forms.MenuItem();
            this._cMnuSelHeight = new System.Windows.Forms.MenuItem();
            this._cMnuMarFit = new System.Windows.Forms.MenuItem();
            this._cMnuMargWidth = new System.Windows.Forms.MenuItem();
            this._cMnuMargHeight = new System.Windows.Forms.MenuItem();
            this._menuItem4 = new System.Windows.Forms.MenuItem();
            this._cMnuDelete = new System.Windows.Forms.MenuItem();
            this._hScrollBarPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // _hScrollBarPanel
            // 
            resources.ApplyResources(this._hScrollBarPanel, "_hScrollBarPanel");
            this._hScrollBarPanel.Controls.Add(this._hScrollBar);
            this._hScrollBarPanel.Name = "_hScrollBarPanel";
            // 
            // _hScrollBar
            // 
            resources.ApplyResources(this._hScrollBar, "_hScrollBar");
            this._hScrollBar.Name = "_hScrollBar";
            this._hScrollBar.Scroll += new System.Windows.Forms.ScrollEventHandler(this.HScrollBarScroll);
            // 
            // _vScrollBar
            // 
            resources.ApplyResources(this._vScrollBar, "_vScrollBar");
            this._vScrollBar.Name = "_vScrollBar";
            this._vScrollBar.Scroll += new System.Windows.Forms.ScrollEventHandler(this.VScrollBarScroll);
            // 
            // _contextMenuRight
            // 
            this._contextMenuRight.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this._cMnuMoveUp,
            this._cMnuMoveDown,
            this._menuItem2,
            this._cMnuSelAli,
            this._cMnuMarAli,
            this._menuItem19,
            this._cMnuSelFit,
            this._cMnuMarFit,
            this._menuItem4,
            this._cMnuDelete});
            resources.ApplyResources(this._contextMenuRight, "_contextMenuRight");
            // 
            // _cMnuMoveUp
            // 
            resources.ApplyResources(this._cMnuMoveUp, "_cMnuMoveUp");
            this._cMnuMoveUp.Index = 0;
            this._cMnuMoveUp.Text = global::DotSpatial.Controls.MessageStrings.LayoutMenuStripSelectMoveUp;
            this._cMnuMoveUp.Click += new System.EventHandler(this.CMnuMoveUpClick);
            // 
            // _cMnuMoveDown
            // 
            resources.ApplyResources(this._cMnuMoveDown, "_cMnuMoveDown");
            this._cMnuMoveDown.Index = 1;
            this._cMnuMoveDown.Text = global::DotSpatial.Controls.MessageStrings.LayoutMenuStripSelectMoveDown;
            this._cMnuMoveDown.Click += new System.EventHandler(this.CMnuMoveDownClick);
            // 
            // _menuItem2
            // 
            resources.ApplyResources(this._menuItem2, "_menuItem2");
            this._menuItem2.Index = 2;
            // 
            // _cMnuSelAli
            // 
            resources.ApplyResources(this._cMnuSelAli, "_cMnuSelAli");
            this._cMnuSelAli.Index = 3;
            this._cMnuSelAli.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this._cMnuSelLeft,
            this._cMnuSelRight,
            this._cMnuSelTop,
            this._cMnuSelBottom,
            this._cMnuSelHor,
            this._cMnuSelVert});
            this._cMnuSelAli.Text = global::DotSpatial.Controls.MessageStrings.LayoutCmnuSelectionAlignment;
            // 
            // _cMnuSelLeft
            // 
            resources.ApplyResources(this._cMnuSelLeft, "_cMnuSelLeft");
            this._cMnuSelLeft.Index = 0;
            this._cMnuSelLeft.Text = global::DotSpatial.Controls.MessageStrings.LayoutCmnuLeft;
            this._cMnuSelLeft.Click += new System.EventHandler(this.CMnuSelLeftClick);
            // 
            // _cMnuSelRight
            // 
            resources.ApplyResources(this._cMnuSelRight, "_cMnuSelRight");
            this._cMnuSelRight.Index = 1;
            this._cMnuSelRight.Text = global::DotSpatial.Controls.MessageStrings.LayoutCmnuRight;
            this._cMnuSelRight.Click += new System.EventHandler(this.CMnuSelRightClick);
            // 
            // _cMnuSelTop
            // 
            resources.ApplyResources(this._cMnuSelTop, "_cMnuSelTop");
            this._cMnuSelTop.Index = 2;
            this._cMnuSelTop.Text = global::DotSpatial.Controls.MessageStrings.LayoutCmnuTop;
            this._cMnuSelTop.Click += new System.EventHandler(this.CMnuSelTopClick);
            // 
            // _cMnuSelBottom
            // 
            resources.ApplyResources(this._cMnuSelBottom, "_cMnuSelBottom");
            this._cMnuSelBottom.Index = 3;
            this._cMnuSelBottom.Text = global::DotSpatial.Controls.MessageStrings.LayoutCmnuBottom;
            this._cMnuSelBottom.Click += new System.EventHandler(this.CMnuSelBottomClick);
            // 
            // _cMnuSelHor
            // 
            resources.ApplyResources(this._cMnuSelHor, "_cMnuSelHor");
            this._cMnuSelHor.Index = 4;
            this._cMnuSelHor.Text = global::DotSpatial.Controls.MessageStrings.LayoutCmnuHor;
            this._cMnuSelHor.Click += new System.EventHandler(this.CMnuSelHorClick);
            // 
            // _cMnuSelVert
            // 
            resources.ApplyResources(this._cMnuSelVert, "_cMnuSelVert");
            this._cMnuSelVert.Index = 5;
            this._cMnuSelVert.Text = global::DotSpatial.Controls.MessageStrings.LayoutCmnuVert;
            this._cMnuSelVert.Click += new System.EventHandler(this.CMnuSelVertClick);
            // 
            // _cMnuMarAli
            // 
            resources.ApplyResources(this._cMnuMarAli, "_cMnuMarAli");
            this._cMnuMarAli.Index = 4;
            this._cMnuMarAli.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this._cMnuMargLeft,
            this._cMnuMargRight,
            this._cMnuMargTop,
            this._cMnuMargBottom,
            this._cMnuMargHor,
            this._cMnuMargVert});
            this._cMnuMarAli.Text = global::DotSpatial.Controls.MessageStrings.LayoutCmnuMargAlign;
            // 
            // _cMnuMargLeft
            // 
            resources.ApplyResources(this._cMnuMargLeft, "_cMnuMargLeft");
            this._cMnuMargLeft.Index = 0;
            this._cMnuMargLeft.Text = global::DotSpatial.Controls.MessageStrings.LayoutCmnuLeft;
            this._cMnuMargLeft.Click += new System.EventHandler(this.CMnuMargLeftClick);
            // 
            // _cMnuMargRight
            // 
            resources.ApplyResources(this._cMnuMargRight, "_cMnuMargRight");
            this._cMnuMargRight.Index = 1;
            this._cMnuMargRight.Text = global::DotSpatial.Controls.MessageStrings.LayoutCmnuRight;
            this._cMnuMargRight.Click += new System.EventHandler(this.CMnuMargRightClick);
            // 
            // _cMnuMargTop
            // 
            resources.ApplyResources(this._cMnuMargTop, "_cMnuMargTop");
            this._cMnuMargTop.Index = 2;
            this._cMnuMargTop.Text = global::DotSpatial.Controls.MessageStrings.LayoutCmnuTop;
            this._cMnuMargTop.Click += new System.EventHandler(this.CMnuMargTopClick);
            // 
            // _cMnuMargBottom
            // 
            resources.ApplyResources(this._cMnuMargBottom, "_cMnuMargBottom");
            this._cMnuMargBottom.Index = 3;
            this._cMnuMargBottom.Text = global::DotSpatial.Controls.MessageStrings.LayoutCmnuBottom;
            this._cMnuMargBottom.Click += new System.EventHandler(this.CMnuMargBottomClick);
            // 
            // _cMnuMargHor
            // 
            resources.ApplyResources(this._cMnuMargHor, "_cMnuMargHor");
            this._cMnuMargHor.Index = 4;
            this._cMnuMargHor.Text = global::DotSpatial.Controls.MessageStrings.LayoutCmnuHor;
            this._cMnuMargHor.Click += new System.EventHandler(this.CMnuMargHorClick);
            // 
            // _cMnuMargVert
            // 
            resources.ApplyResources(this._cMnuMargVert, "_cMnuMargVert");
            this._cMnuMargVert.Index = 5;
            this._cMnuMargVert.Text = global::DotSpatial.Controls.MessageStrings.LayoutCmnuVert;
            this._cMnuMargVert.Click += new System.EventHandler(this.CMnuMargVertClick);
            // 
            // _menuItem19
            // 
            resources.ApplyResources(this._menuItem19, "_menuItem19");
            this._menuItem19.Index = 5;
            // 
            // _cMnuSelFit
            // 
            resources.ApplyResources(this._cMnuSelFit, "_cMnuSelFit");
            this._cMnuSelFit.Index = 6;
            this._cMnuSelFit.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this._cMnuSelWidth,
            this._cMnuSelHeight});
            this._cMnuSelFit.Text = global::DotSpatial.Controls.MessageStrings.LayoutCmnuSelectionFit;
            // 
            // _cMnuSelWidth
            // 
            resources.ApplyResources(this._cMnuSelWidth, "_cMnuSelWidth");
            this._cMnuSelWidth.Index = 0;
            this._cMnuSelWidth.Text = global::DotSpatial.Controls.MessageStrings.LayoutCmnuWidth;
            this._cMnuSelWidth.Click += new System.EventHandler(this.CMnuSelWidthClick);
            // 
            // _cMnuSelHeight
            // 
            resources.ApplyResources(this._cMnuSelHeight, "_cMnuSelHeight");
            this._cMnuSelHeight.Index = 1;
            this._cMnuSelHeight.Text = global::DotSpatial.Controls.MessageStrings.LayoutCmnuHeight;
            this._cMnuSelHeight.Click += new System.EventHandler(this.CMnuSelHeightClick);
            // 
            // _cMnuMarFit
            // 
            resources.ApplyResources(this._cMnuMarFit, "_cMnuMarFit");
            this._cMnuMarFit.Index = 7;
            this._cMnuMarFit.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this._cMnuMargWidth,
            this._cMnuMargHeight});
            this._cMnuMarFit.Text = global::DotSpatial.Controls.MessageStrings.LayoutCmnuMarginFit;
            // 
            // _cMnuMargWidth
            // 
            resources.ApplyResources(this._cMnuMargWidth, "_cMnuMargWidth");
            this._cMnuMargWidth.Index = 0;
            this._cMnuMargWidth.Text = global::DotSpatial.Controls.MessageStrings.LayoutCmnuWidth;
            this._cMnuMargWidth.Click += new System.EventHandler(this.CMnuMargWidthClick);
            // 
            // _cMnuMargHeight
            // 
            resources.ApplyResources(this._cMnuMargHeight, "_cMnuMargHeight");
            this._cMnuMargHeight.Index = 1;
            this._cMnuMargHeight.Text = global::DotSpatial.Controls.MessageStrings.LayoutCmnuHeight;
            this._cMnuMargHeight.Click += new System.EventHandler(this.CMnuMargHeightClick);
            // 
            // _menuItem4
            // 
            resources.ApplyResources(this._menuItem4, "_menuItem4");
            this._menuItem4.Index = 8;
            // 
            // _cMnuDelete
            // 
            resources.ApplyResources(this._cMnuDelete, "_cMnuDelete");
            this._cMnuDelete.Index = 9;
            this._cMnuDelete.Text = global::DotSpatial.Controls.MessageStrings.LayoutMenuStripSelectDelete;
            this._cMnuDelete.Click += new System.EventHandler(this.CMnuDeleteClick);
            // 
            // LayoutControl
            // 
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this._vScrollBar);
            this.Controls.Add(this._hScrollBarPanel);
            this.Name = "LayoutControl";
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.LayoutControlKeyUp);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.LayoutControlMouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.LayoutControlMouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.LayoutControlMouseUp);
            this.Resize += new System.EventHandler(this.LayoutControlResize);
            this._hScrollBarPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion Windows Form Designer generated code

        private MenuItem _cMnuDelete;
        private MenuItem _cMnuMarAli;
        private MenuItem _cMnuMarFit;
        private MenuItem _cMnuMargBottom;
        private MenuItem _cMnuMargHeight;
        private MenuItem _cMnuMargHor;
        private MenuItem _cMnuMargLeft;
        private MenuItem _cMnuMargRight;
        private MenuItem _cMnuMargTop;
        private MenuItem _cMnuMargVert;
        private MenuItem _cMnuMargWidth;
        private MenuItem _cMnuMoveDown;
        private MenuItem _cMnuMoveUp;
        private MenuItem _cMnuSelAli;
        private MenuItem _cMnuSelBottom;
        private MenuItem _cMnuSelFit;
        private MenuItem _cMnuSelHeight;
        private MenuItem _cMnuSelHor;
        private MenuItem _cMnuSelLeft;
        private MenuItem _cMnuSelRight;
        private MenuItem _cMnuSelTop;
        private MenuItem _cMnuSelVert;
        private MenuItem _cMnuSelWidth;
        private ContextMenu _contextMenuRight;
        private HScrollBar _hScrollBar;
        private Panel _hScrollBarPanel;
        private LayoutDocToolStrip _layoutDocToolStrip;
        private LayoutInsertToolStrip _layoutInsertToolStrip;
        private LayoutListBox _layoutListBox;
        private LayoutMapToolStrip _layoutMapToolStrip;
        private LayoutMenuStrip _layoutMenuStrip;
        private LayoutPropertyGrid _layoutPropertyGrip;
        private LayoutZoomToolStrip _layoutZoomToolStrip;
        private MenuItem _menuItem19;
        private MenuItem _menuItem2;
        private MenuItem _menuItem4;
        private VScrollBar _vScrollBar;
    }
}
