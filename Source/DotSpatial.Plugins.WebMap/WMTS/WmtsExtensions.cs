﻿using BruTile;
using BruTile.Web;
using BruTile.Wmts;
using BruTile.Wmts.Generated;
using DotSpatial.Projections;
using GeoAPI.Geometries;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Serialization;

namespace DotSpatial.Plugins.WebMap
{
    public static class WmtsExtensions
    {
        public static Capabilities GetCapabilities(string capabilitiesUrl)
        {
            Capabilities capabilities = null;
            try
            {
                var myRequest = WebRequest.Create(capabilitiesUrl);
                using (var myResponse = myRequest.GetResponse())
                using (var stream = myResponse.GetResponseStream())
                {
                    capabilities = GetCapabilities( stream);
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
            }
            return capabilities;
        }
       
        public static Capabilities GetCapabilities(Stream stream)
        {
            Capabilities capabilities = null;
            if (stream != null)
            {
                try
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(Capabilities));
                    using (StreamReader textReader = new StreamReader(stream))
                    {
                        capabilities = (Capabilities)xmlSerializer.Deserialize(textReader);
                    }
                }
                catch (Exception e)
                {
                    Trace.WriteLine(e);
                }
            }
            return capabilities;
        }
        public static TileMatrixSet GetTileMatrixSet(this Capabilities capabilities, string tileMatrixSetName)
        {
            TileMatrixSet tileMatrixSet = capabilities.Contents.TileMatrixSet.FirstOrDefault(x => x is TileMatrixSet && x.Identifier.Value == tileMatrixSetName) as TileMatrixSet;
            return tileMatrixSet;
        }
        public static Extent GetExtent(this LayerType layerType)
        {
            if (layerType != null)
            {
                if (layerType.Items?.Length > 0)
                {
                    bool ret0 = layerType.Items[0].UpperCorner.GetCoordinate(out double xMax, out double yMax);
                    bool ret1 = layerType.Items[0].LowerCorner.GetCoordinate(out double xMin, out double yMin);
                    if (ret0 && ret1)
                    {
                        return new BruTile.Extent(xMin, yMin, xMax, yMax);
                    }
                }
            }
            return new BruTile.Extent();
        }
        public static Coordinate GetCoordinate(this string coordStr)
        {
            Coordinate coordinate = null;
            bool ret = coordStr.GetCoordinate(out double x, out double y);
            if (ret)
            {
                coordinate = new Coordinate(x, y);
            }
            return coordinate;
        }
        public static bool GetCoordinate(this string coordStr, out double x, out double y, string splitStr = " ")
        {
            string[] array = coordStr.Split(new string[] { splitStr }, StringSplitOptions.RemoveEmptyEntries);
            bool ret0 = double.TryParse(array[0], out x);
            bool ret1 = double.TryParse(array[1], out y);
            bool ret = ret0 && ret1;
            return ret;
        }
        public static ResourceUrl GetTileResourceUrl(this LayerType layerType)
        {
            ResourceUrl resourceUrl = null;
            URLTemplateType templateType = layerType.ResourceURL?.FirstOrDefault(x => x.resourceType == URLTemplateTypeResourceType.tile);
            if (templateType != null)
            {
                resourceUrl = new ResourceUrl()
                {
                    Template = templateType.template,
                    Format = templateType.format,
                    ResourceType = templateType.resourceType
                };
            }
            return resourceUrl;
        }
        public static Style GetDefaultStyle(this LayerType layerType, string style)
        {
            Style style0 = layerType.Style?.FirstOrDefault(x => x.Identifier.Value == style);
            return style0;
        }
        public static int GetEpsg(this TileMatrixSet tileMatrixSet)
        {
            int epsg = -1;
            string supportedCRS = tileMatrixSet.SupportedCRS;
            if (!string.IsNullOrEmpty(supportedCRS))
            {
                supportedCRS = supportedCRS.Replace("urn:ogc:def:crs:", "");
                string[] array = supportedCRS.Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries);
                if (array.Length == 2 && array[0] == "EPSG")
                {
                    int.TryParse(array[1], out epsg);
                }
            }
            return epsg;
        }
        public static Coordinate GetTopLeftCorner(this TileMatrixSet tileMatrixSet, bool useLatLon)
        {
            Coordinate topLeftCorner = null;
            if (tileMatrixSet.TileMatrix?.Length > 0)
            {
                var coordStr = tileMatrixSet.TileMatrix[0].TopLeftCorner;
                string[] array = coordStr.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                bool ret = false;
                double x, y;
                if (useLatLon)
                {
                    ret = GetCoordinate(coordStr, out y, out x);
                }
                else
                {
                    ret = GetCoordinate(coordStr, out x, out y);
                }
                if (ret)
                {
                    topLeftCorner = new Coordinate(x, y);
                }
            }
            return topLeftCorner;
        }
        public static ProjectionInfo ToProjectionInfo(this string crs)
        {
            ProjectionInfo projectionInfo=null;
            try
            {
                if (!string.IsNullOrEmpty(crs))
                {
                    var supportedCRS = crs.Replace("urn:ogc:def:crs:", "");
                    string[] array = supportedCRS.Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries);
                    if (array.Length == 2 && array[0] == "EPSG")
                    {
                        if (int.TryParse(array[1], out int epsg))
                        {
                            projectionInfo = ProjectionInfo.FromEpsgCode(epsg); 
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
            }
            return projectionInfo;
        }
    }
}
