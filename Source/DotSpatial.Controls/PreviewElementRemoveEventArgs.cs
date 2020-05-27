// Copyright (c) DotSpatial Team. All rights reserved.
// Licensed under the MIT license. See License.txt file in the project root for full license information.

using System;

namespace DotSpatial.Controls
{
    public class PreviewElementRemoveEventArgs : EventArgs
    {
        public bool Handled { get; set; }
        public LayoutElement LayoutElement { get; set; }
        public PreviewElementRemoveEventArgs(LayoutElement layoutElement, bool handled)
        {
            LayoutElement = layoutElement;
            Handled = handled;
        }
    }
}