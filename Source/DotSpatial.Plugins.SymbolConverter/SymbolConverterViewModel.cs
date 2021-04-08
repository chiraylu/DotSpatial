using DotSpatial.Controls;
using DotSpatial.Symbology;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DotSpatial.Plugins.SymbolConverter
{
    /// <summary>
    /// 符号转换视图模型
    /// </summary>
    public class SymbolConverterViewModel : NotifyClass
    {
        IMap Map { get; }
        private double _pixelSize;
        /// <summary>
        /// 像素大小
        /// </summary>
        public double PixelSize
        {
            get { return _pixelSize; }
            set { SetProperty(ref _pixelSize, value, nameof(PixelSize)); }
        }
        private double _worldSize;
        /// <summary>
        /// 世界大小
        /// </summary>
        public double WorldSize
        {
            get { return _worldSize; }
            set { SetProperty(ref _worldSize, value, nameof(WorldSize)); }
        }
        /// <summary>
        /// 图层信息
        /// </summary>
        public BindingList<LayerInfo> LayerInfos { get; }
        public SymbolConverterViewModel(IMap map)
        {
            Map = map;
            LayerInfos = new BindingList<LayerInfo>();
            var featureLayers = Map.MapFrame.GetAllFeatureLayers();
            featureLayers.Sort(CompareLegendText);
            foreach (var item in featureLayers)
            {
                LayerInfo layerInfo = new LayerInfo(item);
                LayerInfos.Add(layerInfo);
            }
        }

        private int CompareLegendText(IFeatureLayer x, IFeatureLayer y)
        {
            return x.LegendText.CompareTo(y.LegendText);
        }

        private void ConvertSymbolizer(IPointSymbolizer symbolizer, double ratio)
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
        /// <summary>
        /// 转换
        /// </summary>
        /// <returns>成功返回null，反之返回错误信息</returns>
        public string Convert()
        {
            string error = null;
            if (WorldSize <= 0)
            {
                error = "世界大小必须大于0";
                return error;
            }
            if (PixelSize <= 0)
            {
                error = "像素大小必须大于0";
                return error;
            }

            var ratio = WorldSize / PixelSize;
            foreach (var item in LayerInfos)
            {
                if (!item.IsChecked)
                {
                    continue;
                }
                if (item.Layer is IMapPointLayer pointLayer)
                {
                    foreach (var category in pointLayer.Symbology.Categories)
                    {
                        ConvertSymbolizer(category.Symbolizer, ratio);
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
            return error;
        }
    }
}
