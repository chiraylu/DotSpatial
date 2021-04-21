﻿// Copyright (c) DotSpatial Team. All rights reserved.
// Licensed under the MIT license. See License.txt file in the project root for full license information.

using System;
using System.Drawing;
using GeoAPI.Geometries;

namespace DotSpatial.Plugins.WebMap.Tiling
{
    /// <summary>
    /// Tiles can be used to return the tiles that the provider returned.
    /// </summary>
    public class Tiles : IDisposable
    {
        #region  Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Tiles"/> class.
        /// </summary>
        /// <param name="zoom">zoom</param>
        /// <param name="bitmaps">An array of bitmaps.</param>
        /// <param name="topLeftTile">The top left tile.</param>
        /// <param name="bottomRightTile">The bottom right tile.</param>
        public Tiles(int zoom, Bitmap[,] bitmaps, Envelope topLeftTile, Envelope bottomRightTile)
        {
            Zoom = zoom;
            BottomRightTile = bottomRightTile;
            TopLeftTile = topLeftTile;
            Bitmaps = bitmaps;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets an array of bitmaps.
        /// </summary>
        public Bitmap[,] Bitmaps { get; private set; }

        /// <summary>
        /// Gets the bottom right tile.
        /// </summary>
        public Envelope BottomRightTile { get; private set; }

        /// <summary>
        /// Gets the top left tile.
        /// </summary>
        public Envelope TopLeftTile { get; private set; }
        public int Zoom { get; private set; }

        public void Dispose()
        {
            if (Bitmaps != null && Bitmaps.Length > 0)
            {
                for (var y = 0; y < Bitmaps.GetLength(1); y++)
                {
                    for (var x = 0; x < Bitmaps.GetLength(0); x++)
                    {
                        if (Bitmaps[x, y] != null)
                        {
                            Bitmaps[x, y].Dispose();
                            Bitmaps[x, y] = null;
                        }
                    }
                }
                Bitmaps = null;
            }
        }

        #endregion
    }
}