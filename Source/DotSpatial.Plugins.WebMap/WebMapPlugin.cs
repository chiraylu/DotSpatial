// Copyright (c) DotSpatial Team. All rights reserved.
// Licensed under the MIT license. See License.txt file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using DotSpatial.Controls;
using DotSpatial.Controls.Header;
using DotSpatial.Data;
using DotSpatial.Plugins.WebMap.Properties;
using DotSpatial.Plugins.WebMap.Tiling;
using DotSpatial.Projections;
using DotSpatial.Symbology;
using GeoAPI.Geometries;

namespace DotSpatial.Plugins.WebMap
{
    /// <summary>
    /// The WebMapPlugin can be used to load web based layers.
    /// </summary>
    public class WebMapPlugin : Extension
    {
        #region Fields
        private const string Other = "Other";
        private const string Wmts = "Wmts";
        private const string StrKeyServiceDropDown = "kServiceDropDown";
        private readonly ProjectionInfo _webMercProj;
        private ServiceProvider _emptyProvider;
        private WebMapImageLayer _baseLayer;
        private SimpleActionItem _optionsAction;
        private DropDownActionItem _serviceDropDown;
        #endregion

        #region  Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="WebMapPlugin"/> class.
        /// </summary>
        public WebMapPlugin()
        {
            DeactivationAllowed = false;
            _webMercProj = ServiceProviderFactory.WebMercProj.Value;
        }

        #endregion

        #region Properties

        private ServiceProvider CurrentProvider => (ServiceProvider)_serviceDropDown.SelectedItem;

        #endregion

        #region Methods

        /// <summary>
        /// Initialize the DotSpatial plugin
        /// </summary>
        public override void Activate()
        {
            // Add Menu or Ribbon buttons.
            if (App.HeaderControl != null)
            {
                AddServiceDropDown(App.HeaderControl);

                _optionsAction = new SimpleActionItem("Configure", (sender, args) =>
                {
                    var p = CurrentProvider;
                    if (p == null) return;
                    var cf = p.Configure;
                    if (cf != null)
                    {
                        if (cf())
                        {
                            // Update map if configuration changed
                            EnableBasemapFetching(p);
                        }
                    }
                })
                {
                    Key = "kOptions",
                    RootKey = HeaderControl.HomeRootItemKey,
                    GroupCaption = Resources.Panel_Name,
                    Enabled = false,
                };
                App.HeaderControl.Add(_optionsAction);
                _serviceDropDown.SelectedValueChanged += ServiceSelected;
                _serviceDropDown.SelectedItem = _emptyProvider;

                // Add handlers for saving/restoring settings
                App.SerializationManager.Deserializing += SerializationManagerDeserializing;
                App.SerializationManager.NewProjectCreated += SerializationManagerNewProject;

                base.Activate();
            }
        }

        /// <summary>
        /// Fires when the plugin should become inactive
        /// </summary>
        public override void Deactivate()
        {
            App.HeaderControl.RemoveAll();

            DisableBasemapLayer();

            // remove handlers for saving/restoring settings
            App.SerializationManager.Deserializing -= SerializationManagerDeserializing;
            App.SerializationManager.NewProjectCreated -= SerializationManagerNewProject;

            base.Deactivate();
        }

        private void AddBasemapLayerToMap()
        {
            if (!InsertBaseMapLayer(App.Map.MapFrame))
            {
                App.Map.Layers.Add(_baseLayer);
            }
        }

        private void AddServiceDropDown(IHeaderControl header)
        {
            if (header == null)
            {
                return;
            }
            _serviceDropDown = new DropDownActionItem
            {
                Key = StrKeyServiceDropDown,
                RootKey = HeaderControl.HomeRootItemKey,
                Width = 145,
                AllowEditingText = false,
                ToolTipText = Resources.Service_Box_ToolTip,
                GroupCaption = Resources.Panel_Name
            };

            // "None" provider
            _emptyProvider = new ServiceProvider(Resources.None);
            _serviceDropDown.Items.Add(_emptyProvider);

            // Default providers
            _serviceDropDown.Items.AddRange(ServiceProviderFactory.GetDefaultServiceProviders());

            _serviceDropDown.Items.Add(ServiceProviderFactory.Create("map", "http://someservice/WMTS/1.0.0/WMTSCapabilities.xml"));

            // "Other" provider
            _serviceDropDown.Items.Add(ServiceProviderFactory.Create(Other));

            // Add it to the Header
            header.Add(_serviceDropDown);
        }

        private void BaseMapLayerRemoveItem(object sender, EventArgs e)
        {
            if (_baseLayer != null)
            {
                _baseLayer.RemoveItem -= BaseMapLayerRemoveItem;
            }

            _baseLayer = null;
            _serviceDropDown.SelectedItem = _emptyProvider;
        }

        private void DisableBasemapLayer()
        {
            RemoveBasemapLayer(_baseLayer);

            _optionsAction.Enabled = false;
            _baseLayer = null;
        }

        private void EnableBasemapFetching(ServiceProvider provider)
        {
            // Zoom out as much as possible if there are no other layers.
            // reproject any existing layer in non-webMercator projection.
            if (App.Map.Layers.Count == 0)
            {
                // special case when there are no other layers in the map. Set map projection to WebMercator and zoom to max ext.
                if (App.Map.MapFrame.Projection == null)
                {
                    App.Map.MapFrame.Projection = _webMercProj;
                }
                else
                {
                    if (App.Map.MapFrame.Projection != _webMercProj)
                    {
                        App.Map.MapFrame.ReprojectMapFrame(_webMercProj);
                    }
                }
            }
            else if (!App.Map.Projection.Equals(_webMercProj))
            {
                // get original view extents
                //App.ProgressHandler.Progress(string.Empty, 0, "Reprojecting Map Layers...");
                //double[] viewExtentXy = { App.Map.ViewExtents.MinX, App.Map.ViewExtents.MinY, App.Map.ViewExtents.MaxX, App.Map.ViewExtents.MaxY };
                //double[] viewExtentZ = { 0.0, 0.0 };

                //// reproject view extents
                //Reproject.ReprojectPoints(viewExtentXy, viewExtentZ, App.Map.Projection, _webMercProj, 0, 2);

                //// set the new map extents
                //App.Map.ViewExtents = new Extent(viewExtentXy);

                //// if projection is not WebMercator - reproject all layers:
                //App.Map.MapFrame.ReprojectMapFrame(_webMercProj);

                //App.ProgressHandler.Progress(string.Empty, 0, "Loading Basemap...");
            }

            EnableBasemapLayer(provider);
        }

        private void EnableBasemapLayer(ServiceProvider serviceProvider)
        {
            if (_baseLayer == null)
            {
                // Need to first initialize and add the basemap layer synchronously (it will fail if done in another thread).
                var tempImageData = new InRamImageData(Resources.nodata, new Extent(1, 1, 2, 2));

                _baseLayer = new WebMapImageLayer()
                {
                    LegendText = Resources.Legend_Title,
                    Projection = App.Map.Projection
                };
                _baseLayer.WebMapName = serviceProvider.Name;
                if (serviceProvider is WmtsServiceProvider wmtsServiceProvider)
                {
                    _baseLayer.WebMapUrl = wmtsServiceProvider.CapabilitiesUrl;
                }
                _baseLayer.RemoveItem += BaseMapLayerRemoveItem;
                AddBasemapLayerToMap();
            }
            else
            {
                if(_baseLayer.WebMapName != serviceProvider.Name)
                {
                    _baseLayer.WebMapName = serviceProvider.Name;
                }
            }
        }

        private bool InsertBaseMapLayer(IMapGroup group)
        {
            for (var i = 0; i < group.Layers.Count; i++)
            {
                var layer = group.Layers[i];
                var childGroup = layer as IMapGroup;
                if (childGroup != null)
                {
                    var ins = InsertBaseMapLayer(childGroup);
                    if (ins) return true;
                }

                if (layer is IMapPointLayer || layer is IMapLineLayer)
                {
                    var grp = layer.GetParentItem() as IGroup;
                    if (grp != null)
                    {
                        grp.Insert(i, _baseLayer);
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Finds and removes the online basemap layer.
        /// </summary>
        /// <param name="layer">Name of the online basemap layer.</param>
        private void RemoveBasemapLayer(IMapLayer layer)
        {
            // attempt to remove from the top-level
            if (App.Map.Layers.Remove(layer)) return;

            // Remove from other groups if the user has moved it
            foreach (var group in App.Map.Layers.OfType<IMapGroup>())
            {
                group.Remove(layer);
            }
        }

        /// <summary>
        /// Deserializes the WebMap settings and loads the corresponding basemap.
        /// </summary>
        /// <param name="sender">Sender that raised the event.</param>
        /// <param name="e">The event args.</param>
        private void SerializationManagerDeserializing(object sender, SerializingEventArgs e)
        {
            try
            {
                if (_baseLayer != null)
                {
                    // disable BaseMap because we might be loading a project that doesn't have a basemap
                    DisableBasemapLayer();
                    _serviceDropDown.SelectedItem = _emptyProvider;
                }

                _baseLayer = (WebMapImageLayer)App.Map.MapFrame.GetAllLayers().FirstOrDefault(layer => layer.LegendText == Resources.Legend_Title);
                if (_baseLayer != null)
                {
                    //_baseLayer.Projection = _webMercProj; // changed by jany_(2015-07-09) set the projection because if it is not set we produce a cross thread exception when DotSpatial tries to show the projection dialog

                    // hack: need to set provider to original object, not a new one.
                    _serviceDropDown.SelectedItem = _serviceDropDown.Items.OfType<ServiceProvider>().FirstOrDefault(p => p.Name.Equals(_baseLayer.WebMapName, StringComparison.InvariantCultureIgnoreCase));
                    var pp = CurrentProvider;
                    if (pp != null)
                    {
                        EnableBasemapFetching(pp);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Print(ex.StackTrace);
            }
        }

        /// <summary>
        /// Deactivates the WebMap when a new project is created.
        /// </summary>
        /// <param name="sender">Sender that raised the event.</param>
        /// <param name="e">The event args.</param>
        private void SerializationManagerNewProject(object sender, SerializingEventArgs e)
        {
            if (_baseLayer == null) return;
            DisableBasemapLayer();
            _serviceDropDown.SelectedItem = _emptyProvider;
        }

        private void ServiceSelected(object sender, SelectedValueChangedEventArgs e)
        {
            var p = CurrentProvider;
            if (p == null || p.Name == Resources.None)
            {
                DisableBasemapLayer();
            }
            else
            {
                _optionsAction.Enabled = p.Configure != null;

                if (p.NeedConfigure)
                {
                    p.Configure?.Invoke();
                }

                EnableBasemapFetching(p);
            }
        }
        #endregion
    }
}