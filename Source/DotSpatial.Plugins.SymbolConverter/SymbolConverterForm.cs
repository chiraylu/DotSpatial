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
        IMap Map { get; }
        public SymbolConverterForm(IMap map)
        {
            InitializeComponent();
            Map = map;
        }
        private void ConvertSymbolizer(IPointSymbolizer symbolizer,double ratio)
        {
            symbolizer.ScaleMode = ScaleMode.Geographic;
            symbolizer.Units = GraphicsUnit.World; 
            foreach (var symbol in symbolizer.Symbols)
            {
                var oldSize = symbol.Size;
                symbol.Size = new Size2D(oldSize.Width * ratio, oldSize.Height * ratio);
                symbol.Offset.X *= ratio;
                symbol.Offset.Y *= ratio;
                if (symbol is IOutlinedSymbol outlinedSymbol)
                {
                    outlinedSymbol.OutlineWidth *= ratio;
                }
            }
        }
        private void convertBtn_Click(object sender, EventArgs e)
        {
            var featureLayers = Map.MapFrame.GetAllFeatureLayers();
            double pixelSize = (double)pixelNUDown.Value;
            double worldSize = (double)worldNUDown.Value;
            var ratio = worldSize / pixelSize;
            foreach (var item in featureLayers)
            {
                if (item is IMapPointLayer pointLayer)
                {
                    foreach (var category in pointLayer.Symbology.Categories)
                    {
                        ConvertSymbolizer(category.Symbolizer,ratio);
                        ConvertSymbolizer(category.SelectionSymbolizer, ratio);
                    }
                    pointLayer.AssignFastDrawnStates();//重新计算符号
                }
                //else if (item is IMapLineLayer lineLayer)
                //{
                //    foreach (var category in pointLayer.Symbology.Categories)
                //    {
                //        var symbolizer = category.Symbolizer;
                //        symbolizer.ScaleMode = ScaleMode.Geographic;
                //        symbolizer.Units = GraphicsUnit.World;
                //        foreach (var symbol in symbolizer.Symbols)
                //        {
                //            var oldSize = symbol.Size;
                //            symbol.Size = new Size2D(oldSize.Width * ratio, oldSize.Height * ratio);
                //            if (symbol is IOutlinedSymbol outlinedSymbol)
                //            {
                //                outlinedSymbol.OutlineWidth *= ratio;
                //            }
                //        }
                //    }
                //    pointLayer.AssignFastDrawnStates();//重新计算符号
                //}
            }
            MessageBox.Show(this, "ok");
        }
    }
}
