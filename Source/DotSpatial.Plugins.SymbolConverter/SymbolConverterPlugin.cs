using DotSpatial.Controls;
using DotSpatial.Controls.Header;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotSpatial.Plugins.SymbolConverter
{
    public class SymbolConverterPlugin : Extension
    {
        #region Methods
        HeaderItem HeaderItem { get; set; }
        SymbolConverterForm Form { get; set; }
        /// <inheritdoc/>
        public override void Activate()
        {
            if (App.HeaderControl != null && HeaderItem == null)
            {
                HeaderItem = new SimpleActionItem("SymbolConverterKey", "SymbolConverter", SymbolConverterButtonClick)
                {
                    GroupCaption = "Symbol Converter",
                    RootKey = HeaderControl.HomeRootItemKey,
                    ShowInQuickAccessToolbar=true
                };
                App.HeaderControl.Add(HeaderItem);
                base.Activate();
            }
        }

        private void SymbolConverterButtonClick(object sender, EventArgs e)
        {
            Form = new SymbolConverterForm(App.Map);
            Form.ShowDialog();
        }

        /// <inheritdoc/>
        public override void Deactivate()
        {
            if (App.HeaderControl != null && HeaderItem != null)
            {
                App.HeaderControl.Remove(HeaderItem.Key);
            }
            base.Deactivate();
        }

        #endregion
    }
}
