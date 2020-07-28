// Copyright (c) DotSpatial Team. All rights reserved.
// Licensed under the MIT license. See License.txt file in the project root for full license information.

using DotSpatial.Data;
using GeoAPI.Geometries;

namespace DotSpatial.Plugins.ShapeEditor
{
    public class SnapInfo
    {
        public IFeature Feature { get; set; }
        public SnapMode SnapMode { get; set; }
        public Coordinate Coordinate { get; set; }
    }
}