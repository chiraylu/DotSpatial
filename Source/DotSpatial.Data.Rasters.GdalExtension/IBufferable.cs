// Copyright (c) DotSpatial Team. All rights reserved.
// Licensed under the MIT license. See License.txt file in the project root for full license information.

namespace DotSpatial.Data.Rasters.GdalExtension
{
    public interface IBufferable
    {
        /// <summary>
        /// 缓存
        /// </summary>
        ImageBuffer ImageBuffer { get; set; }
    }
}