// Copyright (c) DotSpatial Team. All rights reserved.
// Licensed under the MIT license. See License.txt file in the project root for full license information.

namespace DotSpatial.Data
{
    public class FeatureRemovedEventArgs : FeatureEventArgs
    {
        public int Fid { get; set; }
        public FeatureRemovedEventArgs(IFeature feature, int fid) : base(feature)
        {
            Fid = fid;
        }
    }
}