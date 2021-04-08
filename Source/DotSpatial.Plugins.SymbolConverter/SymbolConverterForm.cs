using DotSpatial.Controls;
using DotSpatial.Symbology;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DotSpatial.Plugins.SymbolConverter
{
    public partial class SymbolConverterForm : Form
    {
        SymbolConverterViewModel ViewModel { get; }
        public SymbolConverterForm(IMap map)
        {
            InitializeComponent();
            Load += SymbolConverterForm_Load;
            ViewModel = new SymbolConverterViewModel(map);
        }

        private void SymbolConverterForm_Load(object sender, EventArgs e)
        {
            pixelNUDown.DataBindings.Add("Value", ViewModel, nameof(ViewModel.PixelSize), false, DataSourceUpdateMode.OnPropertyChanged);
            worldNUDown.DataBindings.Add("Value", ViewModel, nameof(ViewModel.WorldSize), false, DataSourceUpdateMode.OnPropertyChanged);
            dataGridView1.DataSource = ViewModel.LayerInfos;
            foreach (DataGridViewColumn item in dataGridView1.Columns)
            {
                if (item.Name == "Layer")
                {
                    item.Visible = false;
                }
            }
        }

        private void convertBtn_Click(object sender, EventArgs e)
        {
            string error = ViewModel.Convert();
            if (string.IsNullOrEmpty(error))
            {
                MessageBox.Show(this, "ok");
            }
            else
            {
                MessageBox.Show(this, $"转换失败，{error}");
            }
        }
    }
}
