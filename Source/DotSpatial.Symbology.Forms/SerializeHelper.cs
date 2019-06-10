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
        public static void Save<T>(T t,string filter)
        {
            SaveFileDialog dg = new SaveFileDialog()
            {
                Filter = filter
            };
            if (dg.ShowDialog() == DialogResult.OK)
            {
                XmlSerializer xmlSerializer = new XmlSerializer();
                string value = xmlSerializer.Serialize(t);
                File.WriteAllText(dg.FileName, value);
            }
        }
        public static void SaveSymbolizer(IFeatureSymbolizer symbolizer)
        {
            Save(symbolizer, "*.dsst|*.dsst");
        }
        public static T Open<T>(string filter,string title=null)
        {
            T t = default;
            OpenFileDialog dg = new OpenFileDialog()
            {
                Filter = filter
            };
            if (!string.IsNullOrEmpty(title))
            {
                dg.Title = title;
            }
            if (dg.ShowDialog() == DialogResult.OK)
            {
                XmlDeserializer ds = new XmlDeserializer();
                string value = File.ReadAllText(dg.FileName);
                t = ds.Deserialize<T>(value);
            }
            return t;
        }
        public static IFeatureSymbolizer OpenSymbolizer()
        {
            IFeatureSymbolizer featureSymbolizer = Open<IFeatureSymbolizer>("*.dsst|*.dsst");
            return featureSymbolizer;
        }

        public static IFeatureScheme OpenFeatureScheme()
        {
            IFeatureScheme featureScheme = Open<IFeatureScheme>("*.dsly|*.dsly");
            return featureScheme;
        }

        public static void SaveFeatureScheme(IFeatureScheme newScheme)
        {
            Save(newScheme, "*.dsly|*.dsly");
        }
    }
}
