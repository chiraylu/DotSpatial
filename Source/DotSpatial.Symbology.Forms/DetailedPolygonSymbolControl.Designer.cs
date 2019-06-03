using System.Windows.Forms;

namespace DotSpatial.Symbology.Forms
{
    partial class DetailedPolygonSymbolControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DetailedPolygonSymbolControl));
            this.btnAddToCustom = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnImportSymbol = new System.Windows.Forms.Button();
            this.btnSaveSymbol = new System.Windows.Forms.Button();
            this.cmbUnits = new System.Windows.Forms.ComboBox();
            this.lblUnits = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lblPreview = new System.Windows.Forms.Label();
            this.lblScaleMode = new System.Windows.Forms.Label();
            this.cmbScaleMode = new System.Windows.Forms.ComboBox();
            this.chkSmoothing = new System.Windows.Forms.CheckBox();
            this.lblPatternType = new System.Windows.Forms.Label();
            this.cmbPatternType = new System.Windows.Forms.ComboBox();
            this.tabPatternProperties = new System.Windows.Forms.TabControl();
            this.tabSimple = new System.Windows.Forms.TabPage();
            this.lblColorSimple = new System.Windows.Forms.Label();
            this.cbColorSimple = new DotSpatial.Symbology.Forms.ColorButton();
            this.sldOpacitySimple = new DotSpatial.Symbology.Forms.RampSlider();
            this.tabPicture = new System.Windows.Forms.TabPage();
            this.dbxScaleY = new DotSpatial.Symbology.Forms.DoubleBox();
            this.dbxScaleX = new DotSpatial.Symbology.Forms.DoubleBox();
            this.angTileAngle = new DotSpatial.Symbology.Forms.AngleControl();
            this.lblTileMode = new System.Windows.Forms.Label();
            this.cmbTileMode = new System.Windows.Forms.ComboBox();
            this.btnLoadImage = new System.Windows.Forms.Button();
            this.txtImage = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tabGradient = new System.Windows.Forms.TabPage();
            this.sliderGradient = new DotSpatial.Symbology.Forms.GradientControl();
            this.cmbGradientType = new System.Windows.Forms.ComboBox();
            this.lblEndColor = new System.Windows.Forms.Label();
            this.lblStartColor = new System.Windows.Forms.Label();
            this.angGradientAngle = new DotSpatial.Symbology.Forms.AngleControl();
            this.tabHatch = new System.Windows.Forms.TabPage();
            this.lblHatchStyle = new System.Windows.Forms.Label();
            this.cmbHatchStyle = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.hatchBackOpacity = new DotSpatial.Symbology.Forms.RampSlider();
            this.hatchBackColor = new DotSpatial.Symbology.Forms.ColorButton();
            this.label1 = new System.Windows.Forms.Label();
            this.hatchForeOpacity = new DotSpatial.Symbology.Forms.RampSlider();
            this.hatchForeColor = new DotSpatial.Symbology.Forms.ColorButton();
            this.helpProvider1 = new System.Windows.Forms.HelpProvider();
            this.ocOutline = new DotSpatial.Symbology.Forms.OutlineControl();
            this.ccPatterns = new DotSpatial.Symbology.Forms.PatternCollectionControl();
            this.groupBox1.SuspendLayout();
            this.tabPatternProperties.SuspendLayout();
            this.tabSimple.SuspendLayout();
            this.tabPicture.SuspendLayout();
            this.tabGradient.SuspendLayout();
            this.tabHatch.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnAddToCustom
            // 
            resources.ApplyResources(this.btnAddToCustom, "btnAddToCustom");
            this.helpProvider1.SetHelpKeyword(this.btnAddToCustom, resources.GetString("btnAddToCustom.HelpKeyword"));
            this.helpProvider1.SetHelpNavigator(this.btnAddToCustom, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("btnAddToCustom.HelpNavigator"))));
            this.helpProvider1.SetHelpString(this.btnAddToCustom, resources.GetString("btnAddToCustom.HelpString"));
            this.btnAddToCustom.Name = "btnAddToCustom";
            this.helpProvider1.SetShowHelp(this.btnAddToCustom, ((bool)(resources.GetObject("btnAddToCustom.ShowHelp"))));
            this.btnAddToCustom.UseVisualStyleBackColor = true;
            this.btnAddToCustom.Click += new System.EventHandler(this.BtnAddToCustomClick);
            // 
            // groupBox1
            // 
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Controls.Add(this.btnImportSymbol);
            this.groupBox1.Controls.Add(this.btnSaveSymbol);
            this.groupBox1.Controls.Add(this.cmbUnits);
            this.groupBox1.Controls.Add(this.lblUnits);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.lblPreview);
            this.groupBox1.Controls.Add(this.lblScaleMode);
            this.groupBox1.Controls.Add(this.cmbScaleMode);
            this.groupBox1.Controls.Add(this.chkSmoothing);
            this.helpProvider1.SetHelpKeyword(this.groupBox1, resources.GetString("groupBox1.HelpKeyword"));
            this.helpProvider1.SetHelpNavigator(this.groupBox1, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("groupBox1.HelpNavigator"))));
            this.helpProvider1.SetHelpString(this.groupBox1, resources.GetString("groupBox1.HelpString"));
            this.groupBox1.Name = "groupBox1";
            this.helpProvider1.SetShowHelp(this.groupBox1, ((bool)(resources.GetObject("groupBox1.ShowHelp"))));
            this.groupBox1.TabStop = false;
            this.groupBox1.UseCompatibleTextRendering = true;
            // 
            // btnImportSymbol
            // 
            resources.ApplyResources(this.btnImportSymbol, "btnImportSymbol");
            this.helpProvider1.SetHelpKeyword(this.btnImportSymbol, resources.GetString("btnImportSymbol.HelpKeyword"));
            this.helpProvider1.SetHelpNavigator(this.btnImportSymbol, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("btnImportSymbol.HelpNavigator"))));
            this.helpProvider1.SetHelpString(this.btnImportSymbol, resources.GetString("btnImportSymbol.HelpString"));
            this.btnImportSymbol.Name = "btnImportSymbol";
            this.btnImportSymbol.UseVisualStyleBackColor = true;
            this.btnImportSymbol.Click += new System.EventHandler(this.BtnImportSymbolClick);
            // 
            // btnSaveSymbol
            // 
            resources.ApplyResources(this.btnSaveSymbol, "btnSaveSymbol");
            this.helpProvider1.SetHelpKeyword(this.btnSaveSymbol, resources.GetString("btnSaveSymbol.HelpKeyword"));
            this.helpProvider1.SetHelpNavigator(this.btnSaveSymbol, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("btnSaveSymbol.HelpNavigator"))));
            this.helpProvider1.SetHelpString(this.btnSaveSymbol, resources.GetString("btnSaveSymbol.HelpString"));
            this.btnSaveSymbol.Name = "btnSaveSymbol";
            this.btnSaveSymbol.UseVisualStyleBackColor = true;
            this.btnSaveSymbol.Click += new System.EventHandler(this.BtnSaveSymbolClick);
            // 
            // cmbUnits
            // 
            resources.ApplyResources(this.cmbUnits, "cmbUnits");
            this.cmbUnits.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbUnits.FormattingEnabled = true;
            this.helpProvider1.SetHelpKeyword(this.cmbUnits, resources.GetString("cmbUnits.HelpKeyword"));
            this.helpProvider1.SetHelpNavigator(this.cmbUnits, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("cmbUnits.HelpNavigator"))));
            this.helpProvider1.SetHelpString(this.cmbUnits, resources.GetString("cmbUnits.HelpString"));
            this.cmbUnits.Items.AddRange(new object[] {
            resources.GetString("cmbUnits.Items"),
            resources.GetString("cmbUnits.Items1"),
            resources.GetString("cmbUnits.Items2"),
            resources.GetString("cmbUnits.Items3"),
            resources.GetString("cmbUnits.Items4"),
            resources.GetString("cmbUnits.Items5"),
            resources.GetString("cmbUnits.Items6")});
            this.cmbUnits.Name = "cmbUnits";
            this.helpProvider1.SetShowHelp(this.cmbUnits, ((bool)(resources.GetObject("cmbUnits.ShowHelp"))));
            this.cmbUnits.SelectedIndexChanged += new System.EventHandler(this.CmbUnitsSelectedIndexChanged);
            // 
            // lblUnits
            // 
            resources.ApplyResources(this.lblUnits, "lblUnits");
            this.helpProvider1.SetHelpKeyword(this.lblUnits, resources.GetString("lblUnits.HelpKeyword"));
            this.helpProvider1.SetHelpNavigator(this.lblUnits, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("lblUnits.HelpNavigator"))));
            this.helpProvider1.SetHelpString(this.lblUnits, resources.GetString("lblUnits.HelpString"));
            this.lblUnits.Name = "lblUnits";
            this.helpProvider1.SetShowHelp(this.lblUnits, ((bool)(resources.GetObject("lblUnits.ShowHelp"))));
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.helpProvider1.SetHelpKeyword(this.label3, resources.GetString("label3.HelpKeyword"));
            this.helpProvider1.SetHelpNavigator(this.label3, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("label3.HelpNavigator"))));
            this.helpProvider1.SetHelpString(this.label3, resources.GetString("label3.HelpString"));
            this.label3.Name = "label3";
            this.helpProvider1.SetShowHelp(this.label3, ((bool)(resources.GetObject("label3.ShowHelp"))));
            // 
            // lblPreview
            // 
            resources.ApplyResources(this.lblPreview, "lblPreview");
            this.lblPreview.BackColor = System.Drawing.Color.White;
            this.lblPreview.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.helpProvider1.SetHelpKeyword(this.lblPreview, resources.GetString("lblPreview.HelpKeyword"));
            this.helpProvider1.SetHelpNavigator(this.lblPreview, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("lblPreview.HelpNavigator"))));
            this.helpProvider1.SetHelpString(this.lblPreview, resources.GetString("lblPreview.HelpString"));
            this.lblPreview.Name = "lblPreview";
            this.helpProvider1.SetShowHelp(this.lblPreview, ((bool)(resources.GetObject("lblPreview.ShowHelp"))));
            // 
            // lblScaleMode
            // 
            resources.ApplyResources(this.lblScaleMode, "lblScaleMode");
            this.helpProvider1.SetHelpKeyword(this.lblScaleMode, resources.GetString("lblScaleMode.HelpKeyword"));
            this.helpProvider1.SetHelpNavigator(this.lblScaleMode, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("lblScaleMode.HelpNavigator"))));
            this.helpProvider1.SetHelpString(this.lblScaleMode, resources.GetString("lblScaleMode.HelpString"));
            this.lblScaleMode.Name = "lblScaleMode";
            this.helpProvider1.SetShowHelp(this.lblScaleMode, ((bool)(resources.GetObject("lblScaleMode.ShowHelp"))));
            // 
            // cmbScaleMode
            // 
            resources.ApplyResources(this.cmbScaleMode, "cmbScaleMode");
            this.cmbScaleMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbScaleMode.FormattingEnabled = true;
            this.helpProvider1.SetHelpKeyword(this.cmbScaleMode, resources.GetString("cmbScaleMode.HelpKeyword"));
            this.helpProvider1.SetHelpNavigator(this.cmbScaleMode, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("cmbScaleMode.HelpNavigator"))));
            this.helpProvider1.SetHelpString(this.cmbScaleMode, resources.GetString("cmbScaleMode.HelpString"));
            this.cmbScaleMode.Items.AddRange(new object[] {
            resources.GetString("cmbScaleMode.Items"),
            resources.GetString("cmbScaleMode.Items1"),
            resources.GetString("cmbScaleMode.Items2")});
            this.cmbScaleMode.Name = "cmbScaleMode";
            this.helpProvider1.SetShowHelp(this.cmbScaleMode, ((bool)(resources.GetObject("cmbScaleMode.ShowHelp"))));
            this.cmbScaleMode.SelectedIndexChanged += new System.EventHandler(this.CmbScaleModeSelectedIndexChanged);
            // 
            // chkSmoothing
            // 
            resources.ApplyResources(this.chkSmoothing, "chkSmoothing");
            this.helpProvider1.SetHelpKeyword(this.chkSmoothing, resources.GetString("chkSmoothing.HelpKeyword"));
            this.helpProvider1.SetHelpNavigator(this.chkSmoothing, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("chkSmoothing.HelpNavigator"))));
            this.helpProvider1.SetHelpString(this.chkSmoothing, resources.GetString("chkSmoothing.HelpString"));
            this.chkSmoothing.Name = "chkSmoothing";
            this.helpProvider1.SetShowHelp(this.chkSmoothing, ((bool)(resources.GetObject("chkSmoothing.ShowHelp"))));
            this.chkSmoothing.UseVisualStyleBackColor = true;
            this.chkSmoothing.CheckedChanged += new System.EventHandler(this.ChkSmoothingCheckedChanged);
            // 
            // lblPatternType
            // 
            resources.ApplyResources(this.lblPatternType, "lblPatternType");
            this.helpProvider1.SetHelpKeyword(this.lblPatternType, resources.GetString("lblPatternType.HelpKeyword"));
            this.helpProvider1.SetHelpNavigator(this.lblPatternType, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("lblPatternType.HelpNavigator"))));
            this.helpProvider1.SetHelpString(this.lblPatternType, resources.GetString("lblPatternType.HelpString"));
            this.lblPatternType.Name = "lblPatternType";
            this.helpProvider1.SetShowHelp(this.lblPatternType, ((bool)(resources.GetObject("lblPatternType.ShowHelp"))));
            // 
            // cmbPatternType
            // 
            resources.ApplyResources(this.cmbPatternType, "cmbPatternType");
            this.cmbPatternType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPatternType.FormattingEnabled = true;
            this.helpProvider1.SetHelpKeyword(this.cmbPatternType, resources.GetString("cmbPatternType.HelpKeyword"));
            this.helpProvider1.SetHelpNavigator(this.cmbPatternType, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("cmbPatternType.HelpNavigator"))));
            this.helpProvider1.SetHelpString(this.cmbPatternType, resources.GetString("cmbPatternType.HelpString"));
            this.cmbPatternType.Items.AddRange(new object[] {
            resources.GetString("cmbPatternType.Items"),
            resources.GetString("cmbPatternType.Items1"),
            resources.GetString("cmbPatternType.Items2"),
            resources.GetString("cmbPatternType.Items3")});
            this.cmbPatternType.Name = "cmbPatternType";
            this.helpProvider1.SetShowHelp(this.cmbPatternType, ((bool)(resources.GetObject("cmbPatternType.ShowHelp"))));
            this.cmbPatternType.SelectedIndexChanged += new System.EventHandler(this.CmbPatternTypeSelectedIndexChanged);
            // 
            // tabPatternProperties
            // 
            resources.ApplyResources(this.tabPatternProperties, "tabPatternProperties");
            this.tabPatternProperties.Controls.Add(this.tabSimple);
            this.tabPatternProperties.Controls.Add(this.tabPicture);
            this.tabPatternProperties.Controls.Add(this.tabGradient);
            this.tabPatternProperties.Controls.Add(this.tabHatch);
            this.helpProvider1.SetHelpKeyword(this.tabPatternProperties, resources.GetString("tabPatternProperties.HelpKeyword"));
            this.helpProvider1.SetHelpNavigator(this.tabPatternProperties, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("tabPatternProperties.HelpNavigator"))));
            this.helpProvider1.SetHelpString(this.tabPatternProperties, resources.GetString("tabPatternProperties.HelpString"));
            this.tabPatternProperties.Name = "tabPatternProperties";
            this.tabPatternProperties.SelectedIndex = 0;
            this.helpProvider1.SetShowHelp(this.tabPatternProperties, ((bool)(resources.GetObject("tabPatternProperties.ShowHelp"))));
            // 
            // tabSimple
            // 
            resources.ApplyResources(this.tabSimple, "tabSimple");
            this.tabSimple.Controls.Add(this.lblColorSimple);
            this.tabSimple.Controls.Add(this.cbColorSimple);
            this.tabSimple.Controls.Add(this.sldOpacitySimple);
            this.helpProvider1.SetHelpKeyword(this.tabSimple, resources.GetString("tabSimple.HelpKeyword"));
            this.helpProvider1.SetHelpNavigator(this.tabSimple, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("tabSimple.HelpNavigator"))));
            this.helpProvider1.SetHelpString(this.tabSimple, resources.GetString("tabSimple.HelpString"));
            this.tabSimple.Name = "tabSimple";
            this.helpProvider1.SetShowHelp(this.tabSimple, ((bool)(resources.GetObject("tabSimple.ShowHelp"))));
            this.tabSimple.UseVisualStyleBackColor = true;
            // 
            // lblColorSimple
            // 
            resources.ApplyResources(this.lblColorSimple, "lblColorSimple");
            this.helpProvider1.SetHelpKeyword(this.lblColorSimple, resources.GetString("lblColorSimple.HelpKeyword"));
            this.helpProvider1.SetHelpNavigator(this.lblColorSimple, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("lblColorSimple.HelpNavigator"))));
            this.helpProvider1.SetHelpString(this.lblColorSimple, resources.GetString("lblColorSimple.HelpString"));
            this.lblColorSimple.Name = "lblColorSimple";
            this.helpProvider1.SetShowHelp(this.lblColorSimple, ((bool)(resources.GetObject("lblColorSimple.ShowHelp"))));
            // 
            // cbColorSimple
            // 
            resources.ApplyResources(this.cbColorSimple, "cbColorSimple");
            this.cbColorSimple.BevelRadius = 4;
            this.cbColorSimple.Color = System.Drawing.Color.Blue;
            this.helpProvider1.SetHelpKeyword(this.cbColorSimple, resources.GetString("cbColorSimple.HelpKeyword"));
            this.helpProvider1.SetHelpNavigator(this.cbColorSimple, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("cbColorSimple.HelpNavigator"))));
            this.helpProvider1.SetHelpString(this.cbColorSimple, resources.GetString("cbColorSimple.HelpString"));
            this.cbColorSimple.LaunchDialogOnClick = true;
            this.cbColorSimple.Name = "cbColorSimple";
            this.cbColorSimple.RoundingRadius = 10;
            this.helpProvider1.SetShowHelp(this.cbColorSimple, ((bool)(resources.GetObject("cbColorSimple.ShowHelp"))));
            this.cbColorSimple.ColorChanged += new System.EventHandler(this.CbColorSimpleColorChanged);
            // 
            // sldOpacitySimple
            // 
            resources.ApplyResources(this.sldOpacitySimple, "sldOpacitySimple");
            this.sldOpacitySimple.ColorButton = null;
            this.sldOpacitySimple.FlipRamp = false;
            this.sldOpacitySimple.FlipText = false;
            this.helpProvider1.SetHelpKeyword(this.sldOpacitySimple, resources.GetString("sldOpacitySimple.HelpKeyword"));
            this.helpProvider1.SetHelpNavigator(this.sldOpacitySimple, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("sldOpacitySimple.HelpNavigator"))));
            this.helpProvider1.SetHelpString(this.sldOpacitySimple, resources.GetString("sldOpacitySimple.HelpString"));
            this.sldOpacitySimple.InvertRamp = false;
            this.sldOpacitySimple.Maximum = 1D;
            this.sldOpacitySimple.MaximumColor = System.Drawing.Color.CornflowerBlue;
            this.sldOpacitySimple.Minimum = 0D;
            this.sldOpacitySimple.MinimumColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.sldOpacitySimple.Name = "sldOpacitySimple";
            this.sldOpacitySimple.NumberFormat = null;
            this.sldOpacitySimple.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.sldOpacitySimple.RampRadius = 8F;
            this.sldOpacitySimple.RampText = "Opacity";
            this.sldOpacitySimple.RampTextAlignment = System.Drawing.ContentAlignment.BottomCenter;
            this.sldOpacitySimple.RampTextBehindRamp = true;
            this.sldOpacitySimple.RampTextColor = System.Drawing.Color.Black;
            this.sldOpacitySimple.RampTextFont = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.helpProvider1.SetShowHelp(this.sldOpacitySimple, ((bool)(resources.GetObject("sldOpacitySimple.ShowHelp"))));
            this.sldOpacitySimple.ShowMaximum = true;
            this.sldOpacitySimple.ShowMinimum = true;
            this.sldOpacitySimple.ShowTicks = true;
            this.sldOpacitySimple.ShowValue = false;
            this.sldOpacitySimple.SliderColor = System.Drawing.Color.SteelBlue;
            this.sldOpacitySimple.SliderRadius = 4F;
            this.sldOpacitySimple.TickColor = System.Drawing.Color.DarkGray;
            this.sldOpacitySimple.TickSpacing = 5F;
            this.sldOpacitySimple.Value = 0D;
            this.sldOpacitySimple.ValueChanged += new System.EventHandler(this.SldOpacitySimpleValueChanged);
            // 
            // tabPicture
            // 
            resources.ApplyResources(this.tabPicture, "tabPicture");
            this.tabPicture.Controls.Add(this.dbxScaleY);
            this.tabPicture.Controls.Add(this.dbxScaleX);
            this.tabPicture.Controls.Add(this.angTileAngle);
            this.tabPicture.Controls.Add(this.lblTileMode);
            this.tabPicture.Controls.Add(this.cmbTileMode);
            this.tabPicture.Controls.Add(this.btnLoadImage);
            this.tabPicture.Controls.Add(this.txtImage);
            this.tabPicture.Controls.Add(this.label4);
            this.helpProvider1.SetHelpKeyword(this.tabPicture, resources.GetString("tabPicture.HelpKeyword"));
            this.helpProvider1.SetHelpNavigator(this.tabPicture, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("tabPicture.HelpNavigator"))));
            this.helpProvider1.SetHelpString(this.tabPicture, resources.GetString("tabPicture.HelpString"));
            this.tabPicture.Name = "tabPicture";
            this.helpProvider1.SetShowHelp(this.tabPicture, ((bool)(resources.GetObject("tabPicture.ShowHelp"))));
            this.tabPicture.UseVisualStyleBackColor = true;
            // 
            // dbxScaleY
            // 
            resources.ApplyResources(this.dbxScaleY, "dbxScaleY");
            this.dbxScaleY.BackColorInvalid = System.Drawing.Color.Salmon;
            this.dbxScaleY.BackColorRegular = System.Drawing.Color.Empty;
            this.helpProvider1.SetHelpKeyword(this.dbxScaleY, resources.GetString("dbxScaleY.HelpKeyword"));
            this.helpProvider1.SetHelpNavigator(this.dbxScaleY, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("dbxScaleY.HelpNavigator"))));
            this.helpProvider1.SetHelpString(this.dbxScaleY, resources.GetString("dbxScaleY.HelpString"));
            this.dbxScaleY.InvalidHelp = "The value entered could not be correctly parsed into a valid double precision flo" +
    "ating point value.";
            this.dbxScaleY.IsValid = true;
            this.dbxScaleY.Name = "dbxScaleY";
            this.dbxScaleY.NumberFormat = null;
            this.dbxScaleY.RegularHelp = "Enter a double precision floating point value.";
            this.helpProvider1.SetShowHelp(this.dbxScaleY, ((bool)(resources.GetObject("dbxScaleY.ShowHelp"))));
            this.dbxScaleY.Value = 0D;
            this.dbxScaleY.TextChanged += new System.EventHandler(this.DbxScaleYTextChanged);
            // 
            // dbxScaleX
            // 
            resources.ApplyResources(this.dbxScaleX, "dbxScaleX");
            this.dbxScaleX.BackColorInvalid = System.Drawing.Color.Salmon;
            this.dbxScaleX.BackColorRegular = System.Drawing.Color.Empty;
            this.helpProvider1.SetHelpKeyword(this.dbxScaleX, resources.GetString("dbxScaleX.HelpKeyword"));
            this.helpProvider1.SetHelpNavigator(this.dbxScaleX, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("dbxScaleX.HelpNavigator"))));
            this.helpProvider1.SetHelpString(this.dbxScaleX, resources.GetString("dbxScaleX.HelpString"));
            this.dbxScaleX.InvalidHelp = "The value entered could not be correctly parsed into a valid double precision flo" +
    "ating point value.";
            this.dbxScaleX.IsValid = true;
            this.dbxScaleX.Name = "dbxScaleX";
            this.dbxScaleX.NumberFormat = null;
            this.dbxScaleX.RegularHelp = "Enter a double precision floating point value.";
            this.helpProvider1.SetShowHelp(this.dbxScaleX, ((bool)(resources.GetObject("dbxScaleX.ShowHelp"))));
            this.dbxScaleX.Value = 0D;
            this.dbxScaleX.TextChanged += new System.EventHandler(this.DbxScaleXTextChanged);
            // 
            // angTileAngle
            // 
            resources.ApplyResources(this.angTileAngle, "angTileAngle");
            this.angTileAngle.Angle = 0;
            this.angTileAngle.BackColor = System.Drawing.SystemColors.Control;
            this.angTileAngle.Clockwise = false;
            this.helpProvider1.SetHelpKeyword(this.angTileAngle, resources.GetString("angTileAngle.HelpKeyword"));
            this.helpProvider1.SetHelpNavigator(this.angTileAngle, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("angTileAngle.HelpNavigator"))));
            this.helpProvider1.SetHelpString(this.angTileAngle, resources.GetString("angTileAngle.HelpString"));
            this.angTileAngle.KnobColor = System.Drawing.Color.SteelBlue;
            this.angTileAngle.Name = "angTileAngle";
            this.helpProvider1.SetShowHelp(this.angTileAngle, ((bool)(resources.GetObject("angTileAngle.ShowHelp"))));
            this.angTileAngle.StartAngle = 0;
            this.angTileAngle.AngleChanged += new System.EventHandler(this.AngTileAngleAngleChanged);
            // 
            // lblTileMode
            // 
            resources.ApplyResources(this.lblTileMode, "lblTileMode");
            this.helpProvider1.SetHelpKeyword(this.lblTileMode, resources.GetString("lblTileMode.HelpKeyword"));
            this.helpProvider1.SetHelpNavigator(this.lblTileMode, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("lblTileMode.HelpNavigator"))));
            this.helpProvider1.SetHelpString(this.lblTileMode, resources.GetString("lblTileMode.HelpString"));
            this.lblTileMode.Name = "lblTileMode";
            this.helpProvider1.SetShowHelp(this.lblTileMode, ((bool)(resources.GetObject("lblTileMode.ShowHelp"))));
            // 
            // cmbTileMode
            // 
            resources.ApplyResources(this.cmbTileMode, "cmbTileMode");
            this.cmbTileMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTileMode.FormattingEnabled = true;
            this.helpProvider1.SetHelpKeyword(this.cmbTileMode, resources.GetString("cmbTileMode.HelpKeyword"));
            this.helpProvider1.SetHelpNavigator(this.cmbTileMode, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("cmbTileMode.HelpNavigator"))));
            this.helpProvider1.SetHelpString(this.cmbTileMode, resources.GetString("cmbTileMode.HelpString"));
            this.cmbTileMode.Items.AddRange(new object[] {
            resources.GetString("cmbTileMode.Items"),
            resources.GetString("cmbTileMode.Items1"),
            resources.GetString("cmbTileMode.Items2"),
            resources.GetString("cmbTileMode.Items3"),
            resources.GetString("cmbTileMode.Items4")});
            this.cmbTileMode.Name = "cmbTileMode";
            this.helpProvider1.SetShowHelp(this.cmbTileMode, ((bool)(resources.GetObject("cmbTileMode.ShowHelp"))));
            this.cmbTileMode.SelectedIndexChanged += new System.EventHandler(this.CmbTileModeSelectedIndexChanged);
            // 
            // btnLoadImage
            // 
            resources.ApplyResources(this.btnLoadImage, "btnLoadImage");
            this.helpProvider1.SetHelpKeyword(this.btnLoadImage, resources.GetString("btnLoadImage.HelpKeyword"));
            this.helpProvider1.SetHelpNavigator(this.btnLoadImage, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("btnLoadImage.HelpNavigator"))));
            this.helpProvider1.SetHelpString(this.btnLoadImage, resources.GetString("btnLoadImage.HelpString"));
            this.btnLoadImage.Name = "btnLoadImage";
            this.helpProvider1.SetShowHelp(this.btnLoadImage, ((bool)(resources.GetObject("btnLoadImage.ShowHelp"))));
            this.btnLoadImage.UseVisualStyleBackColor = true;
            this.btnLoadImage.Click += new System.EventHandler(this.BtnLoadImageClick);
            // 
            // txtImage
            // 
            resources.ApplyResources(this.txtImage, "txtImage");
            this.helpProvider1.SetHelpKeyword(this.txtImage, resources.GetString("txtImage.HelpKeyword"));
            this.helpProvider1.SetHelpNavigator(this.txtImage, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("txtImage.HelpNavigator"))));
            this.helpProvider1.SetHelpString(this.txtImage, resources.GetString("txtImage.HelpString"));
            this.txtImage.Name = "txtImage";
            this.helpProvider1.SetShowHelp(this.txtImage, ((bool)(resources.GetObject("txtImage.ShowHelp"))));
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.helpProvider1.SetHelpKeyword(this.label4, resources.GetString("label4.HelpKeyword"));
            this.helpProvider1.SetHelpNavigator(this.label4, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("label4.HelpNavigator"))));
            this.helpProvider1.SetHelpString(this.label4, resources.GetString("label4.HelpString"));
            this.label4.Name = "label4";
            this.helpProvider1.SetShowHelp(this.label4, ((bool)(resources.GetObject("label4.ShowHelp"))));
            // 
            // tabGradient
            // 
            resources.ApplyResources(this.tabGradient, "tabGradient");
            this.tabGradient.Controls.Add(this.sliderGradient);
            this.tabGradient.Controls.Add(this.cmbGradientType);
            this.tabGradient.Controls.Add(this.lblEndColor);
            this.tabGradient.Controls.Add(this.lblStartColor);
            this.tabGradient.Controls.Add(this.angGradientAngle);
            this.helpProvider1.SetHelpKeyword(this.tabGradient, resources.GetString("tabGradient.HelpKeyword"));
            this.helpProvider1.SetHelpNavigator(this.tabGradient, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("tabGradient.HelpNavigator"))));
            this.helpProvider1.SetHelpString(this.tabGradient, resources.GetString("tabGradient.HelpString"));
            this.tabGradient.Name = "tabGradient";
            this.helpProvider1.SetShowHelp(this.tabGradient, ((bool)(resources.GetObject("tabGradient.ShowHelp"))));
            this.tabGradient.UseVisualStyleBackColor = true;
            // 
            // sliderGradient
            // 
            resources.ApplyResources(this.sliderGradient, "sliderGradient");
            this.sliderGradient.EndValue = 0.8F;
            this.helpProvider1.SetHelpKeyword(this.sliderGradient, resources.GetString("sliderGradient.HelpKeyword"));
            this.helpProvider1.SetHelpNavigator(this.sliderGradient, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("sliderGradient.HelpNavigator"))));
            this.helpProvider1.SetHelpString(this.sliderGradient, resources.GetString("sliderGradient.HelpString"));
            this.sliderGradient.MaximumColor = System.Drawing.Color.Blue;
            this.sliderGradient.MinimumColor = System.Drawing.Color.Lime;
            this.sliderGradient.Name = "sliderGradient";
            this.helpProvider1.SetShowHelp(this.sliderGradient, ((bool)(resources.GetObject("sliderGradient.ShowHelp"))));
            this.sliderGradient.SlidersEnabled = true;
            this.sliderGradient.StartValue = 0.2F;
            this.sliderGradient.GradientChanging += new System.EventHandler(this.SliderGradientGradientChanging);
            // 
            // cmbGradientType
            // 
            resources.ApplyResources(this.cmbGradientType, "cmbGradientType");
            this.cmbGradientType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbGradientType.FormattingEnabled = true;
            this.helpProvider1.SetHelpKeyword(this.cmbGradientType, resources.GetString("cmbGradientType.HelpKeyword"));
            this.helpProvider1.SetHelpNavigator(this.cmbGradientType, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("cmbGradientType.HelpNavigator"))));
            this.helpProvider1.SetHelpString(this.cmbGradientType, resources.GetString("cmbGradientType.HelpString"));
            this.cmbGradientType.Items.AddRange(new object[] {
            resources.GetString("cmbGradientType.Items"),
            resources.GetString("cmbGradientType.Items1"),
            resources.GetString("cmbGradientType.Items2")});
            this.cmbGradientType.Name = "cmbGradientType";
            this.helpProvider1.SetShowHelp(this.cmbGradientType, ((bool)(resources.GetObject("cmbGradientType.ShowHelp"))));
            this.cmbGradientType.SelectedIndexChanged += new System.EventHandler(this.CmbGradientTypeSelectedIndexChanged);
            // 
            // lblEndColor
            // 
            resources.ApplyResources(this.lblEndColor, "lblEndColor");
            this.helpProvider1.SetHelpKeyword(this.lblEndColor, resources.GetString("lblEndColor.HelpKeyword"));
            this.helpProvider1.SetHelpNavigator(this.lblEndColor, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("lblEndColor.HelpNavigator"))));
            this.helpProvider1.SetHelpString(this.lblEndColor, resources.GetString("lblEndColor.HelpString"));
            this.lblEndColor.Name = "lblEndColor";
            this.helpProvider1.SetShowHelp(this.lblEndColor, ((bool)(resources.GetObject("lblEndColor.ShowHelp"))));
            // 
            // lblStartColor
            // 
            resources.ApplyResources(this.lblStartColor, "lblStartColor");
            this.helpProvider1.SetHelpKeyword(this.lblStartColor, resources.GetString("lblStartColor.HelpKeyword"));
            this.helpProvider1.SetHelpNavigator(this.lblStartColor, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("lblStartColor.HelpNavigator"))));
            this.helpProvider1.SetHelpString(this.lblStartColor, resources.GetString("lblStartColor.HelpString"));
            this.lblStartColor.Name = "lblStartColor";
            this.helpProvider1.SetShowHelp(this.lblStartColor, ((bool)(resources.GetObject("lblStartColor.ShowHelp"))));
            // 
            // angGradientAngle
            // 
            resources.ApplyResources(this.angGradientAngle, "angGradientAngle");
            this.angGradientAngle.Angle = 0;
            this.angGradientAngle.BackColor = System.Drawing.SystemColors.Control;
            this.angGradientAngle.Clockwise = false;
            this.helpProvider1.SetHelpKeyword(this.angGradientAngle, resources.GetString("angGradientAngle.HelpKeyword"));
            this.helpProvider1.SetHelpNavigator(this.angGradientAngle, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("angGradientAngle.HelpNavigator"))));
            this.helpProvider1.SetHelpString(this.angGradientAngle, resources.GetString("angGradientAngle.HelpString"));
            this.angGradientAngle.KnobColor = System.Drawing.Color.SteelBlue;
            this.angGradientAngle.Name = "angGradientAngle";
            this.helpProvider1.SetShowHelp(this.angGradientAngle, ((bool)(resources.GetObject("angGradientAngle.ShowHelp"))));
            this.angGradientAngle.StartAngle = 0;
            this.angGradientAngle.AngleChanged += new System.EventHandler(this.AngGradientAngleAngleChanged);
            // 
            // tabHatch
            // 
            resources.ApplyResources(this.tabHatch, "tabHatch");
            this.tabHatch.Controls.Add(this.lblHatchStyle);
            this.tabHatch.Controls.Add(this.cmbHatchStyle);
            this.tabHatch.Controls.Add(this.label2);
            this.tabHatch.Controls.Add(this.hatchBackOpacity);
            this.tabHatch.Controls.Add(this.hatchBackColor);
            this.tabHatch.Controls.Add(this.label1);
            this.tabHatch.Controls.Add(this.hatchForeOpacity);
            this.tabHatch.Controls.Add(this.hatchForeColor);
            this.helpProvider1.SetHelpKeyword(this.tabHatch, resources.GetString("tabHatch.HelpKeyword"));
            this.helpProvider1.SetHelpNavigator(this.tabHatch, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("tabHatch.HelpNavigator"))));
            this.helpProvider1.SetHelpString(this.tabHatch, resources.GetString("tabHatch.HelpString"));
            this.tabHatch.Name = "tabHatch";
            this.helpProvider1.SetShowHelp(this.tabHatch, ((bool)(resources.GetObject("tabHatch.ShowHelp"))));
            this.tabHatch.UseVisualStyleBackColor = true;
            // 
            // lblHatchStyle
            // 
            resources.ApplyResources(this.lblHatchStyle, "lblHatchStyle");
            this.helpProvider1.SetHelpKeyword(this.lblHatchStyle, resources.GetString("lblHatchStyle.HelpKeyword"));
            this.helpProvider1.SetHelpNavigator(this.lblHatchStyle, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("lblHatchStyle.HelpNavigator"))));
            this.helpProvider1.SetHelpString(this.lblHatchStyle, resources.GetString("lblHatchStyle.HelpString"));
            this.lblHatchStyle.Name = "lblHatchStyle";
            this.helpProvider1.SetShowHelp(this.lblHatchStyle, ((bool)(resources.GetObject("lblHatchStyle.ShowHelp"))));
            // 
            // cmbHatchStyle
            // 
            resources.ApplyResources(this.cmbHatchStyle, "cmbHatchStyle");
            this.cmbHatchStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbHatchStyle.FormattingEnabled = true;
            this.helpProvider1.SetHelpKeyword(this.cmbHatchStyle, resources.GetString("cmbHatchStyle.HelpKeyword"));
            this.helpProvider1.SetHelpNavigator(this.cmbHatchStyle, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("cmbHatchStyle.HelpNavigator"))));
            this.helpProvider1.SetHelpString(this.cmbHatchStyle, resources.GetString("cmbHatchStyle.HelpString"));
            this.cmbHatchStyle.Name = "cmbHatchStyle";
            this.helpProvider1.SetShowHelp(this.cmbHatchStyle, ((bool)(resources.GetObject("cmbHatchStyle.ShowHelp"))));
            this.cmbHatchStyle.SelectedIndexChanged += new System.EventHandler(this.CmbHatchStyleSelectedIndexChanged);
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.helpProvider1.SetHelpKeyword(this.label2, resources.GetString("label2.HelpKeyword"));
            this.helpProvider1.SetHelpNavigator(this.label2, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("label2.HelpNavigator"))));
            this.helpProvider1.SetHelpString(this.label2, resources.GetString("label2.HelpString"));
            this.label2.Name = "label2";
            this.helpProvider1.SetShowHelp(this.label2, ((bool)(resources.GetObject("label2.ShowHelp"))));
            // 
            // hatchBackOpacity
            // 
            resources.ApplyResources(this.hatchBackOpacity, "hatchBackOpacity");
            this.hatchBackOpacity.ColorButton = null;
            this.hatchBackOpacity.FlipRamp = false;
            this.hatchBackOpacity.FlipText = false;
            this.helpProvider1.SetHelpKeyword(this.hatchBackOpacity, resources.GetString("hatchBackOpacity.HelpKeyword"));
            this.helpProvider1.SetHelpNavigator(this.hatchBackOpacity, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("hatchBackOpacity.HelpNavigator"))));
            this.helpProvider1.SetHelpString(this.hatchBackOpacity, resources.GetString("hatchBackOpacity.HelpString"));
            this.hatchBackOpacity.InvertRamp = false;
            this.hatchBackOpacity.Maximum = 1D;
            this.hatchBackOpacity.MaximumColor = System.Drawing.Color.CornflowerBlue;
            this.hatchBackOpacity.Minimum = 0D;
            this.hatchBackOpacity.MinimumColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.hatchBackOpacity.Name = "hatchBackOpacity";
            this.hatchBackOpacity.NumberFormat = null;
            this.hatchBackOpacity.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.hatchBackOpacity.RampRadius = 8F;
            this.hatchBackOpacity.RampText = "Opacity";
            this.hatchBackOpacity.RampTextAlignment = System.Drawing.ContentAlignment.BottomCenter;
            this.hatchBackOpacity.RampTextBehindRamp = true;
            this.hatchBackOpacity.RampTextColor = System.Drawing.Color.Black;
            this.hatchBackOpacity.RampTextFont = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.helpProvider1.SetShowHelp(this.hatchBackOpacity, ((bool)(resources.GetObject("hatchBackOpacity.ShowHelp"))));
            this.hatchBackOpacity.ShowMaximum = true;
            this.hatchBackOpacity.ShowMinimum = true;
            this.hatchBackOpacity.ShowTicks = true;
            this.hatchBackOpacity.ShowValue = false;
            this.hatchBackOpacity.SliderColor = System.Drawing.Color.Tan;
            this.hatchBackOpacity.SliderRadius = 4F;
            this.hatchBackOpacity.TickColor = System.Drawing.Color.DarkGray;
            this.hatchBackOpacity.TickSpacing = 5F;
            this.hatchBackOpacity.Value = 0D;
            this.hatchBackOpacity.ValueChanged += new System.EventHandler(this.HatchBackOpacityValueChanged);
            // 
            // hatchBackColor
            // 
            resources.ApplyResources(this.hatchBackColor, "hatchBackColor");
            this.hatchBackColor.BevelRadius = 4;
            this.hatchBackColor.Color = System.Drawing.Color.Blue;
            this.helpProvider1.SetHelpKeyword(this.hatchBackColor, resources.GetString("hatchBackColor.HelpKeyword"));
            this.helpProvider1.SetHelpNavigator(this.hatchBackColor, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("hatchBackColor.HelpNavigator"))));
            this.helpProvider1.SetHelpString(this.hatchBackColor, resources.GetString("hatchBackColor.HelpString"));
            this.hatchBackColor.LaunchDialogOnClick = true;
            this.hatchBackColor.Name = "hatchBackColor";
            this.hatchBackColor.RoundingRadius = 10;
            this.helpProvider1.SetShowHelp(this.hatchBackColor, ((bool)(resources.GetObject("hatchBackColor.ShowHelp"))));
            this.hatchBackColor.ColorChanged += new System.EventHandler(this.HatchBackColorColorChanged);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.helpProvider1.SetHelpKeyword(this.label1, resources.GetString("label1.HelpKeyword"));
            this.helpProvider1.SetHelpNavigator(this.label1, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("label1.HelpNavigator"))));
            this.helpProvider1.SetHelpString(this.label1, resources.GetString("label1.HelpString"));
            this.label1.Name = "label1";
            this.helpProvider1.SetShowHelp(this.label1, ((bool)(resources.GetObject("label1.ShowHelp"))));
            // 
            // hatchForeOpacity
            // 
            resources.ApplyResources(this.hatchForeOpacity, "hatchForeOpacity");
            this.hatchForeOpacity.ColorButton = null;
            this.hatchForeOpacity.FlipRamp = false;
            this.hatchForeOpacity.FlipText = false;
            this.helpProvider1.SetHelpKeyword(this.hatchForeOpacity, resources.GetString("hatchForeOpacity.HelpKeyword"));
            this.helpProvider1.SetHelpNavigator(this.hatchForeOpacity, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("hatchForeOpacity.HelpNavigator"))));
            this.helpProvider1.SetHelpString(this.hatchForeOpacity, resources.GetString("hatchForeOpacity.HelpString"));
            this.hatchForeOpacity.InvertRamp = false;
            this.hatchForeOpacity.Maximum = 1D;
            this.hatchForeOpacity.MaximumColor = System.Drawing.Color.CornflowerBlue;
            this.hatchForeOpacity.Minimum = 0D;
            this.hatchForeOpacity.MinimumColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.hatchForeOpacity.Name = "hatchForeOpacity";
            this.hatchForeOpacity.NumberFormat = null;
            this.hatchForeOpacity.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.hatchForeOpacity.RampRadius = 8F;
            this.hatchForeOpacity.RampText = "Opacity";
            this.hatchForeOpacity.RampTextAlignment = System.Drawing.ContentAlignment.BottomCenter;
            this.hatchForeOpacity.RampTextBehindRamp = true;
            this.hatchForeOpacity.RampTextColor = System.Drawing.Color.Black;
            this.hatchForeOpacity.RampTextFont = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.helpProvider1.SetShowHelp(this.hatchForeOpacity, ((bool)(resources.GetObject("hatchForeOpacity.ShowHelp"))));
            this.hatchForeOpacity.ShowMaximum = true;
            this.hatchForeOpacity.ShowMinimum = true;
            this.hatchForeOpacity.ShowTicks = true;
            this.hatchForeOpacity.ShowValue = false;
            this.hatchForeOpacity.SliderColor = System.Drawing.Color.Tan;
            this.hatchForeOpacity.SliderRadius = 4F;
            this.hatchForeOpacity.TickColor = System.Drawing.Color.DarkGray;
            this.hatchForeOpacity.TickSpacing = 5F;
            this.hatchForeOpacity.Value = 0D;
            this.hatchForeOpacity.ValueChanged += new System.EventHandler(this.HatchForeOpacityValueChanged);
            // 
            // hatchForeColor
            // 
            resources.ApplyResources(this.hatchForeColor, "hatchForeColor");
            this.hatchForeColor.BevelRadius = 4;
            this.hatchForeColor.Color = System.Drawing.Color.Blue;
            this.helpProvider1.SetHelpKeyword(this.hatchForeColor, resources.GetString("hatchForeColor.HelpKeyword"));
            this.helpProvider1.SetHelpNavigator(this.hatchForeColor, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("hatchForeColor.HelpNavigator"))));
            this.helpProvider1.SetHelpString(this.hatchForeColor, resources.GetString("hatchForeColor.HelpString"));
            this.hatchForeColor.LaunchDialogOnClick = true;
            this.hatchForeColor.Name = "hatchForeColor";
            this.hatchForeColor.RoundingRadius = 10;
            this.helpProvider1.SetShowHelp(this.hatchForeColor, ((bool)(resources.GetObject("hatchForeColor.ShowHelp"))));
            this.hatchForeColor.ColorChanged += new System.EventHandler(this.HatchForeColorColorChanged);
            // 
            // helpProvider1
            // 
            resources.ApplyResources(this.helpProvider1, "helpProvider1");
            // 
            // ocOutline
            // 
            resources.ApplyResources(this.ocOutline, "ocOutline");
            this.helpProvider1.SetHelpKeyword(this.ocOutline, resources.GetString("ocOutline.HelpKeyword"));
            this.helpProvider1.SetHelpNavigator(this.ocOutline, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("ocOutline.HelpNavigator"))));
            this.helpProvider1.SetHelpString(this.ocOutline, resources.GetString("ocOutline.HelpString"));
            this.ocOutline.Name = "ocOutline";
            this.ocOutline.Pattern = null;
            this.helpProvider1.SetShowHelp(this.ocOutline, ((bool)(resources.GetObject("ocOutline.ShowHelp"))));
            this.ocOutline.OutlineChanged += new System.EventHandler(this.OcOutlineOutlineChanged);
            // 
            // ccPatterns
            // 
            resources.ApplyResources(this.ccPatterns, "ccPatterns");
            this.helpProvider1.SetHelpKeyword(this.ccPatterns, resources.GetString("ccPatterns.HelpKeyword"));
            this.helpProvider1.SetHelpNavigator(this.ccPatterns, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("ccPatterns.HelpNavigator"))));
            this.helpProvider1.SetHelpString(this.ccPatterns, resources.GetString("ccPatterns.HelpString"));
            this.ccPatterns.Name = "ccPatterns";
            this.helpProvider1.SetShowHelp(this.ccPatterns, ((bool)(resources.GetObject("ccPatterns.ShowHelp"))));
            // 
            // DetailedPolygonSymbolControl
            // 
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnAddToCustom);
            this.Controls.Add(this.ocOutline);
            this.Controls.Add(this.tabPatternProperties);
            this.Controls.Add(this.lblPatternType);
            this.Controls.Add(this.cmbPatternType);
            this.Controls.Add(this.ccPatterns);
            this.helpProvider1.SetHelpKeyword(this, resources.GetString("$this.HelpKeyword"));
            this.helpProvider1.SetHelpNavigator(this, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("$this.HelpNavigator"))));
            this.helpProvider1.SetHelpString(this, resources.GetString("$this.HelpString"));
            this.Name = "DetailedPolygonSymbolControl";
            this.helpProvider1.SetShowHelp(this, ((bool)(resources.GetObject("$this.ShowHelp"))));
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabPatternProperties.ResumeLayout(false);
            this.tabSimple.ResumeLayout(false);
            this.tabSimple.PerformLayout();
            this.tabPicture.ResumeLayout(false);
            this.tabPicture.PerformLayout();
            this.tabGradient.ResumeLayout(false);
            this.tabGradient.PerformLayout();
            this.tabHatch.ResumeLayout(false);
            this.tabHatch.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }


        private AngleControl angGradientAngle;
        private AngleControl angTileAngle;
        private Button btnAddToCustom;
        private Button btnLoadImage;
        private ColorButton cbColorSimple;
        private PatternCollectionControl ccPatterns;
        private CheckBox chkSmoothing;
        private ComboBox cmbGradientType;
        private ComboBox cmbHatchStyle;
        private ComboBox cmbPatternType;
        private ComboBox cmbScaleMode;
        private ComboBox cmbTileMode;
        private ComboBox cmbUnits;
        private DoubleBox dbxScaleX;
        private DoubleBox dbxScaleY;
        private GroupBox groupBox1;
        private ColorButton hatchBackColor;
        private RampSlider hatchBackOpacity;
        private ColorButton hatchForeColor;
        private RampSlider hatchForeOpacity;
        private HelpProvider helpProvider1;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label lblColorSimple;
        private Label lblEndColor;
        private Label lblHatchStyle;
        private Label lblPatternType;
        private Label lblPreview;
        private Label lblScaleMode;
        private Label lblStartColor;
        private Label lblTileMode;
        private Label lblUnits;
        private OutlineControl ocOutline;
        private RampSlider sldOpacitySimple;
        private GradientControl sliderGradient;
        private TabPage tabGradient;
        private TabPage tabHatch;
        private TabControl tabPatternProperties;
        private TabPage tabPicture;
        private TabPage tabSimple;
        private TextBox txtImage;

        #endregion

        private Button btnImportSymbol;
        private Button btnSaveSymbol;
    }
}
