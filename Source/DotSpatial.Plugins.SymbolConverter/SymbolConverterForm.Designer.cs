
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
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.pixelNUDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.worldNUDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 29);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "像素大小";
            // 
            // pixelNUDown
            // 
            this.pixelNUDown.Location = new System.Drawing.Point(95, 26);
            this.pixelNUDown.Margin = new System.Windows.Forms.Padding(4);
            this.pixelNUDown.Name = "pixelNUDown";
            this.pixelNUDown.Size = new System.Drawing.Size(209, 25);
            this.pixelNUDown.TabIndex = 1;
            // 
            // worldNUDown
            // 
            this.worldNUDown.Location = new System.Drawing.Point(95, 60);
            this.worldNUDown.Margin = new System.Windows.Forms.Padding(4);
            this.worldNUDown.Name = "worldNUDown";
            this.worldNUDown.Size = new System.Drawing.Size(209, 25);
            this.worldNUDown.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 62);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(67, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "世界大小";
            // 
            // convertBtn
            // 
            this.convertBtn.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.convertBtn.Location = new System.Drawing.Point(157, 431);
            this.convertBtn.Margin = new System.Windows.Forms.Padding(4);
            this.convertBtn.Name = "convertBtn";
            this.convertBtn.Size = new System.Drawing.Size(100, 29);
            this.convertBtn.TabIndex = 4;
            this.convertBtn.Text = "转换";
            this.convertBtn.UseVisualStyleBackColor = true;
            this.convertBtn.Click += new System.EventHandler(this.convertBtn_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(12, 92);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersWidth = 51;
            this.dataGridView1.RowTemplate.Height = 27;
            this.dataGridView1.Size = new System.Drawing.Size(388, 332);
            this.dataGridView1.TabIndex = 5;
            // 
            // SymbolConverterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(412, 475);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.convertBtn);
            this.Controls.Add(this.worldNUDown);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.pixelNUDown);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "SymbolConverterForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "符号转换";
            ((System.ComponentModel.ISupportInitialize)(this.pixelNUDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.worldNUDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown pixelNUDown;
        private System.Windows.Forms.NumericUpDown worldNUDown;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button convertBtn;
        private System.Windows.Forms.DataGridView dataGridView1;
    }
}