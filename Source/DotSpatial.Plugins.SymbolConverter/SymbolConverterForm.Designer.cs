
namespace DotSpatial.Plugins.SymbolConverter
{
    partial class SymbolConverterForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.pixelNUDown = new System.Windows.Forms.NumericUpDown();
            this.worldNUDown = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.convertBtn = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pixelNUDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.worldNUDown)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "像素大小";
            // 
            // pixelNUDown
            // 
            this.pixelNUDown.Location = new System.Drawing.Point(71, 21);
            this.pixelNUDown.Name = "pixelNUDown";
            this.pixelNUDown.Size = new System.Drawing.Size(157, 21);
            this.pixelNUDown.TabIndex = 1;
            // 
            // worldNUDown
            // 
            this.worldNUDown.Location = new System.Drawing.Point(71, 48);
            this.worldNUDown.Name = "worldNUDown";
            this.worldNUDown.Size = new System.Drawing.Size(157, 21);
            this.worldNUDown.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 50);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "世界大小";
            // 
            // convertBtn
            // 
            this.convertBtn.Location = new System.Drawing.Point(91, 179);
            this.convertBtn.Name = "convertBtn";
            this.convertBtn.Size = new System.Drawing.Size(75, 23);
            this.convertBtn.TabIndex = 4;
            this.convertBtn.Text = "转换";
            this.convertBtn.UseVisualStyleBackColor = true;
            this.convertBtn.Click += new System.EventHandler(this.convertBtn_Click);
            // 
            // SymbolConverterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(255, 214);
            this.Controls.Add(this.convertBtn);
            this.Controls.Add(this.worldNUDown);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.pixelNUDown);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "SymbolConverterForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "符号转换";
            ((System.ComponentModel.ISupportInitialize)(this.pixelNUDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.worldNUDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown pixelNUDown;
        private System.Windows.Forms.NumericUpDown worldNUDown;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button convertBtn;
    }
}