using DotSpatial.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DotSpatial.Symbology.Forms
{
    public static class SerializeHelper
    {
        public static void SaveSymbolizer(IFeatureSymbolizer symbolizer)
        {
            SaveFileDialog dg = new SaveFileDialog()
            {
                Filter = "*.dsst|*.dsst"
            };
            if (dg.ShowDialog() == DialogResult.OK)
            {
                XmlSerializer xmlSerializer = new XmlSerializer();
                string value = xmlSerializer.Serialize(symbolizer);
                File.WriteAllText(dg.FileName, value);
            }
        }

        public static IFeatureSymbolizer OpenSymbolizer()
        {
            IFeatureSymbolizer featureSymbolizer = null;
            OpenFileDialog dg = new OpenFileDialog()
            {
                Filter = "*.dsst|*.dsst"
            };
            if (dg.ShowDialog() == DialogResult.OK)
            {
                XmlDeserializer ds = new XmlDeserializer();
                string value = File.ReadAllText(dg.FileName);
                IFeatureSymbolizer ps = ds.Deserialize<IFeatureSymbolizer>(value);
                if (ps != null)
                {
                    featureSymbolizer = ps;
                }
            }
            return featureSymbolizer;
        }
    }
}
