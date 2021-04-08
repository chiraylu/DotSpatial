using DotSpatial.Controls;
using DotSpatial.Symbology;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotSpatial.Plugins.SymbolConverter
{
    /// <summary>
    /// 图层信息
    /// </summary>
    public class LayerInfo : NotifyClass
    {
        private bool _isChecked = true;
        /// <summary>
        /// 是否选择
        /// </summary>
        [Column("选择")]
        public bool IsChecked
        {
            get { return _isChecked; }
            set { SetProperty(ref _isChecked, value, nameof(IsChecked)); }
        }
        /// <summary>
        /// 图层
        /// </summary>
        [Display(AutoGenerateField = false)]
        public IFeatureLayer Layer { get; }
        /// <summary>
        /// 图层名
        /// </summary>
        [Column("图层")]
        public string Name => Layer.LegendText;
        public LayerInfo(IFeatureLayer _layer)
        {
            Layer = _layer;
        }
    }
}