// Copyright (c) DotSpatial Team. All rights reserved.
// Licensed under the MIT license. See License.txt file in the project root for full license information.

namespace DotSpatial.Data
{
    public class PreviewRemoveFeatureEventArgs : FeatureEventArgs
    {
        public bool Handled { get; set; }
        public PreviewRemoveFeatureEventArgs(IFeature feature) : base(feature)
        {
        }
    }
}